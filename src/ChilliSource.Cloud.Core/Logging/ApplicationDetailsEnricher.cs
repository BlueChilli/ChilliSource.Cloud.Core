
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    internal class ApplicationDetailsLogEnricher : ILogEventEnricher
    {

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var config = ProjectConfigurationSection.GetConfig();

            //Add properties and values to all log messages uses this logger
            //Add other properties here if needed
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ApplicationName", config.ProjectName));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ProjectEnvironment", config.ProjectEnvironment));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("BaseUrl", config.BaseUrl));
        }
    }
}
