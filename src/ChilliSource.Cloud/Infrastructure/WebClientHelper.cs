using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud
{
    public class WebClientHelper : WebClient
    {
        WebResponse _response;

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                ((HttpWebRequest)request).KeepAlive = false;
            }

            return request;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            return Set(base.GetWebResponse(request));
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            return Set(base.GetWebResponse(request, result));
        }

        private WebResponse Set(WebResponse value)
        {
            _response = value;
            return value;
        }

        public HttpStatusCode? HttpStatusCode
        {
            get
            {
                var httpResponse = _response as HttpWebResponse;
                if (httpResponse != null)
                {
                    return httpResponse.StatusCode;
                }

                return null;
            }
        }
    }
}
