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

        private GlobalConfiguration() { }

        private ProjectConfigurationSection _projectConfigurationSection;
        private ILogger _logger;
        private IHostingEnvironment _hostingEnvironment;

        public event Action<Exception> LoggingLibraryException = null;

        public ProjectConfigurationSection GetProjectConfigurationSection(bool throwIfNotSet = true)
        {
            if (throwIfNotSet && _projectConfigurationSection == null)
                throw new ApplicationException("Project Configuration Section is not set.");

            return _projectConfigurationSection;
        }

        public ILogger GetLogger(bool throwIfNotSet = true)
        {
            if (throwIfNotSet && _logger == null)
                throw new ApplicationException("Logger is not set.");

            return _logger;
        }

        public IHostingEnvironment GetHostingEnvironment(bool throwIfNotSet = true)
        {
            if (throwIfNotSet && _hostingEnvironment == null)
                throw new ApplicationException("Hosting environment is not set.");

            return _hostingEnvironment;
        }

        public GlobalConfiguration SetProjectConfigurationSection(ProjectConfigurationSection section)
        {
            _projectConfigurationSection = section;
            return this;
        }

        public GlobalConfiguration SetLogger(ILogger logger)
        {
            _logger = logger;
            return this;
        }

        public GlobalConfiguration SetHostingEnvironment(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            return this;
        }

        internal void RaiseLoggingLibraryException(Exception ex)
        {
            try
            {
                LoggingLibraryException?.Invoke(ex);
            }
            catch { /* noop */ }
        }

        public void Shutdown()
        {
            Log.CloseAndFlush();
        }
    }
}
