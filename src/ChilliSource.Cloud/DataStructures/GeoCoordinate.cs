using System;
using System.Globalization;
using System.Data.Entity.Spatial;
using ChilliSource.Cloud.Extensions;

namespace ChilliSource.Cloud.DataStructures
{
    /// <summary>
    /// Represents a GeoLocation.
    /// </summary>
    [Serializable]
    public class GeoCoordinate
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public GeoCoordinate()
        {

        }

        public GeoCoordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        /// <summary>
        /// Calcuates distance betwen two points using haversine formula
        /// returns in Kilometres
        /// </summary>
        /// <param name="other"></param>
        /// <returns>distance in KM</returns>
        public double DistanceInFlatPlane(GeoCoordinate other)
        {
            var latitude = (other.Latitude - Latitude).ToRadian();
            var longtude = (other.Longitude - Longitude).ToRadian();

            var h1 = Math.Sin(latitude / 2) * Math.Sin(latitude / 2) +
                  Math.Cos(Latitude.ToRadian()) * Math.Cos(other.Latitude.ToRadian()) * Math.Sin(longtude / 2) * Math.Sin(longtude / 2);

            var h2 = 2 * Math.Asin(Math.Min(1, Math.Sqrt(h1)));

            return R * h2;
        }

        /// <summary>
        /// Checks wether distance is within give acceptable distance
        /// </summary>
        /// <param name="other">other geo lcoation</param>
        /// <param name="acceptableDistance">this must be in metres</param>
        /// <returns></returns>
        public bool WithinDistanceOf(GeoCoordinate other, double acceptableDistance)
        {
            var distance = DistanceInFlatPlane(other);

            return distance <= (acceptableDistance / 1000);
        }

        private const double R = 6371;


    }

    /// <summary>
    /// Extensions for GeoCoordinate
    /// </summary>
    public static class GeoCoordinateExtensions
    {
        public static DbGeography CreatePoint(this GeoCoordinate coordinate)
        {
            return coordinate == null ? null : CreatePoint(coordinate.Latitude, coordinate.Longitude);
        }

        public static DbGeography CreatePoint(double? latitude, double? longitude)
        {
            if (latitude == null || longitude == null)
                return null;

            var text = string.Format(CultureInfo.InvariantCulture.NumberFormat, "POINT({0} {1})", longitude, latitude);
            // 4326 is most common coordinate system used by GPS/Maps
            return DbGeography.PointFromText(text, 4326);
        }

        public static GeoCoordinate CreatePoint(this DbGeography coordinate)
        {
            if (coordinate == null) return null;

            var latitude = coordinate.Latitude ?? 0;
            var longitude = coordinate.Longitude ?? 0;

            return new GeoCoordinate(latitude, longitude);
        }
    }

}