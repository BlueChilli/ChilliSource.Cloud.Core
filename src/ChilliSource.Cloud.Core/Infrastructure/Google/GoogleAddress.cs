﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChilliSource.Core.Extensions;
using Humanizer;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// Encapsulates a Google Address for Google Maps.
    /// </summary>
    public class GoogleAddress
    {
        private AddressPart[] addressParts;

        public GoogleAddress()
        {
        }

        /// <summary>
        /// Constructs an instance from a JSON string.
        /// </summary>
        /// <param name="googleAddress">JSON string containing a google address</param>
        public GoogleAddress(string googleAddress)
            : this(String.IsNullOrEmpty(googleAddress) ? null : googleAddress.FromJson<GoogleAddressJson>())
        {
        }

        private GoogleAddress(GoogleAddressJson json)
        {
            StreetParts = new StreetPart();
            Location = new GeoCoordinate();
            if (json == null) return;
            Location = new GeoCoordinate(json.geometry.latitude, json.geometry.longitude);
            Address = json.address;
            addressParts = json.addressParts;
            StreetParts = new StreetPart { UnitNumber = GetPart("subpremise"), Number = GetPart("street_number"), Name = GetPart("street_name"), Type = GetPart("street_type") };
            Suburb = GetPart("locality");
            State = GetPart("administrative_area_level_1");
            PostCode = GetPart("postal_code");
            Country = GetPart("country", returnShort: false);
            Region = GetPart("country");
            Json = json.ToJson();
        }

        /// <summary>
        /// Street details
        /// </summary>
        public StreetPart StreetParts { get; set; }
        public string Suburb { get; set; }
        public string State { get; set; }
        public string PostCode { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }

        public GeoCoordinate Location { get; set; }
        public string Address { get; set; }
        public string Json { get; set; }

        /// <summary>
        /// Line 1 of an address. Unit number, street number, street name and street type
        /// </summary>
        public string Street { get { return this.StreetParts.Street; } }

        /// <summary>
        /// Line 2 of an address. Suburb, State, PostCode
        /// </summary>
        public string SuburbDetails { get { return "{0} {1} {2}".FormatWith(Suburb, State, PostCode); } }

        /// <summary>
        /// Constructs a GoogleAddress collection from a JSON array of addresses
        /// </summary>
        /// <param name="googleAddressResultsJson">JSON arrray of addresses</param>
        /// <returns>GoogleAddress collection</returns>
        public static List<GoogleAddress> FromResults(string googleAddressResultsJson)
        {
            var result = new List<GoogleAddress>();
            if (String.IsNullOrEmpty(googleAddressResultsJson)) return result;
            var resultsJson = googleAddressResultsJson.FromJson<GoogleAddressResultsJson>();
            foreach (var json in resultsJson.results) result.Add(new GoogleAddress(json));
            return result;
        }       

        /// <summary>
        /// Contains street details: Unit, Number, Name, Type
        /// </summary>
        public class StreetPart
        {
            /// <summary>
            /// Retrieves the full street name (Unit number, street number, street name and street type)
            /// </summary>
            public string Street { get { return String.Format("{0}{1} {2} {3}", "{0}/".FormatIfNotNull(UnitNumber), Number, Name, Type); } }
            public string UnitNumber { get; set; }
            public string Number { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
        }

        private class GoogleAddressResultsJson
        {
            public GoogleAddressJson[] results { get; set; }
            public GoogleAddressStatus status { get; set; }
        }

        private class GoogleAddressJson
        {
            private string _address;
            public string address { get { return formatted_address.DefaultTo(_address); } set { _address = value; } }
            public string formatted_address { get; set; }
            public Geometry geometry { get; set; }

            private AddressPart[] _addressParts;
            public AddressPart[] addressParts { get { return address_components == null ? _addressParts : address_components; } set { _addressParts = value; } }
            public AddressPart[] address_components { get; set; }
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

        private class AddressPart
        {
            public string[] types { get; set; }
            public string long_name { get; set; }
            public string short_name { get; set; }

            public bool IsEmpty
            {
                get { return String.IsNullOrEmpty(long_name) && String.IsNullOrEmpty(short_name) && types == null; }
            }

        }

        private class GeometryLocation
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        private string GetPart(string type, bool returnShort = true)
        {
            string path = type;
            if (type == "street_name" || type == "street_type") path = "route";
            var part = addressParts.FirstOrNew(a => !String.IsNullOrEmpty(a.types.FirstOrDefault(t => t.Equals(path))));

            var value = returnShort ? part.short_name ?? "" : part.long_name ?? "";
            if (path == "route" && !String.IsNullOrEmpty(part.short_name))
            {
                var street = value.Split(' ');
                if (street.Count() == 1)
                {
                    if (type == "street_name") return street[0];
                    if (type == "street_type") return "";
                }
                else
                {
                    if (type == "street_name") return String.Join(" ", street, 0, street.Count() - 1);
                    if (type == "street_type") return street.Last();
                }
            }

            return value;
        }

        public enum GoogleAddressStatus
        {
            OK = 1,
            ZERO_RESULTS
            //OVER_QUERY_LIMIT //This will raise exception so api key usage can be corrected
        }

    }

    #region Server side google requests

    /// <summary>
    /// Represents a Google geocode request.
    /// </summary>
    public class GoogleAddressRequest
    {
        public const string MapUri = "{0}://maps.googleapis.com/maps/api/geocode/json?{1}&sensor=false&language=en";

        #region Component filter
        public string Address { get; set; }     //Either Query address or component route
        public string Suburb { get; set; }
        public string Postcode { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        #endregion

        public bool UseHttps { get; set; }

        public string QuotaUser { get; set; }

        /// <summary>
        /// Geocode an address to lat, lng
        /// </summary>
        /// <param name="byComponent">If true filter results so they only match each component filter set</param>
        /// <param name="delay">If looping, set a short delay to avoid being rate limited. Delay is in milliseconds. 50 should work.</param>
        /// <returns></returns>
        public List<GoogleAddress> Search(bool byComponent = false, int delay = 0)
        {
            if (delay != 0) Thread.Sleep(delay);

            var uri = String.Format(MapUri, UseHttps ? "https" : "http", GetComponentFilter(byComponent));
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
                    return GoogleAddress.FromResults(json);
                }
            }
        }

        private string GetComponentFilter(bool byComponent)
        {
            if (byComponent)
            {
                var sb = new StringBuilder("components=");
                if (!String.IsNullOrEmpty(Address)) sb.AppendFormat("route:{0}|", Address);
                if (!String.IsNullOrEmpty(Suburb)) sb.AppendFormat("locality:{0}|", Suburb);
                if (!String.IsNullOrEmpty(Postcode)) sb.AppendFormat("postal_code:{0}|", Postcode);
                if (!String.IsNullOrEmpty(State)) sb.AppendFormat("administrative_area:{0}|", State);
                if (!String.IsNullOrEmpty(Country)) sb.AppendFormat("country:{0}|", Country);
                return sb.ToString().TrimEnd('|');
            }
            else
            {
                return "address={0}".FormatWith(Address);
            }
        }

    }

    internal class GoogleRequestHelper
    {
        internal static string SetQuotaUser(string uri, string quotaUser)
        {
            return String.IsNullOrEmpty(quotaUser) ? uri : uri + $"&quotaUser={quotaUser}";
        }        
    }

    /// <summary>
    /// Finds a Google Address based on GeoLocation (latitude, longitude)
    /// </summary>
    public class GoogleReverseGeocodingRequest
    {
        public const string MapUri = "https://maps.googleapis.com/maps/api/geocode/json?latlng={0},{1}&key={2}&language=en";

        public string ApiKey { get; set; }

        /// <summary>
        /// Finds a Google Address based on GeoLocation (latitude, longitude)
        /// </summary>
        /// <param name="latitude">latitude</param>
        /// <param name="longitude">longitude</param>
        /// <returns>List of matching addresses</returns>
        public List<GoogleAddress> Search(double latitude, double longitude)
        {
            var uri = String.Format(MapUri, latitude, longitude, ApiKey);
            var httpRequest = (HttpWebRequest)HttpWebRequest.Create(uri);
            httpRequest.ContentType = "application/json; charset=utf-8";
            httpRequest.Method = WebRequestMethods.Http.Get;
            httpRequest.Accept = "application/json";

            using (HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse())
            {
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var json = sr.ReadToEnd();
                    return GoogleAddress.FromResults(json);
                }
            }
        }
    }

    #endregion

}
