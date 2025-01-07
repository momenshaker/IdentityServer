using App.Core.Domain.Interfaces;
using App.Core.Enums;

namespace App.Core.Domain.Result
{
    public class Result<T> : IResult<T>
    {
        public string Version { get; set; } = "1.0";
        public StatusCode StatusCode { get; set; }
        public List<string>? ErrorMessages { get; set; }
        public T Data { get; set; }
        public string? SuccessMessage { get; set; } = null;
        public DateTime TimeStamp { get; set; } = DateTime.Now;

        public Result(T data)
        {
            Data = data;
        }

        public void Success(string successMessage)
        {
            StatusCode = StatusCode.Success;
            SuccessMessage = successMessage;
        }
        public StatusCode GetStatus(int status)
        {
            switch (status)
            {
                case 200:
                    return StatusCode.Success;
                case 204:
                    return StatusCode.NoContent;
                case 400:
                    return StatusCode.BadRequest;
                case 403:
                    return StatusCode.UnAuthorized;
                case 401:
                    return StatusCode.UnAuthenticated;
                case 404:
                    return StatusCode.NotFound;
                case 500:
                    return StatusCode.InternalError;
                case 409:
                    return StatusCode.AlreadyExist;
                default:
                    return StatusCode.InternalError;
            }
        }
    }
}
