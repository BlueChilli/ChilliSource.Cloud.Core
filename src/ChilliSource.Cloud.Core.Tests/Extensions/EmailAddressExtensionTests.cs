using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChilliSource.Cloud.Core.Email;
using Newtonsoft.Json;
using PhoneNumbers;
using Xunit;
using Xunit.Abstractions;

namespace ChilliSource.Cloud.Core.Tests
{

    public class EmailExtensionTests
    {

        [Theory]
        [InlineData("joe@example.com", true)]
        [InlineData("jane+12345@example.com.au", true)]
        [InlineData("x@x.xx", true)]
        [InlineData("joe..13@example.com", false)]
        [InlineData("example.com", false)]
        [InlineData("@example.com", false)]
        [InlineData("joe@example.com.", false)]
        [InlineData("joe@example..com.au", false)]
        [InlineData("111111111111111111", false)]
        [InlineData("jane±12345@example.com", false)]
        public void IsValidEmailAddress_ShouldBeValidatedCorrectly_WhenEmailAddressIsGiven(string emailaddress, bool shouldBeCorrect)
        {
            if (shouldBeCorrect)
            {
                Assert.True(emailaddress.IsValidEmailAddress());
            }
            else
            {
                Assert.False(emailaddress.IsValidEmailAddress());
            }
        }

        [Theory]
        [InlineData("joe@example.com", "example.com")]
        [InlineData("jane+12345@example.com.au", "example.com.au")]
        [InlineData("@example.com", "example.com")]
        [InlineData("example.com", null)]
        public void GetEmailAddressDomain_ShouldReturnDomainOfAnEmailAddress(string emailaddress, string expectedDomain)
        {
            Assert.Equal(expectedDomain, emailaddress.GetEmailAddressDomain());
        }

        [Theory]
        [InlineData("joe@example.com", "joe")]
        [InlineData("jane+12345@example.com.au", "jane+12345")]
        [InlineData("@example.com", "")]
        [InlineData("example.com", null)]
        public void GetEmailAddressLocalPart_ShouldReturnLocalPartOfAnEmailAddress(string emailaddress, string expectedLocalPart)
        {
            Assert.Equal(expectedLocalPart, emailaddress.GetEmailAddressLocalPart());
        }

    }
}
