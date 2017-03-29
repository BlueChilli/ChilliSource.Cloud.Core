using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace ChilliSource.Cloud.Web.MVC
{
    public static partial class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns HTML string for date picker containing three drop down lists for year, month and day.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProperty">The property of the value.</typeparam>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the model.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes.</param>
        /// <returns>An HTML string for date picker.</returns>
        public static MvcHtmlString DatePickerFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlAttributes = null)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            DateTime? value = metadata.Model.ToNullable<DateTime>();

            var days = Enumerable.Range(1, 31).Select(i => new SelectListItem { Text = i.ToString(), Value = i.ToString(), Selected = value.HasValue && value.Value.Day == i }).ToList();
            var months = Enumerable.Range(1, 12).Select(i => new SelectListItem { Text = GetMonthName(i, true), Value = i.ToString(), Selected = value.HasValue && value.Value.Month == i }).ToList();

            int yearStart = DateTime.Now.AddYears((metadata.AdditionalValues.SingleOrDefault(m => m.Key == "DateYearRange_YearLow").Value as int?).GetValueOrDefault(-100)).Year;
            int yearCount = (metadata.AdditionalValues.SingleOrDefault(m => m.Key == "DateYearRange_YearCount").Value as int?).GetValueOrDefault(200);
            var years = Enumerable.Range(yearStart, yearCount).Select(i => new SelectListItem { Text = i.ToString(), Value = i.ToString(), Selected = value.HasValue && value.Value.Year == i }).ToList();
            if ((metadata.AdditionalValues.SingleOrDefault(m => m.Key == "DateYearRange_IsReversed").Value as bool?).GetValueOrDefault())
                years.Reverse();

            var hours = Enumerable.Range(0, 24).Select(i => new SelectListItem { Text = i.ToString(), Value = i.ToString(), Selected = value.HasValue && value.Value.Hour == i }).ToList();
            var minutes = Enumerable.Range(0, 60).Select(i => new SelectListItem { Text = i.ToString(), Value = i.ToString(), Selected = value.HasValue && value.Value.Minute == i }).ToList();

            if (metadata.IsNullableValueType)
            {
                days.Insert(0, SingleEmptyItem[0]);
                months.Insert(0, SingleEmptyItem[0]);
                years.Insert(0, SingleEmptyItem[0]);
                hours.Insert(0, SingleEmptyItem[0]);
                minutes.Insert(0, SingleEmptyItem[0]);
            }

            // can't use built in helpers because they try to bind to the composite model and ignore the selected item
            bool isCustomCss = GetMetaData(metadata, "IsCustomCss", false);
            string result = "";
            string expressionPropertyName = html.ViewData.TemplateInfo.GetFullHtmlFieldName(expression.GetExpressionText());
            if (GetMetaData(metadata, "ShowDay", true))
            {
                result += GetDropDownHtml(html, expressionPropertyName, days, htmlAttributes, isCustomCss ? "input-day" : "input-mini") + "&nbsp;";
            }
            if (GetMetaData(metadata, "ShowMonth", true))
            {
                result += GetDropDownHtml(html, expressionPropertyName, months, htmlAttributes, isCustomCss ? "input-month" : "input-mini") + "&nbsp;";
            }
            if (GetMetaData(metadata, "ShowYear", true))
            {
                result += GetDropDownHtml(html, expressionPropertyName, years, htmlAttributes, isCustomCss ? "input-year" : "input-small") + "&nbsp;";
            }
            if (GetMetaData(metadata, "ShowHour", false))
            {
                result += " : &nbsp;" + GetDropDownHtml(html, expressionPropertyName, hours, htmlAttributes, isCustomCss ? "input-hour" : "input-mini") + "&nbsp;";
            }
            if (GetMetaData(metadata, "ShowMinute", false))
            {
                result += GetDropDownHtml(html, expressionPropertyName, minutes, htmlAttributes, isCustomCss ? "input-minute" : "input-mini") + "&nbsp;";
            }
            return MvcHtmlString.Create(result.TrimEnd("&nbsp;"));
        }

        private static bool GetMetaData(ModelMetadata metadata, string name, bool defaultAs = false)
        {
            return (metadata.AdditionalValues.SingleOrDefault(m => m.Key == "Date" + name).Value as bool?).GetValueOrDefault(defaultAs);
        }

        private static string GetMonthName(int month, bool abbreviate)
        {
            DateTimeFormatInfo info = DateTimeFormatInfo.CurrentInfo;
            return abbreviate ? info.GetAbbreviatedMonthName(month) : info.GetMonthName(month);
        }

        private static string GetDropDownHtml(HtmlHelper html, string name, List<SelectListItem> items, object htmlAttributes, string inputSize)
        {
            var wrapper = new TagBuilder("div");
            wrapper.AddCssClass("styled-select");
            var select = new TagBuilder("select");
            select.AddCssClass(inputSize);
            select.Attributes["name"] = name;
            select.MergeAttributes(html.GetUnobtrusiveValidationAttributes(name));
            select.Attributes["data-val-date"] = "";
            select.Attributes.Remove("data-val-date");
            if (htmlAttributes != null) select.MergeAttributes(new RouteValueDictionary(htmlAttributes));

            StringBuilder options = new StringBuilder();
            foreach (SelectListItem item in items)
            {
                TagBuilder option = new TagBuilder("option");
                option.Attributes["value"] = item.Value;
                option.SetInnerText(item.Text);
                if (item.Selected)
                    option.Attributes["selected"] = "selected";

                options.Append(option.ToString(TagRenderMode.Normal));
            }

            select.InnerHtml = options.ToString();
            wrapper.InnerHtml = select.ToString(TagRenderMode.Normal) + @"<div class=""arrow""></div>";
            return wrapper.ToString(TagRenderMode.Normal);
        }
    }
}