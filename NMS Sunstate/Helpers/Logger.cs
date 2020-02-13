using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS_Sunstate.Helpers
{
    public class Logger
    {
        //        RollingInterval rollingInterval = RollingInterval.Day;
        //ILogger log = new LoggerConfiguration().WriteTo.File("myname").CreateLogger();
        ILogger log = new LoggerConfiguration().WriteTo.Console().CreateLogger();


        //static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public void Log(string message)
        {

            log.Information(message);
        }
    }
}
