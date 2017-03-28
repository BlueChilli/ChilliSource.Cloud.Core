
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ChilliSource.Cloud
{
    public static class ErrorLogHelper
    {
        /// <summary>
        /// Serialize a model to XML and log it for supporting error information.
        /// </summary>
        /// <param name="message">Message describing what is being logged</param>
        /// <param name="model">Model to be logged. Use [XmlIgnore] on sensitive data - eg passwords!</param>
        public static void LogModel(string message, object model, LogEventLevel level = LogEventLevel.Information)
        {
            var details = "Model couldn't be serialized";
            try
            {
                var xmlSerializer = new XmlSerializer(model.GetType());
                using (var memoryStream = new MemoryStream())
                using (var xmlWriter = new XmlTextWriter(memoryStream, Encoding.UTF8))
                {
                    xmlWriter.Formatting = Formatting.Indented;
                    xmlSerializer.Serialize(xmlWriter, model);

                    UTF8Encoding enconding = new UTF8Encoding();
                    details = enconding.GetString(memoryStream.ToArray());
                }
            }
            catch
            {
            }
            
            GlobalConfiguration.Instance.GetLogger().Write(level, message, new { Message = message, Detail = details });
        }

        public static void LogMessage(string message, LogEventLevel level = LogEventLevel.Information)
        {
            GlobalConfiguration.Instance.GetLogger().Write(level, message);
        }
    }
}
