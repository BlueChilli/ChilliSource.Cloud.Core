using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ChilliSource.Cloud.Web;

namespace ChilliSource.Cloud.Web.MVC
{
    /// <summary>
    /// Defines a template for json responses, which may contain errors.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JsonResponseDto<T>
    {
        protected JsonResponseDto() { this.Errors = new List<KeyValuePair<string, string[]>>(); }

        private bool _Success = true;
        public bool Success
        {
            get
            {
                if (this.Errors != null && this.Errors.Count > 0)
                    return false;
                return _Success;
            }
            set { _Success = value; }
        }

        public List<KeyValuePair<string, string[]>> Errors { get; set; }

        public T Data { get; set; }

        public static JsonResponseDto<T> FromModelState(ModelStateDictionary modelState)
        {
            var response = new JsonResponseDto<T>();
            var errors = modelState.Errors();
            if (errors != null)
                response.Errors.AddRange(errors);

            return response;
        }

        internal static JsonResponseDto<T> FromModelState(ModelStateDictionary ModelState, T data)
        {
            var dto = JsonResponseDto<T>.FromModelState(ModelState);
            dto.Data = data;
            return dto;
        }

        public static JsonResponseDto<T> AsError(string errorMessage, T data = default(T))
        {
            var response = new JsonResponseDto<T>()
            {
                Success = false,
                Data = data
            };

            if (!String.IsNullOrEmpty(errorMessage))
            {
                response.Errors.Add(new KeyValuePair<string, string[]>("", new string[] { errorMessage }));
            }

            return response;
        }

        public static JsonResponseDto<T> AsSuccess(T data = default(T))
        {
            return new JsonResponseDto<T>() { Success = true, Data = data };
        }
    }

    public class JsonResponseDto : JsonResponseDto<object>
    {
    }
}
