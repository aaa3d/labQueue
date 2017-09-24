using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace labQueue
{
    class DataBase
    {
        private static SqlConnection m_conn = null;
        private static Object thisLock = new Object();

        public class settings
        {
            /* доступные настройки: имя-пароль VOIP юзера, сетевой адрес VOIP сервера, сетевой адрес локальной машины (надо, если несколько интерфейсов)
             * из файла .set берется только путь к таксе.
             */

            public static String getSharedSettingValue(String setting_name) {
                String sql = "SELECT value1 FROM SETTINGS WHERE SETT_NAME='" + setting_name + "' AND COMP_NAME='" + "SHARED_SETTING" + "'";
                String _value = DataBase.mssqlReadString(sql);

                return _value;
            }

            public static void saveSharedSettingValue(String setting_name, String value) {
                String sql = "SELECT value1 FROM SETTINGS WHERE SETT_NAME='" + setting_name + "' AND COMP_NAME='" + "SHARED_SETTING" + "'";
                String v1 = DataBase.mssqlReadString(sql);
                if (v1 != null) {
                    if (v1 != value) {
                        sql = "UPDATE SETTINGS SET VALUE1='" + value.ToString() + "' WHERE SETT_NAME='" + setting_name + "' AND COMP_NAME='" + "SHARED_SETTING" + "'";
                        DataBase.mssqlExecuteSQL(sql);
                    }
                } else {
                    sql = "INSERT INTO SETTINGS (VALUE1, COMP_NAME, SETT_NAME) VALUES('" + value.ToString() + "', '" + "SHARED_SETTING" + "', '" + setting_name + "')";
                    DataBase.mssqlExecuteSQL(sql);
                }

            }


            public static String getSettingValue(String setting_name)
            {
                String sql = "SELECT value1 FROM SETTINGS WHERE SETT_NAME='"+setting_name+"' AND COMP_NAME='" + System.Net.Dns.GetHostName().ToString()+"'";
                String _value = DataBase.mssqlReadString(sql);

                return _value;
            }
            public static void saveSettingValue(String setting_name, String value)
            {
                String sql = "SELECT value1 FROM SETTINGS WHERE SETT_NAME='" + setting_name + "' AND COMP_NAME='" + System.Net.Dns.GetHostName().ToString() + "'";
                String v1 = DataBase.mssqlReadString(sql);
                if (v1 != null)
                {
                    if (v1 != value)
                    {
                        sql = "UPDATE SETTINGS SET VALUE1='" + value.ToString() + "' WHERE SETT_NAME='" + setting_name + "' AND COMP_NAME='" + System.Net.Dns.GetHostName().ToString() + "'";
                        DataBase.mssqlExecuteSQL(sql);
                    }
                }
                else
                {
                    sql = "INSERT INTO SETTINGS (VALUE1, COMP_NAME, SETT_NAME) VALUES('" + value.ToString() + "', '" + System.Net.Dns.GetHostName().ToString() + "', '" + setting_name + "')";
                    DataBase.mssqlExecuteSQL(sql);
                }

            }

            

           
        }

        private static String connString = "";

        public static SqlConnection mssqlGetConnection()
        {
            int ms_start = 0;
            ms_start = DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
            

            Stopwatch sWatch = new Stopwatch();
            sWatch.Start();
            
            //sta

            //lock (thisLock) 
            {
                //if (m_conn == null) 

                {
                    try {

                        if (connString == "")
                        {

                            String connectPath = "127.0.0.1";



                            connectPath = "192.168.100.1\\TAXA";

                            try
                            {
                                Microsoft.Win32.RegistryKey readKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("software\\Taxa\\Client");
                                if (readKey != null)
                                {
                                    string loadString = (string)readKey.GetValue("ConnectPath");
                                    readKey.Close();
                                    connectPath = loadString;
                                }
                            }
                            catch (Exception e)
                            {

                            }

                            connString = ConfigurationManager.AppSettings["ConnectionString"];


                            //connString = "Data Source=" + connectPath + ";Database=TAXA;Uid=taxaadmin;Pwd=654321;pooling=true;max pool size=5";
                            //                        connString = "Data Source=" + connectPath + ";Database=TAXA;Uid=taxaadmin;Pwd=654321;pooling=true;max pool size=20";
                        }
                        


                        SqlConnection conn = new SqlConnection(connString);
                        conn.Open();

                        


                        m_conn = conn;
                        // m_conn.Open();
                    } catch (Exception ex) {


                        //MessageBox.Show(ex.Message + "Укажите правильный адрес сервера базы данных");



                    }
                }
            }



            sWatch.Stop();
          //  Console.WriteLine("mssqlGetConnection: " + sWatch.ElapsedMilliseconds.ToString());
                //conn.Open();

          //      System.Console.WriteLine("mssqlGetConnection " + (DateTime.Now.Second * 1000 + DateTime.Now.Millisecond - ms_start).ToString() + "мсек");
                return m_conn;
            //}
        
        }

        public static int mssqlInsert(String table, String[] fields, object[] values)
        {
            //System.Console.WriteLine("mssqlInsert " + table);
            int result = 0;
            lock (thisLock)
            {
                SqlCommand cmd = null;
                SqlConnection conn = null;
                try
                {
                    if (fields.Length != values.Length)
                        return -1;
                    String field_names = "";
                    String value_names = "";

                    for (int i = 0; i < fields.Length; i++)
                    {
                        if (i != 0)
                        {
                            field_names += ", ";
                            value_names += ", ";
                        }

                        field_names += fields[i];
                        value_names += "@" + fields[i];
                    }

                    String sql = "INSERT INTO " + table + "(" + field_names + ") values (" + value_names + ")";




                    conn = mssqlGetConnection();
                    cmd = conn.CreateCommand();
                    cmd.CommandText = sql;

                    for (int i = 0; i < fields.Length; i++)
                    {
                        if (values[i] == null)
                            cmd.Parameters.AddWithValue("@" + fields[i], DBNull.Value);
                        else
                            cmd.Parameters.AddWithValue("@" + fields[i], values[i]);
                    }
                    cmd.ExecuteNonQuery();

                    result = 1;

                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                   // MessageBox.Show(ex.Message);
                    result = 0;
                }
                finally
                {
                    if (cmd != null)
                        cmd.Dispose();
                    if (conn != null)
                        conn.Close();
                }

            }

            return result;
        }

        public static int mssqlInsertOneReturnID(String table, String[] fields, object[] values)
        {
            //System.Console.WriteLine("mssqlInsert " + table);
            int result = 0;
            lock (thisLock)
            {
                SqlCommand cmd = null;
                SqlConnection conn = null;
                try
                {
                    if (fields.Length != values.Length)
                        return -1;
                    String field_names = "";
                    String value_names = "";

                    for (int i = 0; i < fields.Length; i++)
                    {
                        if (i != 0)
                        {
                            field_names += ", ";
                            value_names += ", ";
                        }

                        field_names += fields[i];
                        value_names += "@" + fields[i];
                    }

                    String sql = "INSERT INTO " + table + "(" + field_names + ") values (" + value_names + "); SELECT ID = SCOPE_IDENTITY()";




                    conn = mssqlGetConnection();
                    cmd = conn.CreateCommand();
                    cmd.CommandText = sql;

                    for (int i = 0; i < fields.Length; i++)
                    {
                        if (values[i] == null)
                            cmd.Parameters.AddWithValue("@" + fields[i], DBNull.Value);
                        else
                            cmd.Parameters.AddWithValue("@" + fields[i], values[i]);
                    }
                    int myReturnedID = int.Parse(cmd.ExecuteScalar().ToString());

                    result = myReturnedID;

                }
                catch (Exception ex)
                {
                   // MessageBox.Show(ex.Message);
                    result = 0;
                }
                finally
                {
                    if (cmd != null)
                        cmd.Dispose();
                    if (conn != null)
                        conn.Close();
                }

            }

            return result;
        }

        public static int mssqlUpdate(String table, String field, object value, String key_field, object key_value)
        {
            String[] fields = { field };
            object[] values = { value };
            return mssqlUpdate(table, fields, values, key_field, key_value);
        }

        public static int mssqlUpdate(String table, String[] fields, object[] values, String key_field, object key_value)
        {
            //System.Console.WriteLine("mssqlUpdate "+table);
            int result = 0;
            lock (thisLock)
            {
                SqlCommand cmd = null;
                SqlConnection conn = null;
                try
                {
                    if (fields.Length != values.Length)
                        return -1;
                    String fields_values = "";


                    for (int i = 0; i < fields.Length; i++)
                    {
                        if (i != 0)
                        {
                            fields_values += ", ";

                        }

                        fields_values += fields[i] + "=@" + fields[i];

                    }

                    String sql = "UPDATE " + table + " SET " + fields_values + " WHERE " + key_field + " = @" + key_field;


                    conn = mssqlGetConnection();
                    cmd = conn.CreateCommand();
                    cmd.CommandText = sql;

                    for (int i = 0; i < fields.Length; i++)
                    {
                        if (values[i] == null)
                            cmd.Parameters.AddWithValue("@" + fields[i], DBNull.Value);
                        else
                            cmd.Parameters.AddWithValue("@" + fields[i], values[i]);
                    }

                    cmd.Parameters.AddWithValue("@" + key_field, key_value);
                    cmd.ExecuteNonQuery();
                    result = 1;

                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                }
                finally
                {
                    if (cmd !=null)
                        cmd.Dispose();
                    if (conn != null)
                        conn.Close();
                }
            }

            return result;
        }


        public static int mssqlExecuteSQL(String sql)
        {
            //System.Console.WriteLine(sql);
            SqlTransaction transaction1 = null;
            SqlConnection conn = null;
            lock (thisLock)
            {
                SqlCommand cmd = null;
                try
                {
                    conn = mssqlGetConnection();
                    cmd = conn.CreateCommand();

                    transaction1 =
                        cmd.Connection.BeginTransaction(IsolationLevel.ReadCommitted);
                    //cmd.Connection = mssqlGetConnection();
                    cmd.Transaction = transaction1;

                    cmd.CommandText = sql;

                    cmd.ExecuteNonQuery();
                    transaction1.Commit();

                }
                catch (Exception ex)
                {
                    // if (transaction1!=null)
                    //     transaction1.Rollback();
                    //MessageBox.Show(ex.Message);
                    System.Console.WriteLine(DateTime.Now + ": SQL_EXCEPTION: " + ex.Message);
                }
                finally
                {
                    if (cmd != null)
                        cmd.Dispose();
                    if (conn != null)
                        conn.Close();
                }
            }

            return 0;
        }

        public static DataTable mssqlRead(String query, SqlParameter [] parameters )
        {
            //System.Console.WriteLine(query);
            System.Data.DataTable dt = new DataTable();
            SqlTransaction transaction1 = null;
            SqlConnection conn = null;
            lock (thisLock)
            {
                SqlCommand cmd = null;
                try
                {
                    //            using (
                    {
                        conn = mssqlGetConnection();
                        //                )
                        cmd = conn.CreateCommand();
                        transaction1 =
                            conn.BeginTransaction(IsolationLevel.ReadCommitted);
                        //cmd.Connection = mssqlGetConnection();
                        cmd.Transaction = transaction1;

                        cmd.CommandText = query;




                        //  cmd.CommandText += " with (NOLOCK) ";

                        if (parameters != null)
                        {
                            foreach (SqlParameter par in parameters)
                            {
                                cmd.Parameters.AddWithValue(par.ParameterName, par.Value);
                            }
                        }
                        SqlDataReader dataReader = cmd.ExecuteReader();

                        dt.Load(dataReader);

                        dataReader.Close();
                        transaction1.Commit();
                        cmd.Dispose();
                    }

                }
                //   conn.Close();
                catch (Exception ex)
                {
                    //   if (transaction1 != null)
                    //      transaction1.Rollback();
                    //MessageBox.Show(ex.Message);
                    System.Console.WriteLine(ex.Message);
                    System.Console.WriteLine(DateTime.Now+": SQL_EXCEPTION: " + ex.Message);
                }

                finally
                {
                    if (cmd != null)
                        cmd.Dispose();
                    if (conn != null)
                        conn.Close();
                }
            }
            


            return dt;
        }

        public static DataTable mssqlRead(String query)
        {

            return mssqlRead(query, null);
           
        }

        //возвращает строку  - для получения конкретного значения
        public static String mssqlReadString(String query) {
            //System.Console.WriteLine(query);
            String result = null;
            lock (thisLock) {
                SqlCommand cmd = null;
                SqlConnection conn = null;
                try
                {
                    conn = mssqlGetConnection();
                    cmd = conn.CreateCommand();

                    cmd.CommandText = query;
                    SqlDataReader dataReader = cmd.ExecuteReader();

                    if (dataReader.Read())
                    {
                        result = dataReader.GetString(0);
                    }

                    dataReader.Close();
                }

                finally
                {
                    if (cmd != null)
                        cmd.Dispose();
                    if (conn != null)
                        conn.Close();
                }
            }

                return result;
            
        }

        //возвращает int  - для получения конкретного значения
        public static int mssqlReadInt(String query)
        {
            //System.Console.WriteLine(query);
            int result = 0;
            lock (thisLock)
            {
                SqlCommand cmd = null;
                SqlConnection conn = null;
                try
                {
                    conn = mssqlGetConnection();
                    cmd = conn.CreateCommand();

                    cmd.CommandText = query;
                    SqlDataReader dataReader = cmd.ExecuteReader();

                    if (dataReader.Read())
                    {
                        result = dataReader.GetInt32(0);
                    }

                    dataReader.Close();
                }
                finally
                {
                    if (cmd != null)
                        cmd.Dispose();
                    if (conn != null)
                        conn.Close();
                }
            }

            return result;

        }


        //возвращает пару ключ-значение  - для получения конкретного значения
        public static KeyValuePair<int, String> mssqlReadKeyValue(String query)
        {
            KeyValuePair<Int32, String> result = new KeyValuePair<Int32, String>(0, ""); ;
            lock (thisLock)
            {
                SqlCommand cmd = null;
                SqlConnection conn = null;
                try
                {
                    cmd = conn.CreateCommand();

                    cmd.CommandText = query;
                    SqlDataReader dataReader = cmd.ExecuteReader();

                    if (dataReader.Read())
                    {
                        result = new KeyValuePair<int, string>(dataReader.GetInt32(0), dataReader.GetString(1));
                    }
                    dataReader.Close();

                }
            
                finally
                {
                    if (cmd != null)
                        cmd.Dispose();
                    if (conn != null)
                        conn.Close();
                }
            }
            
            return result;
        }


    }
}
