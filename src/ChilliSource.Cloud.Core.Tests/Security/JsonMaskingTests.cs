using ChilliSource.Cloud.Core.Security;
using ChilliSource.Core.Extensions;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ChilliSource.Cloud.Core.Tests
{
    public class JsonMaskingTests
    {
        private const string invalidJson = "{ \"name\": \"jim }";
        private const string json = "{\"name\":\"jim\",\"password\":\"jimrocks\",\"age\":50,\"pins\":[1123,1234],\"secrets\":[{\"secret\":\"topsecret\"}]}";

        [Fact]
        public void JsonMaskingWorks()
        {
            var notMasked = invalidJson.MaskFields(new string[] { "password", "pin" }, "*****");
            Assert.Equal(invalidJson, notMasked);

            var notMasked2 = json.MaskFields(new string[] { }, "*****");
            Assert.Equal(json, notMasked2);

            var notMasked3 = json.MaskFields(new string[] { "jack", "jill" }, "*****");
            Assert.Equal(json, notMasked3);

            var isMasked = json.MaskFields(new string[] { "PASSWORD", "pins", "secret" }, "*****");
            Assert.Equal("{\"name\":\"jim\",\"password\":\"*****\",\"age\":50,\"pins\":\"*****\",\"secrets\":[{\"secret\":\"*****\"}]}", isMasked);
        }

    }

}
