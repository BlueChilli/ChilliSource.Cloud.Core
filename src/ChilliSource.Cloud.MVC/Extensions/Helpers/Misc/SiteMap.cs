
using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace ChilliSource.Cloud.Web.MVC.Misc
{
    /// <summary>
    /// Represents an item in sitemap XML file.
    /// </summary>
    public class SitemapItem : ISitemapItem
    {
        /// <summary>
        /// Initialize a new instance of BlueChilli.Web.Misc.SitemapItem with specified URL.
        /// </summary>
        /// <param name="url"></param>
        public SitemapItem(string url)
        {
            Url = UriExtensions.Parse(url).AbsoluteUri;
        }

        /// <summary>
        /// Gets or sets URL.
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Gets or sets last modified date and time.
        /// </summary>
        public DateTime? LastModified { get; set; }
        /// <summary>
        /// Gets or sets page change frequency.
        /// </summary>
        public SiteMapChangeFrequency ChangeFrequency { get; set; }
        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        public decimal? Priority { get; set; }
    }

    /// <summary>
    /// Interface for item in sitemap XML file.
    /// </summary>
    public interface ISitemapItem
    {
        /// <summary>
        /// Gets the URL of page.
        /// </summary>
        string Url { get; }
        /// <summary>
        /// Gets last modified date and time.
        /// </summary>
        DateTime? LastModified { get; }
        /// <summary>
        /// Gets page change frequency.
        /// </summary>
        SiteMapChangeFrequency ChangeFrequency { get; }
        /// <summary>
        /// Gets the priority.
        /// </summary>
        decimal? Priority { get; }
    }

    /// <summary>
    /// Represents the sitemap XML.
    /// </summary>
    public static class XmlSitemap
    {
        private const string Url = "url";
        private const string UrlSet = "urlset";
        private const string UrlSetSchemaLocation = "schemaLocation";
        private const string UrlXsi = "xsi";
        private const string UrlSetSchemaLocationUrl = "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd";

        private static readonly XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        private static readonly XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

        /// <summary>
        /// Creates sitemap XML from specified collection of sitemap items.
        /// </summary>
        /// <param name="items">A collection of sitemap items</param>
        /// <returns>An XML string of the sitemap.</returns>
        public static string Create(IEnumerable<ISitemapItem> items)
        {
            XDocument xDoc = new XDocument(
                new XDeclaration("1.0", HttpContext.Current.Response.ContentEncoding.WebName, "yes"),
                new XElement(xmlns + UrlSet,
                    new XAttribute(XNamespace.Xmlns + UrlXsi, xsi),
                    new XAttribute(xsi + UrlSetSchemaLocation, UrlSetSchemaLocationUrl),
                    items.Select(i => CreateItemElement(i))
                )
            );

            return (xDoc.Declaration + Environment.NewLine + xDoc.ToString());
        }

        private static XElement CreateItemElement(ISitemapItem item)
        {
            var itemElement = new XElement(xmlns + "url");

            itemElement.Add(new XElement(xmlns + "loc", item.Url.ToLower()));

            if (item.LastModified.HasValue)
                itemElement.Add(new XElement(xmlns + "lastmod", item.LastModified.Value.ToString("yyyy-MM-dd")));

            if (item.ChangeFrequency != SiteMapChangeFrequency.None)
                itemElement.Add(new XElement(xmlns + "changefreq", item.ChangeFrequency.GetDescription().ToLower()));

            if (item.Priority.HasValue)
                itemElement.Add(new XElement(xmlns + "priority", item.Priority.Value.ToString(CultureInfo.InvariantCulture)));

            return itemElement;
        }
    }

    /// <summary>
    /// Enumeration values for BlueChilli.Web.Misc.SiteMapChangeFrequency.
    /// </summary>
    public enum SiteMapChangeFrequency
    {
        /// <summary>
        /// Change frequency has not been specified.
        /// </summary>
        None = 0,
        /// <summary>
        /// Page never updates.
        /// </summary>
        Never,
        /// <summary>
        /// Page updates constantly.
        /// </summary>
        Always,
        /// <summary>
        /// Page updates each hour.
        /// </summary>
        Hourly,
        /// <summary>
        /// Page updates every day.
        /// </summary>
        Daily,
        /// <summary>
        /// Page updates each week.
        /// </summary>
        Weekly,
        /// <summary>
        /// Page updates each month.
        /// </summary>
        Monthly,
        /// <summary>
        /// Page updates each year.
        /// </summary>
        Yearly
    }
}