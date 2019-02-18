
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Extension to log the exception. This method SHOULD NOT throw any exceptions
        /// </summary>
        public static void LogException(this Exception ex, LogEventLevel level = LogEventLevel.Error)
        {            
            try
            {
                var logger = GlobalConfiguration.Instance.GetLogger();
                logger.Write(level, ex, ex.Message);
            }
            catch (Exception logException)
            {
                GlobalConfiguration.Instance.RaiseLoggingLibraryException(logException);
            }
        }
    }
}
