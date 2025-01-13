using App.Core.Enums;
using App.Core.Result;
using Microsoft.AspNetCore.Mvc;

namespace IdenitityServer.Extensions
{
    public static class ResultExtensions
    {
        public static ActionResult ConvertToActionResult<T>(this Result<T> result, ControllerBase controllerBase)
        {

            switch (result.StatusCode)
            {
                case StatusCode.Success: return controllerBase.Ok(result);
                case StatusCode.UnAuthorized: return controllerBase.Unauthorized();
                case StatusCode.BadRequest: return BadRequest(controllerBase, result);
                case StatusCode.UnAuthenticated: return controllerBase.Unauthorized();
                case StatusCode.NotFound: return controllerBase.NotFound();
                case StatusCode.AlreadyExist: return controllerBase.Conflict(result);
                case StatusCode.InternalError: return controllerBase.StatusCode(StatusCodes.Status500InternalServerError, result);
                default: return new ObjectResult((StatusCodes.Status500InternalServerError));
            }
        }
        public static ActionResult BadRequest<T>(ControllerBase controllerBase, Result<T> result)
        {
            return controllerBase.BadRequest(result);
        }
    }
}
