using ChilliSource.Cloud.Core;
using System;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Defines methods to run when object created or disposed.
    /// </summary>
    internal class DisposableWrapper : IDisposable
    {
        private Action end;

        /// <summary>
        /// Initialise the object and run "begin" function.
        /// </summary>
        /// <param name="begin">Function to run when object created.</param>
        /// <param name="end">Function to run when object disposed.</param>
        public DisposableWrapper(Action begin, Action end)
        {
            this.end = end;
            begin();
        }

        /// <summary>
        /// When the object is disposed (end of using block), runs "end" function
        /// </summary>
        public void Dispose()
        {
            end();
        }
    }

    public static partial class HtmlHelperExtensions
    {
        /// <summary>
        /// Returns BlueChilli.Web.DisposableWrapper object to write HTML label begin tag when created and to write HTML label end tag when disposed.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value of the model.</typeparam>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="labelExpression">An expression that identifies the model.</param>
        /// <param name="fieldOptions">An object that contains additional options for label field.</param>
        /// <returns>A BlueChilli.Web.DisposableWrapper object.</returns>
        public static IDisposable FieldOuterFor<TModel, TValue>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TValue>> labelExpression, FieldOptions fieldOptions = null)
        {
            return new DisposableWrapper(
                () => htmlHelper.ViewContext.Writer.Write(htmlHelper.FieldOuterForBegin(labelExpression, fieldOptions: fieldOptions)),
                () => htmlHelper.ViewContext.Writer.Write(htmlHelper.FieldOuterForEnd(fieldOptions))
            );
        }

        /// <summary>
        /// Returns BlueChilli.Web.DisposableWrapper object to write HTML "&lt;div class='control-group'&gt;&lt;div class='controls'&gt;" when created and to write HTML "&lt;/div&gt;&lt;/div&gt;" when disposed.
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <returns>A BlueChilli.Web.DisposableWrapper object.</returns>
        public static IDisposable FieldOuterFor(this HtmlHelper htmlHelper)
        {
            return new DisposableWrapper(
                () => htmlHelper.ViewContext.Writer.Write(htmlHelper.FieldOuterForBegin()),
                () => htmlHelper.ViewContext.Writer.Write(htmlHelper.FieldOuterForEnd(null))
            );
        }

        /// <summary>
        /// Returns BlueChilli.Web.DisposableWrapper object to write scripts "$(function () {" when created and to write scripts "});" when disposed.
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <returns>A BlueChilli.Web.DisposableWrapper object.</returns>
        public static IDisposable ScriptOnJqueryReady(this HtmlHelper htmlHelper)
        {
            return new DisposableWrapper(
                () => htmlHelper.ViewContext.Writer.Write(htmlHelper.ScriptOnJqueryReadyStart()),
                () => htmlHelper.ViewContext.Writer.Write(htmlHelper.ScriptOnJqueryReadyEnd())
            );
        }

        /// <summary>
        /// Returns BlueChilli.Web.DisposableWrapper object to write HTML link begin tag when created and to write HTML link end tag when disposed.
        /// </summary>
        /// <param name="htmlHelper">The System.Web.Mvc.HtmlHelper instance that this method extends.</param>
        /// <param name="link">An HTML-encoded link.</param>
        /// <returns>A BlueChilli.Web.DisposableWrapper object.</returns>
        public static IDisposable BeginLink(this HtmlHelper htmlHelper, MvcHtmlString link)
        {
            return new DisposableWrapper(
                () => htmlHelper.ViewContext.Writer.Write(link.ToHtmlString().TrimEnd("</a>")),
                () => htmlHelper.ViewContext.Writer.Write("</a>")
            );
        }
    }
}