
using ChilliSource.Cloud.Core;
using System;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Encapsulates functions that assist in working with cookies.
    /// </summary>
    public class SecureCookie
    {
        private readonly CookieHelper helper;
        private readonly bool IsSecure;

        /// <summary>
        /// Initializes a new instance of the SecureCookie class by using the specified name, domain and isSecure flag.
        /// </summary>
        /// <param name="name">The cookie name.</param>
        /// <param name="isSecure">True by default, False, gets the value of the cookie protected.</param>
        /// <param name="domain">The domain name.</param>
        public SecureCookie(string name, bool isSecure = true, string domain = null)
        {
            helper = new CookieHelper(name, domain);
            IsSecure = isSecure;
        }

        /// <summary>
        /// Delete cookie.
        /// </summary>
        public void Delete()
        {
            helper.Delete();
        }

        /// <summary>
        /// Delete a cookie, the name of the cookie is needed.
        /// </summary>
        /// <param name="name">The cookie name.</param>
        public void Delete(string name)
        {
            helper.Delete(name);
        }

        /// <summary>
        /// Get browser cookies.
        /// </summary>
        /// <typeparam name="T">The type of cookie value.</typeparam>
        /// <param name="name">The cookie name.</param>
        /// <returns>Browser cookie.</returns>
        public T Get<T>(string name)
        {
            var value = helper.GetValue(name);
            if (value == null) return default(T);

            var valueProtected = Convert.FromBase64String(value);
            if (valueProtected.Length == 0) return default(T);
            var valueConverted = IsSecure ? MachineKey.Unprotect(valueProtected, helper.CookieName) : valueProtected;

            return valueConverted.To<T>();
        }

        /// <summary>
        /// Set browser cookies.
        /// </summary>
        /// <typeparam name="T">The type of cookie value.</typeparam>
        /// <param name="name">The cookie name.</param>
        /// <param name="value">The cookie value.</param>
        public void Set<T>(string name, T value)
        {
            var byteArray = value.ToByteArray();

            string strProtected = null;
            if (byteArray != null)
            {
                var valueProtected = IsSecure ? MachineKey.Protect(byteArray, helper.CookieName) : byteArray;
                strProtected = Convert.ToBase64String(valueProtected);
            }

            helper.SetValue(name, strProtected);
        }
    }

    /// <summary>
    /// Encapsulates functions that assist in working with cookies.
    /// </summary>
    public class CookieHelper
    {
        /// <summary>
        /// Read only string for cookie name.
        /// </summary>
        public readonly string CookieName;
        /// <summary>
        /// Read only string for domain name.
        /// </summary>
        public readonly string Domain;

        /// <summary>
        /// Initializes a new instance of the CookieHelper class by using the specified name and domain.
        /// </summary>
        /// <param name="name">The cookie name.</param>
        /// <param name="domain">The domain name.</param>
        public CookieHelper(string name, string domain = null)
        {
            CookieName = name;
            Domain = domain;
        }

        private void SetOrAddResponse(HttpCookie cookie)
        {
            if (GetResponseCookie() == null)
            {
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

        /// <summary>
        /// Delete cookie.
        /// </summary>
        public void Delete()
        {
            HttpCookie cookie = GetResponseCookie();
            if (cookie == null) cookie = new HttpCookie(CookieName);

            cookie.Expires = new DateTime(2000, 1, 1);

            SetOrAddResponse(cookie);
        }

        /// <summary>
        /// Delete cookie by cookie name.
        /// </summary>
        /// <param name="name">Cookie name.</param>
        public void Delete(string name)
        {
            HttpCookie cookie = GetResponseCookie();
            if (cookie == null)
            {
                cookie = GetRequestCookie() ?? new HttpCookie(CookieName);
            }
            cookie.Values.Set(name, null);

            SetOrAddResponse(cookie);
        }

        /// <summary>
        /// Set browser cookie.
        /// </summary>
        /// <param name="name">The cookie name.</param>
        /// <param name="value">The cookie value.</param>
        public void SetValue(string name, string value)
        {
            HttpCookie cookie = GetResponseCookie();
            if (cookie == null)
            {
                cookie = GetRequestCookie() ?? new HttpCookie(CookieName);
            }

            cookie.Values.Set(name, value);
            if (Domain != null) { cookie.Domain = Domain; }

            SetOrAddResponse(cookie);
        }

        private HttpCookie GetResponseCookie()
        {
            if (!HttpContext.Current.Response.Cookies.AllKeys.Contains(CookieName))
                return null;
            return HttpContext.Current.Response.Cookies[CookieName];
        }

        private HttpCookie GetRequestCookie()
        {
            if (!HttpContext.Current.Request.Cookies.AllKeys.Contains(CookieName))
                return null;
            return HttpContext.Current.Request.Cookies[CookieName];
        }

        /// <summary>
        /// Get cookie value.
        /// </summary>
        /// <param name="name">The cookie name.</param>
        /// <returns>The cookie value.</returns>
        public string GetValue(string name)
        {
            //Checks response first.
            var cookie = GetResponseCookie();
            if (cookie == null || String.IsNullOrWhiteSpace(cookie.Value))
            {
                cookie = GetRequestCookie();
                if (cookie == null || String.IsNullOrWhiteSpace(cookie.Value))
                    return null;
            }

            if (cookie.Values.AllKeys.Contains(name))
            {
                var value = cookie[name];
                return value;
            }

            return null;
        }
    }
}