using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using labQueue.Models;
using System.Collections;

namespace labQueue.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            //динамически создавать массив талонов на текущий день
            DateTime dt = DateTime.Today.AddHours(8);
            int curTalonNumber = 1;

            ArrayList talons = new ArrayList();
            for(int i=0; i<16; i++)
            {
                for(int j=0; j < 20; j++)
                {
                    Talon talon = new Talon();
                    talon.talonNumber = curTalonNumber;
                    talon.talonDateTimePeriod = dt;
                    talon.talon_status = Talon.talon_statuses.свободен;
                    talons.Add(talon);
                    curTalonNumber++;
                }
                dt = dt.AddMinutes(15);
            }
            
            ViewBag.Message = "Your application description page.";
            ViewBag.Message17 = "Your application description page 17.";
            ViewBag.talons = talons;
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