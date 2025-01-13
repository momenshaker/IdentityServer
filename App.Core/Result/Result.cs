using App.Core.Enums;
using App.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace App.Core.Result
{
    /// <summary>
    /// Represents the result of an operation, including status, data, and error messages.
    /// </summary>
    /// <typeparam name="T">The type of the data being returned in the result.</typeparam>
    public class Result<T> : IResult<T>
    {
        /// <summary>
        /// Gets or sets the version of the result. Default is "1.0".
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// Gets or sets the status code indicating the result of the operation.
        /// </summary>
        public StatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets a list of error messages if the operation failed.
        /// </summary>
        public List<string>? ErrorMessages { get; set; }

        /// <summary>
        /// Gets or sets the data returned from the operation, if successful.
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Gets or sets a success message, if applicable.
        /// </summary>
        public string? SuccessMessage { get; set; } = null;

        /// <summary>
        /// Gets or sets the timestamp when the result was created.
        /// </summary>
        public DateTime TimeStamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Initializes a new instance of the <see cref="Result{T}"/> class with the provided data.
        /// </summary>
        /// <param name="data">The data to be included in the result.</param>
        public Result(T data)
        {
            Data = data;
        }

        /// <summary>
        /// Marks the result as successful with the specified success message.
        /// </summary>
        /// <param name="successMessage">The success message to be included in the result.</param>
        public void Success(string successMessage)
        {
            StatusCode = StatusCode.Success;
            SuccessMessage = successMessage;
        }

        /// <summary>
        /// Maps an HTTP status code to the corresponding <see cref="StatusCode"/>.
        /// </summary>
        /// <param name="status">The HTTP status code to be mapped.</param>
        /// <returns>The corresponding <see cref="StatusCode"/>.</returns>
        public StatusCode GetStatus(int status)
        {
            return status switch
            {
                200 => StatusCode.Success,
                204 => StatusCode.NoContent,
                400 => StatusCode.BadRequest,
                403 => StatusCode.UnAuthorized,
                401 => StatusCode.UnAuthenticated,
                404 => StatusCode.NotFound,
                500 => StatusCode.InternalError,
                409 => StatusCode.AlreadyExist,
                _ => StatusCode.InternalError,
            };
        }
    }
}
