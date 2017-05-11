﻿
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    public static class SerilogExtensions
    {
        internal static LoggerConfiguration WithApplicationInformation(this LoggerConfiguration config)
        {
            return config.Enrich.With(new ApplicationDetailsLogEnricher());
        }
    }
}