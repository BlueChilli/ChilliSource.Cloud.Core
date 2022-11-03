using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core.Security
{
    /// <summary>
    /// https://github.com/ThiagoBarradas/jsonmasking/blob/master/JsonMasking/JsonMasking.cs
    /// </summary>
    public static class JsonMasking
    {
        /// <summary>
        /// Mask fields
        /// </summary>
        /// <param name="json">json to mask properties</param>
        /// <param name="blacklist">insensitive property array</param>
        /// <param name="mask">mask to replace property value</param>
        /// <returns></returns>
        public static string MaskFields(this string json, string[] blacklist, string mask)
        {
            if (string.IsNullOrWhiteSpace(json) == true)
            {
                throw new ArgumentNullException(nameof(json));
            }

            if (blacklist == null)
            {
                throw new ArgumentNullException(nameof(blacklist));
            }

            if (blacklist.Any() == false)
            {
                return json;
            }

            try
            {
                var jsonObject = (JObject)JsonConvert.DeserializeObject(json);

                MaskFieldsFromJToken(jsonObject, blacklist, mask);

                return jsonObject.ToString(Formatting.None);
            }
            catch
            {
                return json;
            }
        }

        /// <summary>
        /// Mask fields from JToken
        /// </summary>
        /// <param name="token"></param>
        /// <param name="blacklist"></param>
        /// <param name="mask"></param>
        private static void MaskFieldsFromJToken(JToken token, string[] blacklist, string mask)
        {
            JContainer container = token as JContainer;
            if (container == null)
            {
                return; // abort recursive
            }

            List<JToken> removeList = new List<JToken>();
            foreach (JToken jtoken in container.Children())
            {
                var matching = false;
                if (jtoken is JProperty prop)
                {
                    matching = blacklist.Any(item =>
                    {
                        return String.Equals(prop.Name, item, StringComparison.OrdinalIgnoreCase);
                    });

                    if (matching)
                    {
                        removeList.Add(jtoken);
                    }
                }

                // call recursive 
                if (!matching) MaskFieldsFromJToken(jtoken, blacklist, mask);
            }

            // replace 
            foreach (JToken el in removeList)
            {
                var prop = (JProperty)el;
                prop.Value = mask;
            }
        }

    }
}
