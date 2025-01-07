using App.Contract.Dto;
using App.Core.Domain.Result;
using App.Services.Services;
using IdenitityServer.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdenitityServer.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var userResult = await _accountService.RegisterNewUserAsync(model);
                return userResult.ConvertToActionResult(this);
            }

            var result = CreateBadRequestResult();
            return result.ConvertToActionResult(this);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> UpdatePassword([FromBody] SetupPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var userResult = await _accountService.ChangePasswordAsync(model);
                return userResult.ConvertToActionResult(this);
            }

            var result = CreateBadRequestResult();
            return result.ConvertToActionResult(this);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var userResult = await _accountService.SendForgetPasswordAsync(model);
                return userResult.ConvertToActionResult(this);
            }

            var result = CreateBadRequestResult();
            return result.ConvertToActionResult(this);
        }

        [HttpPost]
        public async Task<IActionResult> SendOTPToken(string userName)
        {
            var userResult = await _accountService.ReSendOTPAsync(userName);
            return userResult.ConvertToActionResult(this);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            var userResult = await _accountService.GetProfileAsync(User.Claims.FirstOrDefault()?.Value);
            return userResult.ConvertToActionResult(this);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AssignRoles([FromBody] AssignRolesDto assignRolesDto)
        {
            if (ModelState.IsValid)
            {
                var userResult = await _accountService.AssignRolesAsync(assignRolesDto.userId, assignRolesDto.roles);
                return userResult.ConvertToActionResult(this);
            }

            var result = CreateBadRequestResult();
            return result.ConvertToActionResult(this);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateRole([FromBody] string role)
        {
            if (ModelState.IsValid)
            {
                var userResult = await _accountService.CreateRoleAsync(role);
                return userResult.ConvertToActionResult(this);
            }

            var result = CreateBadRequestResult();
            return result.ConvertToActionResult(this);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetRoles()
        {
            if (ModelState.IsValid)
            {
                var userResult = await _accountService.GetRolesAsync();
                return userResult.ConvertToActionResult(this);
            }

            var result = CreateBadRequestResult();
            return result.ConvertToActionResult(this);
        }

        // Helper method to create a standardized bad request response
        private Result<bool> CreateBadRequestResult()
        {
            var errorMessages = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            var result = new Result<bool>(false)
            {
                StatusCode = App.Core.Enums.StatusCode.BadRequest,
                ErrorMessages = errorMessages
            };

            return result;
        }
    }
}
