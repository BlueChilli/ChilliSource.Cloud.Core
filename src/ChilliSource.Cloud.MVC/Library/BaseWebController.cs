
using ChilliSource.Cloud.Core;
using System;
using System.Diagnostics;
using System.Net;
using System.Web.Mvc;

namespace ChilliSource.Cloud.Web.MVC
{
    public class BaseWebController : Controller
    {
        public ViewNamingConvention ViewNamingConvention { get; set; }


        protected override ITempDataProvider CreateTempDataProvider()
        {
            return new CookieTempDataProvider(useEncryption: true);
        }

        [DebuggerNonUserCode]
        public IServiceCallerSyntax<T> ServiceCall<T>(Func<ServiceResult<T>> action)
        {
            return (new ServiceCaller<T>(this)).SetAction(action);
        }

        [DebuggerNonUserCode]
        internal class ServiceCaller<T> : IServiceCallerSyntax<T>
        {
            private Func<ServiceResult<T>> _action;

            private Func<T, ActionResult> _onFailure;

            private Func<T, ActionResult> _onSuccess;

            private readonly BaseWebController _controller;

            private bool IgnoreModelState
            {
                get { return (_controller.HttpContext.Items["ServiceCaller_IsModelStateEvaluated"] as bool?).GetValueOrDefault(false); }
                set { _controller.HttpContext.Items["ServiceCaller_IsModelStateEvaluated"] = true; }
            }

            public ServiceCaller(BaseWebController controller)
            {
                _controller = controller;

                //default action for success;
                if (controller.ViewNamingConvention == ViewNamingConvention.Default)
                {
                    this.OnSuccess((response) => _controller.View(response));
                }
                else if (controller.ViewNamingConvention == ViewNamingConvention.ControllerPrefix)
                {
                    var viewname = controller.RouteData.Values["controller"].ToString() + controller.RouteData.Values["action"].ToString();
                    this.OnSuccess((response) => _controller.View(viewname, response));
                }

                //default action for failure;
                _onFailure = _onSuccess;
            }

            [DebuggerNonUserCode]
            internal ServiceCaller<T> SetAction(Func<ServiceResult<T>> action)
            {
                _action = action;
                return this;
            }

            [DebuggerNonUserCode]
            public IServiceCallerSyntax<T> OnSuccess(Func<ActionResult> onSuccess)
            {
                _onSuccess = T => onSuccess();
                return this;
            }

            [DebuggerNonUserCode]
            public IServiceCallerSyntax<T> OnSuccess(Func<T, ActionResult> onSuccess)
            {
                _onSuccess = onSuccess;
                return this;
            }

            [DebuggerNonUserCode]
            public IServiceCallerSyntax<T> OnFailure(Func<ActionResult> onFailure)
            {
                _onFailure = T => onFailure();
                return this;
            }

            [DebuggerNonUserCode]
            public IServiceCallerSyntax<T> OnFailure(Func<T, ActionResult> onFailure)
            {
                _onFailure = onFailure;
                return this;
            }

            [DebuggerNonUserCode]
            public IServiceCallerSyntax<T> Always(Func<ActionResult> always)
            {
                _onSuccess = T => always();
                _onFailure = T => always();
                return this;
            }

            [DebuggerNonUserCode]
            public IServiceCallerSyntax<T> Always(Func<T, ActionResult> always)
            {
                _onSuccess = always;
                _onFailure = always;
                return this;
            }

            [DebuggerNonUserCode]
            public ActionResult Call()
            {
                if (_action == null ||
                   _onSuccess == null ||
                   _onFailure == null)
                {
                    throw new ApplicationException("You need to set up the service call, and either Always or OnSuccess and OnFailure actions");
                }

                var responseValue = default(T);
                if (!_controller.ModelState.IsValid && !IgnoreModelState)
                {
                    this.IgnoreModelState = true;
                    return _onFailure(responseValue);
                }
                var response = _action();
                responseValue = response.Result;
                if (!response.Success)
                {
                    response.AddToModelState(_controller);
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return new HttpNotFoundResult(response.Error);
                    }
                    this.IgnoreModelState = true;
                    return _onFailure(response.Result);
                }

                return _onSuccess(response.Result);
            }
        }
    }

    public interface IServiceCallerSyntax<T>
    {
        IServiceCallerSyntax<T> OnSuccess(Func<ActionResult> onSuccess);
        IServiceCallerSyntax<T> OnSuccess(Func<T, ActionResult> onSuccess);
        IServiceCallerSyntax<T> OnFailure(Func<ActionResult> onFailure);
        IServiceCallerSyntax<T> OnFailure(Func<T, ActionResult> onFailure);
        IServiceCallerSyntax<T> Always(Func<ActionResult> always);
        IServiceCallerSyntax<T> Always(Func<T, ActionResult> always);
        ActionResult Call();
    }

    public enum ViewNamingConvention
    {
        Default = 0,
        ControllerPrefix
    }
}