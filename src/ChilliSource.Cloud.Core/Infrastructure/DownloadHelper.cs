using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    public static class DownloadHelper
    {
        const string USER_AGENT = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/33.0.1750.154 Safari/537.36";

        public static DownloadedData GetData(string url)
        {
            return TaskHelper.GetResultSafeSync(() => GetDataAsync(url));
        }

        public static async Task<DownloadedData> GetDataAsync(string url)
        {
            var result = new DownloadedData();
            using (var client = new WebClientHelper())
            {
                client.Headers.Add("user-agent", USER_AGENT);
                try
                {
                    result.Data = await client.DownloadDataTaskAsync(url);
                    result.HttpStatusCode = client.HttpStatusCode;
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    result.Data = null;
                }

                result.ContentType = client.ResponseHeaders["Content-Type"];
            }

            return result;
        }
    }

    public class DownloadedData
    {
        public HttpStatusCode? HttpStatusCode { get; internal set; }
        public byte[] Data { get; internal set; }
        public string ContentType { get; internal set; }
        public bool HasOkStatus() { return this.Exception == null && this.Data != null && this.HttpStatusCode == System.Net.HttpStatusCode.OK; }

        public Exception Exception { get; internal set; }
    }
}
