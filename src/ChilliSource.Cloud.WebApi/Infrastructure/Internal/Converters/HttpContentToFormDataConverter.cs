using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Web.Api.Internal
{
    internal class HttpContentToFormDataConverter
    {
        public async Task<FormData> Convert(HttpContent content)
        {
            if(content == null)
                throw new ArgumentNullException("content");

            if (!content.IsMimeMultipartContent())
            {
                throw new Exception("Unsupported Media Type");
            }

            MultipartMemoryStreamProvider multipartProvider = await content.ReadAsMultipartAsync();

            var multipartFormData = await Convert(multipartProvider);
            return multipartFormData;
        }

        public async Task<FormData> Convert(MultipartMemoryStreamProvider multipartProvider)
        {
            var multipartFormData = new FormData();

            foreach (var file in multipartProvider.Contents.Where(x => IsFile(x.Headers.ContentDisposition)))
            {
                var name = UnquoteToken(file.Headers.ContentDisposition.Name);
                string fileName = FixFilename(file.Headers.ContentDisposition.FileName);
                string mediaType = file.Headers.ContentType.MediaType;

                var stream = await file.ReadAsStreamAsync();
                multipartFormData.Add(name, new HttpFile(fileName, mediaType, stream));

                //using (var stream = await file.ReadAsStreamAsync())
                //{
                //    byte[] buffer = ReadAllBytes(stream);
                //    if (buffer.Length > 0)
                //    {
                //        multipartFormData.Add(name, new HttpPostedFileBase(fileName, mediaType, buffer));
                //    }
                //}
            }

            foreach (var part in multipartProvider.Contents.Where(x => x.Headers.ContentDisposition.DispositionType == "form-data"
                                                                  && !IsFile(x.Headers.ContentDisposition)))
            {
                var name = UnquoteToken(part.Headers.ContentDisposition.Name);
                var data = await part.ReadAsStringAsync();
                multipartFormData.Add(name, data);
            }

            return multipartFormData;
        } 

        private bool IsFile(ContentDispositionHeaderValue disposition)
        {
            return !string.IsNullOrEmpty(disposition.FileName);
        }

        /// <summary>
        /// Remove bounding quotes on a token if present
        /// </summary>
        private static string UnquoteToken(string token)
        {
            if (String.IsNullOrWhiteSpace(token))
            {
                return token;
            }

            if (token.StartsWith("\"", StringComparison.Ordinal) && token.EndsWith("\"", StringComparison.Ordinal) && token.Length > 1)
            {
                return token.Substring(1, token.Length - 2);
            }

            return token;
        }

        /// <summary>
        /// Amend filenames to remove surrounding quotes and remove path from IE
        /// </summary>
        private static string FixFilename(string originalFileName)
        {
            if (string.IsNullOrWhiteSpace(originalFileName))
                return string.Empty;

            var result = originalFileName.Trim();
            
            // remove leading and trailing quotes
            result = result.Trim('"');

            // remove full path versions
            if (result.Contains("\\"))
                result = Path.GetFileName(result);

            return result;
        }

        //private byte[] ReadAllBytes(Stream input)
        //{
        //    using (var stream = new MemoryStream())
        //    {
        //        input.CopyTo(stream);
        //        return stream.ToArray();
        //    }
        //}
    }
}
