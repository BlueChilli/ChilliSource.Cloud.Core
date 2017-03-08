﻿using ChilliSource.Cloud.Configuration;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Logging.Extensions
{
    public static class LoggingExtensions
    {         
        /// <summary>
        /// Exception extention to log exception to Raygun.io
        /// </summary>
        public static void LogException(this Exception ex, LogEventLevel level = LogEventLevel.Error)
        {            
            try
            {
                GlobalConfiguration.Instance.Logger.Write(level, ex, ex.Message);
            }
            catch (Exception logException)
            {
                GlobalConfiguration.Instance.RaiseLoggingException(logException);                
            }
        }
    }
}
