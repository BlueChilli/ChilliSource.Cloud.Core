using ChilliSource.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// Represents a request to query a term on Google places.
    /// </summary>
    public class GooglePlaceRequest
    {
        public const string MapUri = "https://maps.googleapis.com/maps/api/place/findplacefromtext/json?input={0}&key={1}&locationbias=circle:{2}@{3}&inputtype=textquery&language=en&fields=place_id,formatted_address,geometry,name";

        public string ApiKey { get; set; }

        public string QuotaUser { get; set; }

        /// <summary>
        /// Calls Google places API
        /// </summary>
        /// <param name="query">Search term to query</param>
        /// <returns>List of Google Places</returns>
        public List<GooglePlace> Search(string query, out GoogleResponseStatus status)
        {
            return this.Search(query, null, null, out status);
        }

        /// <summary>
        /// Calls Google places API
        /// </summary>
        /// <param name="query">Search term to query</param>
        /// <param name="coordinate">Coordinate to bias the result</param>
        /// <param name="radius">Radius to bias the result</param>
        /// <param name="status">Response status</param>
        /// <returns>List of Google Places</returns>
        ///         
        public List<GooglePlace> Search(string query, GeoCoordinate coordinate, long? radius, out GoogleResponseStatus status)
        {
            string location = (coordinate != null) ? String.Format("{0},{1}", coordinate.Latitude, coordinate.Longitude) : null;
            var uri = String.Format(MapUri, query, ApiKey, Convert.ToString(radius), location);
            uri = GoogleRequestHelper.SetQuotaUser(uri, this.QuotaUser);
            var httpRequest = (HttpWebRequest)HttpWebRequest.Create(uri);
            httpRequest.ContentType = "application/json; charset=utf-8";
            httpRequest.Method = WebRequestMethods.Http.Get;
            httpRequest.Accept = "application/json";

            using (HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse())
            {
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var json = sr.ReadToEnd();
                    return GooglePlaceResult.ProcessResult(json, out status);
                }
            }
        }

        /// <summary>
        ///  Google response status
        /// </summary>
        public abstract class GoogleResponseStatus
        {
            /// <summary>
            /// Raw status
            /// </summary>
            public string status { get; set; }
            /// <summary>
            /// Raw mesage
            /// </summary>
            public string error_message { get; set; }

            /// <summary>
            /// Checks if the response is ok
            /// </summary>
            /// <returns>True if the response has no errors.</returns>
            public bool Ok()
            {
                return status == "OK";
            }
        }

        private class GooglePlaceResult : GoogleResponseStatus
        {
            public GooglePlaceJson[] candidates { get; set; }

            //public enum GooglePlaceStatus
            //{
            //    OK = 1,
            //    REQUEST_DENIED
            //}

            public static List<GooglePlace> ProcessResult(string googlePlaceResultJson, out GoogleResponseStatus status)
            {
                var googlePlaceResult = googlePlaceResultJson.FromJson<GooglePlaceResult>();
                var result = new List<GooglePlace>();
                foreach (var place in googlePlaceResult.candidates)
                {
                    result.Add(new GooglePlace
                    {
                        Name = place.name,
                        Address = place.formatted_address,
                        Location = new GeoCoordinate(place.geometry.latitude, place.geometry.longitude),
                        PlaceId = place.place_id
                    });
                }

                status = googlePlaceResult;
                return result;
            }
        }

        private class GooglePlaceJson
        {
            public string place_id { get; set; }
            public string name { get; set; }
            public string formatted_address { get; set; }
            public Geometry geometry { get; set; }
        }

        private class Geometry
        {
            public Geometry()
            {
                location = new GeometryLocation();
            }

            public GeometryLocation location { get; set; }
            public double latitude { get { return location.lat; } set { location.lat = value; } }
            public double longitude { get { return location.lng; } set { location.lng = value; } }
        }

        private class GeometryLocation
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }
    }

    /// <summary>
    /// Represents a Google place response
    /// </summary>
    public class GooglePlace
    {
        /// <summary>
        /// PlaceId for place details lookup => https://developers.google.com/places/web-service/details
        /// </summary>
        public string PlaceId { get; set; }
        /// <summary>
        /// Place name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Google places formatted address
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// Place location (lat, lng)
        /// </summary>
        public GeoCoordinate Location { get; set; }
    }
}
