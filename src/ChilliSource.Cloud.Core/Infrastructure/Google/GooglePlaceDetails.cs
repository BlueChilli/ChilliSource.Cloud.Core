using ChilliSource.Core.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{

    public class GooglePlaceDetailsRequest
    {
        public const string MapUri = "https://maps.googleapis.com/maps/api/place/details/json?placeid={0}&key={1}&fields=address_component,adr_address,formatted_address,geometry,icon,id,name,photo,place_id,scope,type,url,utc_offset,vicinity,formatted_phone_number,international_phone_number,opening_hours,website{2}";

        public string ApiKey { get; set; }

        /// <summary>
        /// The Atmosphere category includes the following fields: price_level, rating, review, user_ratings_total. This has a higher cost per api call
        /// </summary>
        public bool IncludeAtmosphereFields { get; set; }

        public GooglePlaceDetails Search(string placeId, out GooglePlaceRequest.GoogleResponseStatus status)
        {
            var uri = String.Format(MapUri, placeId, ApiKey, IncludeAtmosphereFields ? ",price_level,rating,review,user_ratings_total" : "");
            var httpRequest = (HttpWebRequest)HttpWebRequest.Create(uri);
            httpRequest.ContentType = "application/json; charset=utf-8";
            httpRequest.Method = WebRequestMethods.Http.Get;
            httpRequest.Accept = "application/json";

            using (HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse())
            {
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var json = sr.ReadToEnd();
                    return ProcessResult(json, out status);
                }
            }
        }

        public static GooglePlaceDetails ProcessResult(string googlePlaceResultJson, out GooglePlaceRequest.GoogleResponseStatus status)
        {
            var model = googlePlaceResultJson.FromJson<GooglePlaceDetailsInternal.GooglePlaceDetailsResult>();
            var details = model.result;
            status = model;
            if (details != null)
            {
                var result = new GooglePlaceDetails
                {
                    Name = details.Name,
                    Location = new GeoCoordinate(details.Geometry.Location.Lat, details.Geometry.Location.Lng),
                    Website = details.Website,
                    Phone = details.Formatted_Phone_Number,
                    Url = details.Url,
                    Photos = details.Photos == null ? new List<string>() : details.Photos.Where(p => p.Width > 1000).Select(p => p.Photo_Reference).Take(5).ToList(),
                    OpeningHours = details.Opening_Hours == null ? new List<GooglePlaceDetails.OpenHours>() : details.Opening_Hours.Periods.Select(x => x.GetOpeningHours()).Union(GooglePlaceDetails.OpenHours.DefaultOpenHours(false), new GooglePlaceDetails.OpenHoursCompare()).ToList(),
                    Reviews = details.Reviews == null ? new List<GooglePlaceDetails.Review>() : details.Reviews.Select(x => new GooglePlaceDetails.Review { Author = x.Author_Name, AuthorUrl = x.Author_Url, AuthorImageUrl = x.Profile_Photo_Url, Rating = x.Rating, Text = x.Text, ReviewedOn = x.Time }).ToList(),
                    Json = googlePlaceResultJson
                };
                var address = new GoogleAddress(details.ToJson());
                result.Address = address;
                return result;
            }

            return null;
        }
    }

    public class GooglePlaceDetails
    {
        public GoogleAddress Address { get; set; }

        public GeoCoordinate Location { get; internal set; }
        public string Name { get; internal set; }
        public List<OpenHours> OpeningHours { get; internal set; }
        public List<Review> Reviews { get; internal set; }
        public List<string> Photos { get; internal set; }
        public double Rating { get; internal set; }
        public string Url { get; internal set; }
        public string Website { get; internal set; }
        public string Phone { get; internal set; }
        public string Json { get; internal set; }

        public class OpenHours
        {
            public DayOfWeek Day { get; set; }

            public DateTime OpenStartTime { get; set; }

            public DateTime OpenEndTime { get; set; }

            public static List<OpenHours> DefaultOpenHours(bool isOpen)
            {
                var openingHours = EnumHelper.GetValues<DayOfWeek>();
                var result = openingHours.Select(x => new OpenHours { Day = x, OpenStartTime = DateTime.Today, OpenEndTime = isOpen ? DateTime.Today.AddHours(23).AddMinutes(59) : DateTime.Today }).ToList();
                return result;
            }
        }

        public class OpenHoursCompare : IEqualityComparer<OpenHours>
        {
            public bool Equals(OpenHours x, OpenHours y)
            {
                return x.Day == y.Day;
            }

            public int GetHashCode(OpenHours obj)
            {
                return (int)obj.Day;
            }
        }

        public class Review
        {
            public string Author { get; set; }
            public string AuthorUrl { get; internal set; }
            public string AuthorImageUrl { get; internal set; }
            public int Rating { get; set; }
            public string Text { get; set; }
            public DateTime ReviewedOn { get; set; }
        }
    }

}

