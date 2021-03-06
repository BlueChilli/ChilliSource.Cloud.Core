﻿using ChilliSource.Cloud.Core;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    public class GlobalConfiguration
    {
        private static readonly GlobalConfiguration _instance = new GlobalConfiguration();
        public static GlobalConfiguration Instance { get { return _instance; } }

        private GlobalConfiguration() { }

        private ILogger _logger;
        private IHostingEnvironment _hostingEnvironment;
        private IMimeMapping _mimmeMapping;

        public event Action<Exception> LoggingLibraryException = null;

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

        public IMimeMapping GetMimeMapping(bool throwIfNotSet = true)
        {
            if (throwIfNotSet && _mimmeMapping == null)
                throw new ApplicationException("Mime Mapping is not set.");

            return _mimmeMapping;
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

        public GlobalConfiguration SetMimeMapping(IMimeMapping mimeMapping)
        {
            _mimmeMapping = mimeMapping;

            return this;
        }

        /// <summary>
        /// This method SHOULD NOT throw any exceptions
        /// </summary>
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
