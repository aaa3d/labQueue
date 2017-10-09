using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace labQueue
{
    public class LaboratoryHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }

        public void Send(dynamic message)
        {
            Clients.All.RefreshPage();
        }
    }
}