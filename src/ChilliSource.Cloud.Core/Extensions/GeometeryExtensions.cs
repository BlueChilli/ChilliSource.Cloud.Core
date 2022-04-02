#if !NET_4X
using System;
using NetTopologySuite.Geometries;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// Extensions for Geometery related objects
    /// </summary>
    public static class GeometeryExtensions
    {
        public static Point ToPoint(this GeoCoordinate source)
        {
            return new Point(source.Longitude, source.Latitude) { SRID = 4326 };
        }

        public static GeoCoordinate ToGeoCoordinate(this Point source)
        {
            return new GeoCoordinate(source.Y, source.X);
        }

        public static double Latitude(this Point p)
        {
            return p.Y;
        }

        public static double Longitude(this Point p)
        {
            return p.X;
        }
    }
}
#endif