using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using labQueue.Models;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

namespace labQueue.Controllers
{
    public class HomeController : Controller
    {


        private void generateTalons(DateTime start_time,  int period_count, int talon_in_period)
        {
            DateTime dt = start_time;
            

            ArrayList talons = new ArrayList();
            for (int i = 0; i < period_count; i++)
            {
                for (int j = 0; j < talon_in_period; j++)
                {
                    
                    String[] keys = { "number", "period", "local_index", "status" };

                    object[] values = { (i* talon_in_period + j+1).ToString(), dt, (j+1).ToString(), 0 };

                    DataBase.mssqlInsert("talon", keys, values);
                   
                }
                dt = dt.AddMinutes(15);
            }

            

        }

        public ActionResult Index()
        {
            return View();
        }


        private void btnClicked(String btn_id)
        {
            if (btn_id == null)
                return;
            int period = 0;
            int.TryParse(btn_id, out period);

            DateTime btn_time = DateTime.Today.AddHours(8).AddMinutes(period * 15);
            DateTime current_time_period = DateTime.Now.AddMinutes(-15); //сдвижка текущего времени на отрезок

            DataRow nextTalon = null;

            if (btn_time < current_time_period)    //опаздал. выдать из резервных начиная с текущего периода до конца сегодняшнего дня
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("dt1", current_time_period));
                parameters.Add(new SqlParameter("dt2", DateTime.Today.AddDays(1)));
                DataTable dt = DataBase.mssqlRead("select * from Talon where period between @dt1 and @dt2 and status=0 and local_index in (19, 20) order by period, local_index", parameters.ToArray());
                if (dt != null)
                    if(dt.Rows.Count>0)
                    nextTalon = dt.Rows[0];
            }

            if ((btn_time > current_time_period) && (btn_time < current_time_period.AddMinutes(15)))    //вовремя. выдать по порядку из текущих свободных
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("dt1", btn_time));
                parameters.Add(new SqlParameter("dt2", btn_time.AddMinutes(15)));
                DataTable dt = DataBase.mssqlRead("select * from Talon where period between @dt1 and @dt2 and status=0 and not local_index in (19, 20) order by period, local_index", parameters.ToArray());
                if (dt != null)
                    if (dt.Rows.Count > 0)
                        nextTalon = dt.Rows[0];
            }

            if ((btn_time > current_time_period.AddMinutes(15)))    //рано. выдать по порядку в своем периоде из свободных
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("dt1", btn_time));
                parameters.Add(new SqlParameter("dt2", btn_time.AddMinutes(15)));
                DataTable dt = DataBase.mssqlRead("select * from Talon where period between @dt1 and @dt2 and status=0 and not local_index in (19, 20) order by period, local_index", parameters.ToArray());
                if (dt != null)
                    if (dt.Rows.Count > 0)
                        nextTalon = dt.Rows[0];
            }

            
            /*
             * если талон не нашелся - выдать сообщение (свободных талонов нет, чтото пошло не так)
             * 
             * талон нашел. 
             * сменить ему статус.
             * распечатать
             * после этого - вывести обновленую таблицу-view, при этом в табличе цветом выделить этот последний выданный талон
             * 
             */ 
                
            if (nextTalon != null)
            {
                DataBase.mssqlExecuteSQL("update Talon set status=1 where id = " + nextTalon["id"].ToString());
            }



        }


        public ActionResult About(String subaction, String subaction_param)
        {
             
                if ((subaction != null)  && (subaction.CompareTo("btn_click") == 0))
                {
                    btnClicked(subaction_param);
                }



            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("dt1", DateTime.Today));
            parameters.Add(new SqlParameter("dt2", DateTime.Today.AddDays(1)));
            DataTable dt = DataBase.mssqlRead("select * from Talon where period between @dt1 and @dt2 order by period, local_index", parameters.ToArray());
            if (dt.Rows.Count < 1)
            {
                generateTalons(DateTime.Today.AddHours(8), 36, 20);
                dt = DataBase.mssqlRead("select * from Talon where period between @dt1 and @dt2 order by period, local_index", parameters.ToArray());
            }
            //динамически создавать массив талонов на текущий день

            if (dt.Rows.Count < 1)
            {
                ViewBag.Message = "Ошибка формирования талонов на день";
            }
            else
                ViewBag.Message = "Талонов на день: "+ dt.Rows.Count;


            parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("dt1", DateTime.Today));
            parameters.Add(new SqlParameter("dt2", DateTime.Today.AddDays(1)));
            DataTable dt_active_talons = DataBase.mssqlRead("select * from Talon where period between @dt1 and @dt2 and status in (1,2) order by status desc, laborant, period, local_index", parameters.ToArray());






            ViewBag.Message17 = "Your application description page 17.";
            ViewBag.talons = dt.Rows;
            ViewBag.active_talons = dt_active_talons.Rows;
            ViewBag.timePeriods = 36;
            ViewBag.talonsInPeriod = 20;


            return View();
        }

        public ActionResult Contact()
        {
            
           

            return View();
        }
    }
}