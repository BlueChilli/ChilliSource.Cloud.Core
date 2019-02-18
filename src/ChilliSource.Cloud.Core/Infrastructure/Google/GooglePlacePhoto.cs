#if NET_4X
using System;
using System.Drawing;
using System.Net;

namespace ChilliSource.Cloud.Core
{

    public class GooglePlacePhotoRequest
    {
        public const string MapUri = "https://maps.googleapis.com/maps/api/place/photo?maxwidth={0}&maxheight={1}&photoreference={2}&key={3}";

        public string ApiKey { get; set; }

        public Image Search(string photoId, int? width, int? height)
        {
            var uri = String.Format(MapUri, width, height, photoId, ApiKey);
            var httpRequest = (HttpWebRequest)HttpWebRequest.Create(uri);
            httpRequest.ContentType = "application/json; charset=utf-8";
            httpRequest.Method = WebRequestMethods.Http.Get;
            httpRequest.Accept = "application/json";

            using (HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse())
            {
                var image = Image.FromStream(httpResponse.GetResponseStream());
                return image;
            }
        }

    }

}
#endif