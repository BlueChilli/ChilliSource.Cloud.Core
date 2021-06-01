using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// Represents a response from the Service Layer.
    /// </summary>
    /// <typeparam name="T">Result class to be returned along with the response</typeparam>
    public class ServiceResult<T>
    {
        public ServiceResult()
        {
            Error = "";
            StatusCode = HttpStatusCode.OK;
        }

        /// <summary>
        /// Specifies whether the service call was successful.
        /// </summary>
        public virtual bool Success { get; set; } //Virtual property to allow us to keep ServiceResult.Success property with the same behavior as before.
        /// <summary>
        /// Error message from the service
        /// </summary>
        public string Error { get; set; }
        /// <summary>
        /// Result object returned along with the response
        /// </summary>
        public virtual T Result { get; set; }
        /// <summary>
        /// ViewModel Key to link an error to a ViewModel property.
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Http Status code from the service.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }
        /// <summary>
        /// Creates a successful response
        /// </summary>
        /// <param name="result">(Optional) result object</param>
        /// <returns>A successful ServiceResult</returns>
        public static ServiceResult<T> AsSuccess(T result = default(T))
        {
            return new ServiceResult<T>() { Success = true, Result = result };
        }

        /// <summary>
        /// Creates an error response
        /// </summary>
        /// <param name="error">Error message</param>
        /// <param name="statusCode">http status as optional</param>
        /// <returns>An error response</returns>
        public static ServiceResult<T> AsError(string error = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new ServiceResult<T>() { Success = false, Result = default(T), Error = error, StatusCode = statusCode };
        }

        /// <summary>
        /// Creates an error response
        /// </summary>
        /// <param name="result">(Optional) result object</param>
        /// <param name="error">Error message</param>
        /// <param name="statusCode">http status as optional</param>
        /// <returns>An error response</returns>
        public static ServiceResult<T> AsError(T result, string error = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new ServiceResult<T>() { Success = false, Result = result, Error = error, StatusCode = statusCode };
        }

        /// <summary>
        /// Copies the response from one ServiceResult instance to another.
        /// The destination result object can be different from origin but it will copied over only when they are compatible (if origin can be cast to destination)
        /// </summary>
        /// <typeparam name="Y">Original result class</typeparam>
        /// <param name="other">Original ServiceResult instance</param>
        /// <returns>The ServiceResult copy</returns>
        public static ServiceResult<T> CopyFrom<Y>(ServiceResult<Y> other)
        {
            var copy = new ServiceResult<T>() { Success = other.Success, Error = other.Error, StatusCode = other.StatusCode, Key = other.Key };
            if (other.Result is T)
                copy.Result = (T)(object)other.Result;

            return copy;
        }

        /// <summary>
        /// Copies only the status and error message from one ServiceResult to another.
        /// The result object can also be specified.
        /// </summary>
        /// <typeparam name="Y">Original result class</typeparam>
        /// <param name="other">Original ServiceResult instance</param>
        /// <param name="result">Result object to be return along with the response.</param>
        /// <returns>The ServiceResult copy</returns>
        public static ServiceResult<T> CopyFrom<Y>(ServiceResult<Y> other, T result)
        {
            var obj = CopyFrom(other);
            obj.Result = result;
            return obj;
        }

        public static ServiceResult<T> CopyFrom<Y>(ServiceResult<Y> other, Func<Y, T> resultFunc)
        {
            var obj = CopyFrom(other);
            if (other.Result != null)
            {
                obj.Result = resultFunc(other.Result);
            }
            return obj;
        }

    }

    /// <summary>
    /// Represents a simple response from the Service Layer (not attached response object)
    /// </summary>
    public class ServiceResult : ServiceResult<object> //ServiceResult inherits from ServiceResult<object>  so that we can create reusable code.
    {
        public ServiceResult() : base() { }

        private bool _success;
        /// <summary>
        /// Specifies whether the service call was successful.
        /// If Error is set, this will always return false.
        /// </summary>
        public override bool Success
        {
            get { return String.IsNullOrEmpty(Error) ? _success : false; }
            set { _success = value; }
        }

        /// <summary>
        /// Copies the response from one ServiceResult instance to another. The additional result object is ignored.
        /// </summary>
        /// <param name="other">Original ServiceResult instance</param>
        /// <returns>The ServiceResult copy</returns>
        public static ServiceResult CopyFrom<T>(ServiceResult<T> other)
        {
            //Ignores result.
            return new ServiceResult() { Success = other.Success, Error = other.Error, StatusCode = other.StatusCode, Key = other.Key };
        }

        /// <summary>
        /// Creates a successful response
        /// </summary>
        /// <returns>A successful response</returns>
        public static ServiceResult AsSuccess()
        {
            return new ServiceResult() { Success = true };
        }

        /// <summary>
        /// Creates an error response
        /// </summary>
        /// <param name="error">Error message</param>
        /// <returns>An error response</returns>
        public new static ServiceResult AsError(string error = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new ServiceResult() { Success = false, Error = error, StatusCode = statusCode };
        }
    }
}
