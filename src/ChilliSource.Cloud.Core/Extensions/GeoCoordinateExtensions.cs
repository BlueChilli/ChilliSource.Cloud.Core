
using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// Extensions for GeoCoordinate
    /// </summary>
    public static class GeoCoordinateExtensions
    {
        public static DbGeography ToDbGeography(this GeoCoordinate coordinate)
        {
            return coordinate == null ? null : CreateDbGeography(coordinate.Latitude, coordinate.Longitude);
        }

        private static DbGeography CreateDbGeography(double? latitude, double? longitude)
        {
            if (latitude == null || longitude == null)
                return null;

            var text = string.Format(CultureInfo.InvariantCulture.NumberFormat, "POINT({0} {1})", longitude, latitude);
            // 4326 is most common coordinate system used by GPS/Maps
            return DbGeography.PointFromText(text, 4326);
        }

        public static GeoCoordinate ToGeoCoordinate(this DbGeography coordinate)
        {
            if (coordinate == null) return null;

            var latitude = coordinate.Latitude ?? 0;
            var longitude = coordinate.Longitude ?? 0;

            return new GeoCoordinate(latitude, longitude);
        }
    }
}
