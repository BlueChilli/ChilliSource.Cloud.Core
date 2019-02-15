using System;
using ChilliSource.Core.Extensions;

namespace ChilliSource.Cloud.Core
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
        /// <param name="other">the other geoordinate point</param>
        /// <returns>distance in KM</returns>
        public double DistanceInFlatPlane(GeoCoordinate other)
        {
            var latitude = (other.Latitude - Latitude).ToRadians();
            var longtude = (other.Longitude - Longitude).ToRadians();

            var h1 = Math.Sin(latitude / 2) * Math.Sin(latitude / 2) +
                  Math.Cos(Latitude.ToRadians()) * Math.Cos(other.Latitude.ToRadians()) * Math.Sin(longtude / 2) * Math.Sin(longtude / 2);

            var h2 = 2 * Math.Asin(Math.Min(1, Math.Sqrt(h1)));

            return R * h2;
        }

        /// <summary>
        /// Checks wether the distance of two points is within a given acceptable distance
        /// </summary>
        /// <param name="other">other geo lcoation</param>
        /// <param name="acceptableDistanceMetres">distance in metres to check against</param>
        /// <returns></returns>
        public bool WithinDistanceOf(GeoCoordinate other, double acceptableDistanceInMetres)
        {
            var distance = DistanceInFlatPlane(other);

            return distance <= (acceptableDistanceInMetres / 1000);
        }

        private const double R = 6371;
    }
}