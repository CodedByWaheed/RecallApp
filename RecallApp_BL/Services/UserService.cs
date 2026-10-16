
using RecallApp_BL.Services.Interfaces;
using RecallApp_DL.Interfaces;
using RecallApp_DL.Models;
using Microsoft.Extensions.Logging;
using RecallApp_DL.Repositories.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace RecallApp_BL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordService _passwordService;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, PasswordService passwordService , ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _logger = logger;
        }

        public async Task<Guid> RegisterAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email)) { 
                _logger.LogWarning("Attempted to register with an empty or whitespace email.");
                throw new ArgumentException("Email is required.");
            }   

            email = email.Trim().ToLowerInvariant();

            if (email.Length > 320 ||!new EmailAddressAttribute().IsValid(email))
            {
                _logger.LogWarning("Attempted to register with an invalid email: {Email}", email);
                throw new ArgumentException("Invalid email address.");
            }

            if (string.IsNullOrWhiteSpace(password)) { 
                _logger.LogWarning("Attempted to register with an empty or whitespace password.");
                throw new ArgumentException("Password is required.");
            }

            if (password.Length < 8 || password.Length > 128)
            {
                _logger.LogWarning("Attempted to register with a password that is not between 8 and 128 characters: {PasswordLength}", password.Length);
                throw new ArgumentException("Password must be between 8 and 128 characters.");
            }

            string passwordHash = _passwordService.HashPassword(password);

            var user = new UserModel
            {
                Email = email,
                PasswordHash = passwordHash
            };

            return await _userRepository.CreateAsync(user);
        }

        public async Task<UserModel?> GetProfileAsync(Guid userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task<bool> UpdateEmailAsync(Guid userId,string email)
        {
            if (string.IsNullOrWhiteSpace(email)) { 
                _logger.LogWarning("Attempted to update email with an empty or whitespace value.");
                throw new ArgumentException("Email is required.");
            }

            email = email.Trim().ToLowerInvariant();

            var emailValidator = new EmailAddressAttribute();

            if (!emailValidator.IsValid(email))
            {
                _logger.LogWarning("Attempted to update email with an invalid value: {Email}", email);
                throw new ArgumentException("Invalid email address.");
            }

            if (email.Length > 320)
            {
                _logger.LogWarning("Attempted to update email with a value that is too long: {EmailLength}", email.Length);
                throw new ArgumentException("Email is too long.");
            }   

            return await _userRepository.UpdateEmailAsync(userId, email);
        }

        public async Task<bool> ChangePasswordAsync(Guid userId,string currentPassword,string newPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                _logger.LogWarning("Attempted to change password with an empty or whitespace current password.");
                throw new ArgumentException("Current password is required.");
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                _logger.LogWarning("Attempted to change password with an empty or whitespace new password.");
                throw new ArgumentException("New password is required.");
            }

            if (newPassword.Length < 8) { 
            _logger.LogWarning("Attempted to change password with a new password that is too short: {NewPasswordLength}", newPassword.Length);
            throw new ArgumentException("New password is too short.");
            }   

            if (newPassword.Length > 128) { 
                _logger.LogWarning("Attempted to change password with a new password that is too long: {NewPasswordLength}", newPassword.Length);
                throw new ArgumentException("New password is too long.");
            }

            var user = await _userRepository.GetByIdAsync(userId);

            if (user is null)
                return false;

            bool isCurrentPasswordValid =
                _passwordService.VerifyPassword(
                    currentPassword,
                    user.PasswordHash);

            if (!isCurrentPasswordValid) { 
                _logger.LogWarning("Attempted to change password with an incorrect current password.");
                throw new ArgumentException("Current password is incorrect.");
            }

            string newPasswordHash =_passwordService.HashPassword(newPassword);

            return await _userRepository.UpdatePasswordHashAsync(userId,newPasswordHash);
        }

        public async Task<bool> DeleteAccountAsync(Guid userId,string currentPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                _logger.LogWarning("Attempted to delete account with an empty or whitespace current password.");
                throw new ArgumentException("Current password is required.");
            }
            var user = await _userRepository.GetByIdAsync(userId);

            if (user is null)
                return false;

            bool isPasswordValid = _passwordService.VerifyPassword(currentPassword,user.PasswordHash);

            if (!isPasswordValid) { 
                _logger.LogWarning("Attempted to delete account with an incorrect current password.");
                throw new ArgumentException("Current password is incorrect.");
            }   

            return await _userRepository.DeleteAsync(userId);
        }


    }
}
