using ChilliSource.Cloud.Configuration;
using ChilliSource.Cloud.Infrastructure;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Configuration
{
    public class GlobalConfiguration
    {
        private static readonly GlobalConfiguration _instance = new GlobalConfiguration();
        public static GlobalConfiguration Instance { get { return _instance; } }

        private GlobalConfiguration()
        {
            SetLogger(new LoggerConfiguration().CreateLogger()); //Empty logger
        }

        public event Action<Exception> LoggingException = null;

        public ProjectConfigurationSection ProjectConfigurationSection { get; private set; } = new ProjectConfigurationSection();
        public ILogger Logger { get; private set; }
        public IHostingEnvironment HostingEnvironment { get; internal set; }

        public GlobalConfiguration SetProjectConfigurationSection(ProjectConfigurationSection section)
        {
            ProjectConfigurationSection = section;
            return this;
        }

        public GlobalConfiguration SetLogger(ILogger logger)
        {
            Logger = logger;
            return this;
        }

        public GlobalConfiguration SetHostingEnvironment(IHostingEnvironment hostingEnvironment)
        {
            HostingEnvironment = hostingEnvironment;
            return this;
        }

        internal void RaiseLoggingException(Exception ex)
        {
            try
            {
                LoggingException?.Invoke(ex);
            }
            catch { /* noop */ }
        }

        public void Shutdown()
        {
            Log.CloseAndFlush();
        }
    }
}
