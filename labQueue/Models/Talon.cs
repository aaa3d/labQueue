using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace labQueue.Models
{
    public class Talon
    {
        public enum talon_statuses  {свободен,недоступен,вызов,завершен};

        public int talonNumber = 1;
        public DateTime talonDateTimePeriod { get; set; }
        public talon_statuses talon_status { get; set; }

    }
}