namespace ChilliSource.Cloud.Core.GooglePlaceDetailsInternal
{
    public class GooglePlaceDetailsResult : GooglePlaceRequest.GoogleResponseStatus
    {
        public GooglePlaceDetailsJson result { get; set; }

        //public enum GooglePlaceStatus
        //{
        //    OK = 1,
        //    REQUEST_DENIED
        //}
    }

    public class GooglePlaceDetailsJson
    {
        public IList<AddressComponent> Address_Components { get; set; }
        public string Adr_Address { get; set; }
        public string Formatted_Address { get; set; }
        public string Formatted_Phone_Number { get; set; }
        public Geometry Geometry { get; set; }
        public string Icon { get; set; }
        public string Id { get; set; }
        public string International_Phone_Number { get; set; }
        public string Name { get; set; }
        public OpeningHours Opening_Hours { get; set; }
        public List<Review> Reviews { get; set; }
        public IList<Photo> Photos { get; set; }
        public string PlaceId { get; set; }
        public string Reference { get; set; }
        public string Scope { get; set; }
        public IList<string> Types { get; set; }
        public string Url { get; set; }
        public int Utc_Offset { get; set; }
        public string Vicinity { get; set; }
        public string Website { get; set; }
    }

    public class AddressComponent
    {
        public string Long_Name { get; set; }
        public string Short_Name { get; set; }
        public IList<string> Types { get; set; }
    }

    public class Location
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class Northeast
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class Southwest
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class Viewport
    {
        public Northeast Northeast { get; set; }
        public Southwest Southwest { get; set; }
    }

    public class Geometry
    {
        public Location Location { get; set; }
        public Viewport Viewport { get; set; }
    }

    public class Close
    {
        public int Day { get; set; }
        public string Time { get; set; }
    }

    public class Open
    {
        public int Day { get; set; }
        public string Time { get; set; }
    }

    public class Period
    {
        public Close Close { get; set; }
        public Open Open { get; set; }

        public GooglePlaceDetails.OpenHours GetOpeningHours()
        {
            var result = new GooglePlaceDetails.OpenHours
            {
                Day = (DayOfWeek)(Open.Day == 7 ? 0 : Open.Day),
                OpenStartTime = DateTime.ParseExact(Open?.Time ?? "0000", "HHmm", CultureInfo.InvariantCulture),
                OpenEndTime = DateTime.ParseExact(Close?.Time ?? "0000", "HHmm", CultureInfo.InvariantCulture)
            };
            if (Open?.Time == "0000" && Close == null) result.OpenEndTime = DateTime.Today.AddHours(23).AddMinutes(59);
            return result;
        }
    }

    public class OpeningHours
    {
        public bool OpenNow { get; set; }
        public IList<Period> Periods { get; set; }
        public IList<string> WeekdayText { get; set; }
    }

    public class Photo
    {
        public int Height { get; set; }
        public IList<string> Html_Attributions { get; set; }
        public string Photo_Reference { get; set; }
        public int Width { get; set; }
    }

    public class Review
    {
        public string Author_Name { get; set; }
        public string Author_Url { get; set; }
        public string Language { get; set; }
        public string Profile_Photo_Url { get; set; }
        public int Rating { get; set; }
        public string Relative_Time_Description { get; set; }
        public string Text { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Time { get; set; }
    }
}
