using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data;

namespace labQueue.Controllers
{
    public class LaboratoryController : Controller
    {
        // GET: Laboratory
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost] //обработка сигналов от службы работы с COM портом
        public ActionResult DeviceApi(String signal_sender, String signal_name, String signal_param)
        {
            //сигнал от лаборанта - вызов следующего
            //signal_param - номер лаборанта
            //signal_sender - код лаборатории (утсройства с кнопками)
            //signal_name - в общем случае btn_click

            //найти текущий талон на лаборанте
            //отметить его как закрытый
            //найти следующий ожидающий талон
            //задать ему статус активен, номер лаборанта
            //вернуть успех
            //обновить страницы у регистратора и табло

            int laborant = 0;
            int.TryParse(signal_param, out laborant);

            if (laborant == 0)
                return Json(new { result = "ERROR", result_descr = "Неверный номер лаборанта" }, JsonRequestBehavior.AllowGet); ;


            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("dt1", DateTime.Today));
            parameters.Add(new SqlParameter("laborant", laborant));
            DataBase.mssqlExecuteSQL("update Talon  set status = 3 where period > @dt1 and status=2 and laborant=@laborant ", parameters.ToArray());


            parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("dt1", DateTime.Today));
            DataTable dt = DataBase.mssqlRead("select * from Talon  where period > @dt1 and status=1 order by period, local_index", parameters.ToArray());
            if (dt != null)
                if (dt.Rows.Count > 0) {
                    //есть следующий талон. дать ему новый статус
                    parameters = new List<SqlParameter>();
                    parameters.Add(new SqlParameter("dt1", DateTime.Today));
                    parameters.Add(new SqlParameter("laborant", laborant));

                    String [] fields = { "status", "laborant"};
                    Object [] values = { "2", laborant };

                    DataBase.mssqlUpdate("Talon", fields, values, "id", dt.Rows[0]["id"]);
                }

            return Json(new { result = "INFO", result_descr = "Запрос обработан" },JsonRequestBehavior.AllowGet);
        }

        [HttpPost] //обработка ajax запросов со стороны WEB приложения
        public ActionResult AjaxApi(String signal_name, String signal_param)
        {

            if (signal_name == null)
                return Json(new { result = "ERROR", result_descr = "Не указан сигнал" });


            if (signal_name.CompareTo("btn_click") == 0)
            {

                if (signal_param == null)
                    return Json(new { result = "ERROR", result_descr = "Не указан параметр сигнала" }, JsonRequestBehavior.AllowGet);
                int period = 0;
                int.TryParse(signal_param, out period);

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
                        if (dt.Rows.Count > 0)
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
                else
                    return Json(new { result = "NEXT_TALON_FAIL", result_descr = "нет доступных талонов" });

                return Json(new { result = "NEXT_TALON_OK", number = nextTalon["number"], print_date = DateTime.Now.ToString("YYYY-MM-DD HH:mm:SS") });
            }


            return Json(new { result = "WARN", result_descr = "нереализованный сигнал" });

        }


        public ActionResult InfoPanel()
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("dt1", DateTime.Today));
            parameters.Add(new SqlParameter("dt2", DateTime.Today.AddDays(1)));
            DataTable dt_active_talons = DataBase.mssqlRead("select * from Talon where period between @dt1 and @dt2 and status in (1,2) order by status desc, laborant, period, local_index", parameters.ToArray());

            ViewBag.active_talons = dt_active_talons.Rows;

            return View();
        }
    }
        
    
}