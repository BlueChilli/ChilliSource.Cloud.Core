using ChilliSource.Core.Extensions;
using System;
using System.IO;
using Xunit;

namespace ChilliSource.Cloud.Core.Tests
{
    public class TypeMergerTests
    {

        [Fact]
        public void MergeObject_MergesTwoObjects_IntoANewObject()
        {
            var object1 = new { Id = 1, Name = "Object1", DateCreated = new DateTime(2001, 1, 1) };
            var object2 = new { Id = 3, Description = "Object2 is richer", Money = 12345.0M };

            var mergedObject = TypeMerger.MergeTypes(object1, object2);

            var dictionary = mergedObject.ToDictionary();

            Assert.Equal("Object1", dictionary["Name"]);
            Assert.Equal("Object2 is richer", dictionary["Description"]);
            Assert.Equal(12345.0M, dictionary["Money"]);
            Assert.Equal(new DateTime(2001, 1, 1), dictionary["DateCreated"]);
            Assert.Equal(1, dictionary["Id"]);
        }
    }
}
