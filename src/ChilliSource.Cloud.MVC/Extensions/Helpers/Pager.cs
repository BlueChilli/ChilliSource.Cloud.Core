using ChilliSource.Cloud.Core;
using ChilliSource.Cloud.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns HTML string for pager.
        /// </summary>
        /// <typeparam name="TModel">The type of the model</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <param name="currentPage">The page number for current page.</param>
        /// <param name="pageCount">Specifies how many items on one page.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes.</param>
        /// <param name="pagerOptions">Page options defined by BlueChilli.Web.Helpers.PagerOptions.</param>
        /// <returns>An HTML string for pager.</returns>
        public static MvcHtmlString PagerFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, int currentPage, int pageCount, object htmlAttributes = null, PagerOptions pagerOptions = null)
        {
            if (pageCount <= 1) return MvcHtmlString.Empty;
            if (pagerOptions == null) pagerOptions = new PagerOptions();
            string pagerId = pagerOptions.PagerId.HasValue ? String.Format(@" class=""pager{0}""", pagerOptions.PagerId.Value) : "";

            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);

            StringBuilder list = new StringBuilder();
            list.Append("<ul>");
            int start, finish;
            if (pageCount <= 7)
            {
                start = 1; finish = pageCount;
            }
            else
            {
                start = ((currentPage - 1) / 5) * 5 + 1;
                finish = pageCount == (start + 5) ? pageCount : Math.Min(start + 4, pageCount);
            }
            
            string listItemFormat = pagerOptions.ServerPaging ?
                @"<li{0}><a href=""" + HttpContext.Current.Request.Url.AddQuery(new { page = "{1}" }).AbsoluteUri + @"""{3}>{4}</a></li>"
                : @"<li{0}><a href=""javascript:void(0);"" data-page=""{1}"" data-control=""{2}""{3}>{4}</a></li>";

            if (start != 1)
                list.Append(listItemFormat.FormatWith(@" class=""previous""", start - 1, metadata.PropertyName, pagerId, pagerOptions.PreviousHtml.DefaultTo("«")));

            for (int i = start; i <= finish; i++)
            {
                list.Append(listItemFormat.FormatWith(i == currentPage ? @" class=""active""" : "", i, metadata.PropertyName, pagerId, i));
            }

            if (finish != pageCount)
                list.Append(listItemFormat.FormatWith(@" class=""next""", finish + 1, metadata.PropertyName, pagerId, pagerOptions.NextHtml.DefaultTo("»")));

            list.Append("</ul>");

            TagBuilder container = new TagBuilder("div");
            container.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            container.AddCssClass("pagination");
            container.InnerHtml = list.ToString();
            return new MvcHtmlString(container.ToString());
        }

        /// <summary>
        /// Represents page options.
        /// </summary>
        public class PagerOptions
        {
            /// <summary>
            /// Gets or sets page Id.
            /// </summary>
            public int? PagerId { get; set; }
            /// <summary>
            /// Gets or sets the HTML string for previous page link.
            /// </summary>
            public string PreviousHtml { get; set; }
            /// <summary>
            /// Gets or sets the HTML string for nest page link.
            /// </summary>
            public string NextHtml { get; set; }
            /// <summary>
            /// Gets or sets the CSS class.
            /// </summary>
            public string CssClass { get; set; }
            /// <summary>
            /// Indicates whether the paging is done on the server or not.
            /// </summary>
            public bool ServerPaging { get; set; }
            /// <summary>
            /// Indicates whether to preserve URL parameters or not.
            /// </summary>
            public bool PreserveUrlParameters { get; set; }
        }

        /// <summary>
        /// Returns HTML string for pager previous link.
        /// </summary>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="currentPage">The page number for current page.</param>
        /// <param name="pageCount">Specifies how many items on one page.</param>
        /// <param name="pagerOptions">Page options defined by BlueChilli.Web.Helpers.PagerOptions.</param>
        /// <returns>An HTML string for pager previous link.</returns>
        public static MvcHtmlString PagerLoopPrevious(this HtmlHelper html, int currentPage, int pageCount, PagerOptions pagerOptions = null)
        {
            if (pageCount <= 1) return MvcHtmlString.Empty;
            if (pagerOptions == null) pagerOptions = new PagerOptions();

            string format = @"<div class=""pagination-loop""><a href=""javascript:void(0);""{0} data-page=""{1}""><</a></div>";
            string pagerId = pagerOptions.PagerId.HasValue ? String.Format(@" class=""pager{0}""", pagerOptions.PagerId.Value) : "";

            return MvcHtmlString.Empty.Format(format, pagerId, currentPage == 1 ? pageCount : currentPage - 1);
        }

        /// <summary>
        /// Returns HTML string for pager next link.
        /// </summary>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="currentPage">The page number for current page.</param>
        /// <param name="pageCount">Specifies how many items on one page.</param>
        /// <param name="pagerOptions">Page options defined by BlueChilli.Web.Helpers.PagerOptions.</param>
        /// <returns>An HTML string for pager next link.</returns>
        public static MvcHtmlString PagerLoopNext(this HtmlHelper html, int currentPage, int pageCount, PagerOptions pagerOptions = null)
        {
            if (pageCount <= 1) return MvcHtmlString.Empty;
            if (pagerOptions == null) pagerOptions = new PagerOptions();

            string format = @"<div class=""pagination-loop""><a href=""javascript:void(0);""{0} data-page=""{1}"">></a></div>";
            string pagerId = pagerOptions.PagerId.HasValue ? String.Format(@" class=""pager{0}""", pagerOptions.PagerId.Value) : "";

            return MvcHtmlString.Empty.Format(format, pagerId, currentPage == pageCount ? 1 : currentPage + 1);
        }

        /// <summary>
        /// Returns HTML string for pager with next and back links.
        /// </summary>
        /// <typeparam name="TModel">The type of the model</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="html">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <param name="currentPage">The page number for current page.</param>
        /// <param name="pageCount">Specifies how many items on one page.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes.</param>
        /// <param name="pagerOptions">Page options defined by BlueChilli.Web.Helpers.PagerOptions.</param>
        /// <returns>An HTML string for pager with next and back links.</returns>
        public static MvcHtmlString PagerNextBackFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, int currentPage, int pageCount, object htmlAttributes = null, PagerOptions pagerOptions = null)
        {
            if (pageCount <= 1) return MvcHtmlString.Empty;
            if (pagerOptions == null) pagerOptions = new PagerOptions();
            string pagerId = pagerOptions.PagerId.HasValue ? String.Format(@" class=""pager{0}""", pagerOptions.PagerId.Value) : "";

            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);

            string template =  @"<a href=""{0}"" class=""{1}"" data-id=""{2}"">{3}</a>";
            var previousPage = (currentPage - 1).ToString();
            var nextPage = (currentPage + 1).ToString();
            var uri = html.ViewContext.RequestContext.HttpContext.Request.Url;

            var sb = new StringBuilder();
            if (currentPage > 1)
            {
                sb.AppendFormat(template, 
                    pagerOptions.ServerPaging 
                        ? pagerOptions.PreserveUrlParameters ? uri.AddQuery(new { page = previousPage }).ToString() : "?page=" + previousPage 
                        : "#Prev", 
                    "prev " + pagerOptions.CssClass, 
                    previousPage, 
                    pagerOptions.PreviousHtml.DefaultTo("«")
                );
            }
            if (currentPage < pageCount)
            {
                sb.AppendFormat(template, 
                    pagerOptions.ServerPaging
                        ? pagerOptions.PreserveUrlParameters ? uri.AddQuery(new { page = nextPage }).ToString() : "?page=" + nextPage
                        : "#Next", 
                    "next " + pagerOptions.CssClass, 
                    previousPage, 
                    pagerOptions.NextHtml.DefaultTo("»"));
            }

            var container = new TagBuilder("div");
            container.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            container.AddCssClass("pagination-next-back");
            container.InnerHtml = sb.ToString();
            return new MvcHtmlString(container.ToString());
        }
    }
}