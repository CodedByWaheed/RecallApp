
using RecallApp_BL.Services.Interfaces;
using RecallApp_DL.Interfaces;
using RecallApp_DL.Models;
using Microsoft.Extensions.Logging;
using RecallApp_DL.Repositories.Interfaces;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RecallApp_BL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordService _passwordService;
        private readonly ILogger<UserService> _logger;
        private readonly IAuditLogRepository _auditLog;
        private readonly CurrentRequestInfo _currentRequestInfo;
        private readonly HttpContext _Context;

        public UserService(IUserRepository userRepository,PasswordService passwordService
            ,ILogger<UserService> logger,IAuditLogRepository auditLog,CurrentRequestInfo currentRequest
            ,IHttpContextAccessor contextAccessor)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _logger = logger;
            _auditLog = auditLog;
            _currentRequestInfo = currentRequest;
            _Context = contextAccessor.HttpContext;
        }

        public async Task<Guid> RegisterAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.");

            email = email.Trim().ToLowerInvariant();

            if (email.Length > 320 ||!new EmailAddressAttribute().IsValid(email))
                throw new ArgumentException("Invalid email address.");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required.");

            if (password.Length < 8 || password.Length > 128)
                throw new ArgumentException("Password must be between 8 and 128 characters.");

            Guid? userId = null;

            try
            {
                string passwordHash = _passwordService.HashPassword(password);

                var user = new UserModel
                {
                    Email = email,
                    PasswordHash = passwordHash
                };

                userId = await _userRepository.CreateAsync(user);

                //await WriteAuditSafelyAsync(userId,"Register","Users",
                //    userId.Value.ToString(),"Success","User registered successfully.");

                return userId.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User registration failed.");

                await WriteAuditSafelyAsync(
                    userId,
                    "Register",
                    "Users",
                    userId?.ToString(),
                    "Failure",
                    "User registration failed."
                    );

                throw;
            }
        }

        public async Task<UserModel?> GetProfileAsync(Guid userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task<bool> UpdateEmailAsync(Guid userId,string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.");

            email = email.Trim().ToLowerInvariant();

            if (email.Length > 320 ||
                !new EmailAddressAttribute().IsValid(email))
            {
                throw new ArgumentException("Invalid email address.");
            }

            try
            {
                bool updated =await _userRepository.UpdateEmailAsync(userId, email);

                //await WriteAuditSafelyAsync(
                //    userId,"UpdateEmail","Users",userId.ToString(),
                //    updated ? "Success" : "Failure",
                //    updated? "User email updated.": "User email update did not complete.");

                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to update email for user {UserId}.",
                    userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "UpdateEmail",
                    "Users",
                    userId.ToString(),
                    "Failure",
                    "User email update failed.");

                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(
            Guid userId,
            string currentPassword,
            string newPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword))
                throw new ArgumentException(
                    "Current password is required.");

            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException(
                    "New password is required.");

            if (newPassword.Length < 8 || newPassword.Length > 128)
                throw new ArgumentException(
                    "New password must be between 8 and 128 characters.");

            try
            {
                var user = await _userRepository.GetByIdAsync(userId);

                if (user is null)
                {
                    await WriteAuditSafelyAsync(
                        userId,
                        "ChangePassword",
                        "Users",
                        userId.ToString(),
                        "Failure",
                        "Password change failed because the user was not found."
                        );

                    return false;
                }

                bool isCurrentPasswordValid =
                    _passwordService.VerifyPassword(
                        currentPassword,
                        user.PasswordHash);

                if (!isCurrentPasswordValid)
                {
                    await WriteAuditSafelyAsync(
                        userId,
                        "ChangePassword",
                        "Users",
                        userId.ToString(),
                        "Failure",
                        "Password change rejected because the current password was incorrect.");

                    throw new ArgumentException(
                        "Current password is incorrect.");
                }

                string newPasswordHash =
                    _passwordService.HashPassword(newPassword);

                bool updated =
                    await _userRepository.UpdatePasswordHashAsync(
                        userId,
                        newPasswordHash);

                //await WriteAuditSafelyAsync(
                //    userId,
                //    "ChangePassword",
                //    "Users",
                //    userId.ToString(),
                //    updated ? "Success" : "Failure",
                //    updated
                //        ? "User password changed."
                //        : "User password change did not complete.");

                return updated;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to change password for user {UserId}.",
                    userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "ChangePassword",
                    "Users",
                    userId.ToString(),
                    "Failure",
                    "User password change failed.");

                throw;
            }
        }

        public async Task<bool> DeleteAccountAsync(
            Guid userId,
            string currentPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword))
                throw new ArgumentException(
                    "Current password is required.");

            try
            {
                var user = await _userRepository.GetByIdAsync(userId);

                if (user is null)
                {
                    await WriteAuditSafelyAsync(
                        userId,
                        "DeleteAccount",
                        "Users",
                        userId.ToString(),
                        "Failure",
                        "Account deletion failed because the user was not found."
                        );

                    return false;
                }

                bool isPasswordValid =
                    _passwordService.VerifyPassword(
                        currentPassword,
                        user.PasswordHash);

                if (!isPasswordValid)
                {
                    await WriteAuditSafelyAsync(
                        userId,
                        "DeleteAccount",
                        "Users",
                        userId.ToString(),
                        "Failure",
                        "Account deletion rejected because the current password was incorrect.");

                    throw new ArgumentException("Current password is incorrect.");
                }

                bool deleted = await _userRepository.DeleteAsync(userId);

                //await WriteAuditSafelyAsync(
                //    userId,
                //    "DeleteAccount",
                //    "Users",
                //    userId.ToString(),
                //    deleted ? "Success" : "Failure",
                //    deleted
                //        ? "User account deleted."
                //        : "User account deletion did not complete.");

                return deleted;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to delete account for user {UserId}.",
                    userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "DeleteAccount",
                    "Users",
                    userId.ToString(),
                    "Failure",
                    "User account deletion failed.");

                throw;
            }
        }

        private async Task WriteAuditSafelyAsync(Guid? userId,string action,string entityType,
            string? entityId,string outcome,string details )
        {
            try
            {
                await _auditLog.CreateAsync(new AuditLogModel
                {
                    UserId = userId,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    Outcome = outcome,
                    Details = details,
                    IpAddress = _currentRequestInfo.IpAddress!,
                    UserAgent = _currentRequestInfo.UserAgent!,
                    TraceId = _Context?.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to write audit log for {Action} on {EntityType}.",
                    action,
                    entityType);
            }
        }
    }
}
