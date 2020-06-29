#if !NET_4X
using System;
using NetTopologySuite.Geometries;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// Extensions for GeoCoordinate
    /// </summary>
    public static class GeoCoordinateExtensions
    {
        public static Point ToPoint(this GeoCoordinate source)
        {
            return new Point(source.Longitude, source.Latitude) { SRID = 4326 };
        }

        public static GeoCoordinate ToGeoCoordinate(this Point source)
        {
            return new GeoCoordinate(source.Y, source.X);
        }
    }
}
#endif