using ChilliSource.Core.Extensions;
using ChilliSource.Cloud.Core;
using Humanizer;
using System;
using System.Collections.Generic;
using Xunit;

namespace ChilliSource.Cloud.Core.Tests
{
    public class CharacterSetTests
    {
        [Fact]
        public void CharacterSet_Works()
        {
            var cs1 = CharacterSet.Numbers;

            Assert.Equal(cs1.Count, 10);
            Assert.True(cs1.Validate("0123456789"));

            var cs2 = CharacterSet.LowerCaseVowels.ToUpper();
            Assert.True(cs2.Validate("AEIOU"));

            var cs3 = CharacterSet.FromChars('$', '*', ')');
            Assert.True(cs3.Validate("$*)"));

            var cs4 = CharacterSet.FromRange('z', 'p');
            Assert.True(cs4.Validate("zyxwvutsrqp"));

            var cs5 = CharacterSet.CombineSets(cs1, cs2, cs3, cs4, cs2);
            Assert.True(cs5.Validate("0123456789zyxwvutsrqp$*)AEIOU"));
            Assert.Equal("0123456789zyxwvutsrqp$*)AEIOU".Length, cs5.Count);
        }

    }
}
