using ChilliSource.Cloud.Core;
using Humanizer;
using System;
using System.Collections.Generic;
using Xunit;

namespace ChilliSource.Cloud.Core.Tests
{
    public class ShortGuidTests
    {
        [Fact]
        public void ToShortGuid_GivenGuid_RetunsShortGuidRepresentationOfGuid()
        {
            var guid = Guid.Parse("7B9DA377-CD4B-431E-BE8E-16693D1B613C");

            var sg = guid.ToShortGuid();
            Assert.Equal("d6Ode0vNHkO-jhZpPRthPA", sg.Value);
            Assert.Equal(sg.Guid, guid);

            Assert.Equal(ShortGuid.Empty.Guid, Guid.Empty);

            var s = ShortGuid.Encode(guid.ToString());
            var g = ShortGuid.Decode(s);
            Assert.Equal(g, guid);

            Assert.True(sg == new ShortGuid(s));
        }

    }
}
