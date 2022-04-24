using ChilliSource.Core.Extensions;
using RestSharp;
using System;
using System.Threading;

namespace ChilliSource.Cloud.Core
{

    //https://developers.google.com/maps/documentation/timezone/overview
    public class GoogleTimezoneHelper
    {
        public const string MapUri = "https://maps.googleapis.com/maps/api/timezone/json";

        private string _apiKey { get; set; }
        private int _delay { get; set; }

        //If looping, set a short delay to avoid being rate limited. Delay is in milliseconds. 50 is fine.
        public GoogleTimezoneHelper(string apikey, int delay = 0) 
        {
            _apiKey = apikey;
            _delay = delay;
        }

        /// <summary>
        /// Return timezone for a lat, lng position
        /// </summary>
        public ServiceResult<GoogleTimezone> Search(double latitude, double longitude)
        {
            if (_delay != 0) Thread.Sleep(_delay);

            var client = new RestClient("https://maps.googleapis.com");
            var request = new RestRequest("maps/api/timezone/json", Method.GET);
            request.AddParameter("key", _apiKey);
            request.AddParameter("location", $"{latitude},{longitude}");
            request.AddParameter("timestamp", (int)DateTime.UtcNow.ToUnixTime().TotalSeconds);
            var response = client.Execute<GoogleTimezone>(request);

            if (response.Data.Status != "OK")
            {
                var error = $"Timezone error for {latitude},{longitude} - {response.Data.ErrorMessage ?? response.Data.Status}";
                return ServiceResult<GoogleTimezone>.AsError(response.Data, error);
            }

            return ServiceResult<GoogleTimezone>.AsSuccess(response.Data);
        }

    }

    public class GoogleTimezone
    {
        public double DstOffset { get; set; }
        public double RawOffset { get; set; }
        public string Status { get; set; }
        public string TimeZoneId { get; set; }
        public string TimeZoneName { get; set; }
        public string ErrorMessage { get; set; }
    }

}