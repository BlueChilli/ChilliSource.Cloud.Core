using ChilliSource.Cloud.Core;
using Humanizer;
using System;
using System.Collections.Generic;
using Xunit;

namespace ChilliSource.Cloud.Core.Tests
{
    public class GeoCoordinateTests
    {
        [Fact]
        public void DistanceInFlatPlane_ReturnsDistance_BetweenTwoPoints()
        {
            var brisbane = new GeoCoordinate(27.4698, 153.0251);
            var sydney = new GeoCoordinate(33.8688, 151.2093);
            var melbourne = new GeoCoordinate(37.8136, 144.9631);
            var perth = new GeoCoordinate(31.9505, 115.8605);

            Assert.Equal(732, (int)brisbane.DistanceInFlatPlane(sydney));
            Assert.Equal(713, (int)melbourne.DistanceInFlatPlane(sydney));
            Assert.Equal(2721, (int)perth.DistanceInFlatPlane(melbourne));

            Assert.True(sydney.WithinDistanceOf(melbourne, 1000000));
        }

    }
}
