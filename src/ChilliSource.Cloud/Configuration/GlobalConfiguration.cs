using ChilliSource.Cloud.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Configuration
{
    public static class GlobalConfiguration
    {
        public static event Action<Exception> LoggingException = null;

        public static ProjectConfigurationSection ProjectConfigurationSection { get; private set; } = new ProjectConfigurationSection();
        public static ILogger Logger { get { return Log.Logger; } }

        static GlobalConfiguration()
        {
            SetLogger(new LoggerConfiguration().CreateLogger()); //Empty logger
        }

        public static void SetProjectConfigurationSection(ProjectConfigurationSection section)
        {
            ProjectConfigurationSection = section;
        }

        public static void SetLogger(ILogger logger)
        {
            Log.Logger = logger;
        }

        internal static void RaiseLoggingException(Exception ex)
        {
            try
            {
                LoggingException?.Invoke(ex);
            }
            catch { /* noop */ }
        }

        public static void Shutdown()
        {
            Log.CloseAndFlush();
        }
    }
}
