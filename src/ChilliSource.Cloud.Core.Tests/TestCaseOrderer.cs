using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace ChilliSource.Cloud.Core.Tests
{
    public class TestCaseOrderer : ITestCaseOrderer
    {
        private readonly IMessageSink diagnosticMessageSink;

        public TestCaseOrderer(IMessageSink diagnosticMessageSink)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
        {
            using (var db = TestDbContext.Create())
            {
                diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Connection String: {db.Database.Connection.ConnectionString}"));
            }

            return testCases;
        }
    }
}
