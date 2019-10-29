//using ChilliSource.Core.Extensions;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Net;
//using System.Text;
//using System.Threading;

//namespace ChilliSource.Cloud.Core
//{
//    /// <summary>
//    /// Represents a Google geocode request.
//    /// https://developers.google.com/maps/documentation/geocoding/intro
//    /// </summary>
//    public class GoogleGeocodeHelper
//    {
//        public const string MapUri = "https://maps.googleapis.com/maps/api/geocode/json?{0}&language=en&key={1}";

//        private string _apiKey { get; set; }
//        private int _delay { get; set; }

//        //If looping, set a short delay to avoid being rate limited. Delay is in milliseconds. 50 is fine.
//        public GoogleGeocodeHelper(string apikey, int delay = 0)
//        {
//            _apiKey = apikey;
//            _delay = delay;
//        }

//        /// <summary>
//        /// Geocode address components to lat, lng
//        /// </summary>
//        public ServiceResult<List<GoogleAddress>> Search(string address, string suburb, string postcode, string state, string country)
//        {
//            var sb = new StringBuilder("components=");
//            if (!String.IsNullOrEmpty(address)) sb.AppendFormat("route:{0}|", address);
//            if (!String.IsNullOrEmpty(suburb)) sb.AppendFormat("locality:{0}|", suburb);
//            if (!String.IsNullOrEmpty(postcode)) sb.AppendFormat("postal_code:{0}|", postcode);
//            if (!String.IsNullOrEmpty(state)) sb.AppendFormat("administrative_area:{0}|", state);
//            if (!String.IsNullOrEmpty(country)) sb.AppendFormat("country:{0}|", country);
//            var query = sb.ToString().TrimEnd('|');

//            var uri = String.Format(MapUri, query, _apiKey);
//            return Execute(uri);
//        }

//        /// <summary>
//        /// Geocode an address to lat, lng
//        /// </summary>
//        public ServiceResult<List<GoogleAddress>> Search(string address)
//        {
//            var uri = String.Format(MapUri, $"address={address}", _apiKey);

//            return Execute(uri);
//        }

//        private ServiceResult<List<GoogleAddress>> Execute(string uri)
//        {
//            if (_delay != 0) Thread.Sleep(_delay);

//            var httpRequest = (HttpWebRequest)HttpWebRequest.Create(uri);
//            httpRequest.ContentType = "application/json; charset=utf-8";
//            httpRequest.Method = WebRequestMethods.Http.Get;
//            httpRequest.Accept = "application/json";

//            using (HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse())
//            {
//                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
//                {
//                    var json = sr.ReadToEnd();
//                    var model = json.FromJson<GeocodingResult>();
//                    if (model.Ok()) return ServiceResult<List<GoogleAddress>>.AsSuccess(GoogleAddress.FromResults(json));
//                    return ServiceResult<List<GoogleAddress>>.AsError(model.error_message);
//                }
//            }
//        }

//        private class GeocodingResult : GooglePlaceRequest.GoogleResponseStatus
//        {
//            public string result { get; set; }
//        }

//    }
//}
