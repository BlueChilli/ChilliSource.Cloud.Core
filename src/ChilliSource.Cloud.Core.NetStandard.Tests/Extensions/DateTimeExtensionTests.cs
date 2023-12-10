using System;
using Xunit;

namespace ChilliSource.Cloud.Core.Tests
{
    public class DateTimeExtensionTests
    {
        [Fact]
        public void ToCustomDate_ShouldReturnDateCustomFormatted()
        {
            var result = new DateTime(2001, 2, 28, 12, 33, 44).ToCustomDate();
            Assert.Equal("Wed 28th February 2001", result);

            result = new DateTime(2001, 2, 28, 12, 33, 44).ToCustomDate(dayOfWeek: CustomDateDayOfWeek.Long);
            Assert.Equal("Wednesday 28th February 2001", result);

            result = new DateTime(2001, 2, 28, 12, 33, 44).ToCustomDate(dayOfWeek: CustomDateDayOfWeek.Hide);
            Assert.Equal("28th February 2001", result);

            result = new DateTime(2001, 2, 28, 12, 33, 44).ToCustomDate(shortMonth: true);
            Assert.Equal("Wed 28th Feb 2001", result);

            result = new DateTime(2001, 2, 28, 12, 33, 44).ToCustomDate(showYear: false);
            Assert.Equal("Wed 28th February", result);
        }

    }
}
