using ChilliSource.Cloud.Core;
using Humanizer;
using System;
using System.Collections.Generic;
using Xunit;

namespace ChilliSource.Cloud.Core.Tests
{
    public class ServiceResultTests
    {
        [Fact]
        public void CopyFrom()
        {
            var r1 = new ServiceResult<int> { Key = "123", Result = 5, StatusCode = System.Net.HttpStatusCode.OK, Success = true };
            var r2 = ServiceResult<string>.CopyFrom(r1);

            Assert.Equal("123", r2.Key);
            Assert.Equal(System.Net.HttpStatusCode.OK, r2.StatusCode);
            Assert.True(r2.Success);

            var r3 = ServiceResult.CopyFrom(r1);
            Assert.Equal("123", r3.Key);
            Assert.Equal(System.Net.HttpStatusCode.OK, r3.StatusCode);
            Assert.True(r3.Success);

        }

    }
}
