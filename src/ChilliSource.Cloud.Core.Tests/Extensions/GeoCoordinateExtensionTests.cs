using ChilliSource.Cloud.Core;
using Humanizer;
using System;
using System.Collections.Generic;
using Xunit;

namespace ChilliSource.Cloud.Core.Tests
{
    public class GeoCoordinateExtensionTests
    {
        [Fact]
        public void ToDbGeography_AndBack_ShouldReturnSameValue()
        {
            var source = new GeoCoordinate(37.3861, 122.0839);

            var result = source.ToDbGeography().ToGeoCoordinate();

            Assert.Equal(source.Latitude, result.Latitude);
            Assert.Equal(source.Longitude, result.Longitude);
        }

    }
}
