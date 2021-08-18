using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using Xunit;

namespace ChilliSource.Cloud.Core.Tests
{
    public class CsvHelperExtensionTests
    {
        [Fact]
        public void ToCsv_FromListInt_ShouldReturnCorrectCsv()
        {
            var result = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 }.ToCsvFile(new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { ShouldQuote = arg => false });

            Assert.Equal("1\r\n2\r\n3\r\n4\r\n5\r\n6\r\n7\r\n8\r\n9\r\n", result);
        }

    }
}
