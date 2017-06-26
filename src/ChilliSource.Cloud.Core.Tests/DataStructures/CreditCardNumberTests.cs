using ChilliSource.Cloud.Core;
using Humanizer;
using System;
using System.Collections.Generic;
using Xunit;

namespace ChilliSource.Cloud.Core.Tests
{
    public class CreditCardNumberTests
    {
        [Fact]
        public void CreditCardNumber_GivenString_ParsesCreditCardNumber()
        {
            var cc1 = new CreditCardNumber("4242424242424242");
            Assert.Equal(CreditCardType.Visa, cc1.Type);
            Assert.True(cc1.IsValid);

            var cc2 = new CreditCardNumber("4242-4242-4242-4242");
            Assert.Equal(CreditCardType.Visa, cc2.Type);
            Assert.Equal(cc2.ParsedNumber, cc1.Number);
            Assert.True(cc2.IsValid);

            var cc4 = new CreditCardNumber("5555 5555 5555 4444");
            Assert.Equal(CreditCardType.MasterCard, cc4.Type);
            Assert.True(cc4.IsValid);

            var cc5 = new CreditCardNumber("4012 8888 8888 1881");
            Assert.Equal(CreditCardType.Visa, cc2.Type);
            Assert.True(cc5.IsValid);

            var cc6 = new CreditCardNumber("5105105105105100");
            Assert.Equal(CreditCardType.MasterCard, cc6.Type);
            Assert.True(cc6.IsValid);

            var cc7 = new CreditCardNumber("6011111111111117");
            Assert.Equal(CreditCardType.Discover, cc7.Type);
            Assert.True(cc7.IsValid);
            var cc8 = new CreditCardNumber("6011000990139424");
            Assert.Equal(CreditCardType.Discover, cc8.Type);
            Assert.True(cc8.IsValid);

            var cc9 = new CreditCardNumber("30569309025904");
            Assert.Equal(CreditCardType.DinersClub, cc9.Type);
            Assert.True(cc9.IsValid);
            var cc10 = new CreditCardNumber("38520000023237");
            Assert.Equal(CreditCardType.DinersClub, cc10.Type);
            Assert.True(cc10.IsValid);

            var cc11 = new CreditCardNumber("3530111333300000");
            Assert.Equal(CreditCardType.JCB, cc11.Type);
            Assert.True(cc11.IsValid);
            var cc12 = new CreditCardNumber("3566002020360505");
            Assert.Equal(CreditCardType.JCB, cc12.Type);
            Assert.True(cc12.IsValid);

            var cc3 = new CreditCardNumber("4242-4242-4242-4241");
            Assert.Equal(CreditCardType.Visa, cc3.Type);
            Assert.False(cc3.IsValid);

        }

    }
}
