using ChilliSource.Cloud.Core;
using Humanizer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

namespace ChilliSource.Cloud.Core.Tests
{
    public class MemberInfoExtensionTests
    {
        public class TestClass
        {
            [Index("SomethingElse")]
            public string Something { get; set; }
        }

        [Fact]
        public void GetAttribute_ReturnsAttribute_OnClassProperty()
        {
            var test = new TestClass();
            var attribute = test.GetType().GetMember("Something")[0].GetAttribute<IndexAttribute>(false);
            var attribute2 = test.GetType().GetMember("Something")[0].GetAttribute<CollectionAttribute>(false);

            Assert.Equal(attribute.Name, "SomethingElse");
            Assert.Null(attribute2);
        }

    }
}
