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


        


        public ActionResult About(String subaction, String subaction_param)
        {


            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("dt1", DateTime.Today));
            parameters.Add(new SqlParameter("dt2", DateTime.Today.AddDays(1)));
            DataTable dt = DataBase.mssqlRead("select * from Talon where period between @dt1 and @dt2 order by period, local_index", parameters.ToArray());
            if (dt.Rows.Count < 1)
            {
                generateTalons(DateTime.Today.AddHours(8), 16, 20);
                dt = DataBase.mssqlRead("select * from Talon where period between @dt1 and @dt2 order by period, local_index", parameters.ToArray());
            }
            //динамически создавать массив талонов на текущий день

            if (dt.Rows.Count < 1)
            {
                ViewBag.Message = "Ошибка формирования талонов на день";
            }
            else
                ViewBag.Message = "Талонов на день: "+ dt.Rows.Count;

            
            ViewBag.Message17 = "Your application description page 17.";
            ViewBag.talons = dt.Rows;
            ViewBag.timePeriods = 16;
            ViewBag.talonsInPeriod = 20;


            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}