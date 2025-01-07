using App.Contract.Dto;
using App.Core.Domain.Result;

public interface IAccountService
{
    Task<Result<string>> ChangePasswordAsync(SetupPasswordModel model);
    Task<Result<string>> SendForgetPasswordAsync(ResetPasswordModel model);
    Task<Result<string>> RegisterNewUserAsync(RegisterModel model);
    Task<Result<string>> ReSendOTPAsync(string userName);
    Task<Result<UserProfile>> GetProfileAsync(string userId);
    Task<Result<string>> AssignRolesAsync(string userId, List<string> roles);
    Task<Result<List<string>>> GetRolesAsync();
    Task<Result<string>> CreateRoleAsync(string roleName);
}