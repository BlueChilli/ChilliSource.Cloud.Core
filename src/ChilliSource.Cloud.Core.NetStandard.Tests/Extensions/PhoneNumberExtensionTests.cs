using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChilliSource.Cloud.Core.Phone;
using Newtonsoft.Json;
using PhoneNumbers;
using Xunit;
using Xunit.Abstractions;

namespace ChilliSource.Cloud.Core.Tests
{

    public class PhoneNumberExtensionTests: IDisposable
    {
        private readonly StringBuilder Console = new StringBuilder();
        private readonly ITestOutputHelper _output;

        public PhoneNumberExtensionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        public void Dispose()
        {
            var outputStr = Console.ToString();
            if (outputStr.Length > 0)
            {
                _output.WriteLine(outputStr);
            }
        }

        [Theory]
        [InlineData("041155555", false)]
        [InlineData("098849586", false)]
        [InlineData("9661234", false)]
        [InlineData("0411555555", true)]
        [InlineData("+61411555555", true)]
        [InlineData("+6198583745", false)]
        [InlineData("+61299941524", true)]
        [InlineData("98", false)]
        [InlineData("111111111111111111", false)]
        [InlineData("+ade34234234", false)]

        public void IsValidPhoneNumber_ShouldBeValidatedCorrectly_WhenNumberIsGiven(string number, bool shouldBeCorrect)
        {
            if (shouldBeCorrect)
            {
                Assert.True(number.IsValidPhoneNumber("AU", PhoneNumberType.FIXED_LINE_OR_MOBILE, PhoneNumberType.MOBILE, PhoneNumberType.FIXED_LINE));
            }
            else
            {
                Assert.False(number.IsValidPhoneNumber("AU", PhoneNumberType.FIXED_LINE_OR_MOBILE,PhoneNumberType.MOBILE, PhoneNumberType.FIXED_LINE));
            }
        }

        [Theory]
        [InlineData("0411555555", "61411555555", PhoneNumberFormat.E164)]
        [InlineData("+61411555555", "61411555555", PhoneNumberFormat.E164)]
        [InlineData("61296661234", "61296661234", PhoneNumberFormat.E164)]
        public void FormatPhoneNumber_ShouldFormatToCorrectGivenFormat(string number, string expected, PhoneNumberFormat format)
        {
            Assert.Equal(expected, number.FormatNumber("AU", format, PhoneNumberType.FIXED_LINE_OR_MOBILE, PhoneNumberType.MOBILE, PhoneNumberType.FIXED_LINE));
        }

        [Fact]
        public void PrintRegions()
        {
            var regions = PhoneNumberUtil.GetInstance().GetSupportedRegions();

            var str = JsonConvert.SerializeObject(regions);

            Console.AppendLine(str);
        }
    }
}
