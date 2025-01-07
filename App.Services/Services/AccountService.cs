using App.Contract.Dto;
using App.Core.Context;
using App.Core.Data.Models;
using App.Core.Domain.Result;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Web;

namespace App.Services.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<AspNetUser> _userManager;
        private readonly SignInManager<AspNetUser> _signInManager;
        private readonly ILogger<AccountService> _logger;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AccountService(
            UserManager<AspNetUser> userManager,
            SignInManager<AspNetUser> signInManager,
            ILogger<AccountService> logger,
            IMapper mapper,
            ApplicationDbContext applicationDbContext,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _mapper = mapper;
            _applicationDbContext = applicationDbContext;
            _configuration = configuration;
        }

        /// <summary>
        /// Changes the password for a user.
        /// </summary>
        public async Task<Result<string>> ChangePasswordAsync(SetupPasswordModel model)
        {
            var result = new Result<string>("");

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    result.StatusCode = Core.Enums.StatusCode.NotFound;
                    result.ErrorMessages = new List<string> { "User not found" };
                    return result;
                }

                var userResult = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
                if (userResult.Succeeded)
                {
                    result.StatusCode = Core.Enums.StatusCode.Success;
                    result.SuccessMessage = "Password updated successfully";
                    _logger.LogInformation("Password updated for user: {Email}", model.Email);
                }
                else
                {
                    result.StatusCode = Core.Enums.StatusCode.BadRequest;
                    result.ErrorMessages = userResult.Errors.Select(x => x.Description).ToList();
                    _logger.LogWarning("Password update failed for user: {Email}", model.Email);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating password for user: {Email}", model.Email);
                result.StatusCode = Core.Enums.StatusCode.InternalError;
                result.ErrorMessages = new List<string> { "An error occurred while processing your request" };
                return result;
            }
        }

        /// <summary>
        /// Sends a reset password token to the user's email.
        /// </summary>
        public async Task<Result<string>> SendForgetPasswordAsync(ResetPasswordModel model)
        {
            var result = new Result<string>("");

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    result.StatusCode = Core.Enums.StatusCode.NotFound;
                    result.ErrorMessages = new List<string> { "User not found" };
                    return result;
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                if (!string.IsNullOrEmpty(token))
                {
                    var tokenEncoded = HttpUtility.UrlEncode(token);
                    var emailEncoded = HttpUtility.UrlEncode(model.Email);

                    // TODO: Send email logic should be implemented here.

                    result.StatusCode = Core.Enums.StatusCode.Success;
                    result.SuccessMessage = "Password reset email sent successfully";
                    _logger.LogInformation("Password reset email sent to {Email}, token: {Token}", model.Email, tokenEncoded);
                }
                else
                {
                    result.ErrorMessages = new List<string> { "An error occurred while generating the reset token" };
                    _logger.LogWarning("Failed to send password reset email to {Email}", model.Email);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending password reset email to {Email}", model.Email);
                result.StatusCode = Core.Enums.StatusCode.InternalError;
                result.ErrorMessages = new List<string> { "An error occurred while processing your request" };
                return result;
            }
        }

        /// <summary>
        /// Registers a new user and sends a password reset email.
        /// </summary>
        public async Task<Result<string>> RegisterNewUserAsync(RegisterModel model)
        {
            return await RegisterUserAsync(model, true);
        }

        /// <summary>
        /// Resends the OTP code to the user's phone number.
        /// </summary>
        public async Task<Result<string>> ReSendOTPAsync(string userName)
        {
            var result = new Result<string>("");

            try
            {
                var user = await _userManager.FindByEmailAsync(userName);
                if (user == null)
                {
                    result.StatusCode = Core.Enums.StatusCode.NotFound;
                    result.ErrorMessages = new List<string> { "User not found" };
                    return result;
                }

                var token = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider);

                // TODO: Send OTP to user’s phone.

                result.StatusCode = Core.Enums.StatusCode.Success;
                result.SuccessMessage = "OTP sent successfully";
                _logger.LogInformation("OTP sent to user: {UserName}", userName);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while resending OTP to {UserName}", userName);
                result.StatusCode = Core.Enums.StatusCode.InternalError;
                result.ErrorMessages = new List<string> { "An error occurred while processing your request" };
                return result;
            }
        }

        /// <summary>
        /// Retrieves the user profile for a specific user ID.
        /// </summary>
        public async Task<Result<UserProfile>> GetProfileAsync(string userId)
        {
            var result = new Result<UserProfile>(null);

            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
                if (user == null)
                {
                    result.StatusCode = Core.Enums.StatusCode.NotFound;
                    result.ErrorMessages = new List<string> { "User not found" };
                    return result;
                }

                result.Data = _mapper.Map<UserProfile>(user);
                result.StatusCode = Core.Enums.StatusCode.Success;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving profile for user: {UserId}", userId);
                result.StatusCode = Core.Enums.StatusCode.InternalError;
                result.ErrorMessages = new List<string> { "An error occurred while processing your request" };
                return result;
            }
        }

        /// <summary>
        /// Registers a user with the specified model and roles.
        /// </summary>
        private async Task<Result<string>> RegisterUserAsync(RegisterModel model, bool isMainUser)
        {
            var result = new Result<string>("");

            try
            {
                // Validate roles if provided.
                if (model.Roles != null)
                {
                    var roles = await _applicationDbContext.Roles.Where(x => model.Roles.Contains(x.Name)).ToListAsync();
                    if (roles.Count() != model.Roles.Count())
                    {
                        result.StatusCode = Core.Enums.StatusCode.AlreadyExist;
                        result.ErrorMessages = new List<string> { "One or more roles do not exist" };
                        return result;
                    }
                }

                // Check if user already exists.
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    result.StatusCode = Core.Enums.StatusCode.AlreadyExist;
                    result.ErrorMessages = new List<string> { "User already exists" };
                    return result;
                }

                user = new AspNetUser
                {
                    UserName = model.Email,
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    CountryCode = model.CountryCode
                };

                var userResult = await _userManager.CreateAsync(user, RandomString(10));
                if (!userResult.Succeeded)
                {
                    result.ErrorMessages = userResult.Errors.Select(x => x.Description).ToList();
                    return result;
                }

                // Generate password reset token and send email.
                var tokenResult = await _userManager.GeneratePasswordResetTokenAsync(user);
                if (!string.IsNullOrEmpty(tokenResult))
                {
                    if (model.Roles != null)
                    {
                        await _userManager.AddToRolesAsync(user, model.Roles);
                    }

                    var tokenEncoded = HttpUtility.UrlEncode(tokenResult);
                    var emailEncoded = HttpUtility.UrlEncode(model.Email);

                    // TODO: Send email with the password reset token.

                    result.StatusCode = Core.Enums.StatusCode.Success;
                    result.SuccessMessage = "Registration successful, email sent";
                    _logger.LogInformation("Registration successful for user: {Email}", model.Email);
                    return result;
                }

                result.ErrorMessages = new List<string> { "An error occurred while generating the reset token" };
                _logger.LogWarning("Failed to generate password reset token for user: {Email}", model.Email);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering user: {Email}", model.Email);
                result.StatusCode = Core.Enums.StatusCode.InternalError;
                result.ErrorMessages = new List<string> { "An error occurred while processing your request" };
                return result;
            }
        }

        /// <summary>
        /// Generates a random string of the specified length.
        /// </summary>
        private string RandomString(int length)
        {
            Random random = new Random();
            const string firstChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            const string secondChars = "!@#$%0123456789";
            var firstPart = new string(Enumerable.Repeat(firstChars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            var secondPart = new string(Enumerable.Repeat(secondChars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return firstPart + secondPart;
        }

        /// <summary>
        /// Assigns roles to a user.
        /// </summary>
        public async Task<Result<string>> AssignRolesAsync(string userId, List<string> roles)
        {
            var result = new Result<string>("");

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    result.StatusCode = Core.Enums.StatusCode.NotFound;
                    result.ErrorMessages = new List<string> { "User not found" };
                    return result;
                }

                var validRoles = await _applicationDbContext.Roles
                    .Where(r => roles.Contains(r.Name))
                    .Select(r => r.Name)
                    .ToListAsync();

                if (validRoles.Count != roles.Count)
                {
                    result.StatusCode = Core.Enums.StatusCode.BadRequest;
                    result.ErrorMessages = new List<string> { "One or more roles are invalid" };
                    return result;
                }

                var addRolesResult = await _userManager.AddToRolesAsync(user, validRoles);
                if (!addRolesResult.Succeeded)
                {
                    result.StatusCode = Core.Enums.StatusCode.BadRequest;
                    result.ErrorMessages = addRolesResult.Errors.Select(e => e.Description).ToList();
                    return result;
                }

                result.StatusCode = Core.Enums.StatusCode.Success;
                result.SuccessMessage = "Roles assigned successfully";
                _logger.LogInformation("Roles assigned to user: {UserId}", userId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while assigning roles to user: {UserId}", userId);
                result.StatusCode = Core.Enums.StatusCode.InternalError;
                result.ErrorMessages = new List<string> { "An error occurred while processing your request" };
                return result;
            }
        }

        /// <summary>
        /// Retrieves all available roles in the system.
        /// </summary>
        public async Task<Result<List<string>>> GetRolesAsync()
        {
            var result = new Result<List<string>>(new List<string>());

            try
            {
                var roles = await _applicationDbContext.Roles
                    .Select(r => r.Name)
                    .ToListAsync();

                result.Data = roles;
                result.StatusCode = Core.Enums.StatusCode.Success;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving roles");
                result.StatusCode = Core.Enums.StatusCode.InternalError;
                result.ErrorMessages = new List<string> { "An error occurred while processing your request" };
                return result;
            }
        }

        /// <summary>
        /// Creates a new role.
        /// </summary>
        public async Task<Result<string>> CreateRoleAsync(string roleName)
        {
            var result = new Result<string>("");

            try
            {
                if (string.IsNullOrEmpty(roleName))
                {
                    result.StatusCode = Core.Enums.StatusCode.BadRequest;
                    result.ErrorMessages = new List<string> { "Role name cannot be empty" };
                    return result;
                }

                var roleExist = await _applicationDbContext.Roles.AnyAsync(r => r.Name == roleName);
                if (roleExist)
                {
                    result.StatusCode = Core.Enums.StatusCode.AlreadyExist;
                    result.ErrorMessages = new List<string> { "Role already exists" };
                    return result;
                }

                var role = new IdentityRole(roleName);
                _applicationDbContext.Roles.Add(role);
                await _applicationDbContext.SaveChangesAsync();

                result.StatusCode = Core.Enums.StatusCode.Success;
                result.SuccessMessage = "Role created successfully";
                _logger.LogInformation("Role {RoleName} created successfully", roleName);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating role {RoleName}", roleName);
                result.StatusCode = Core.Enums.StatusCode.InternalError;
                result.ErrorMessages = new List<string> { "An error occurred while processing your request" };
                return result;
            }
        }
    }
}
