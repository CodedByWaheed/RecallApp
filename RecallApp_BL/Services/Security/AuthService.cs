using Microsoft.IdentityModel.Tokens;
using RecallApp_BL.DTOs.LoginResponse;
using RecallApp_BL.Services.Interfaces;
using RecallApp_DL.Interfaces;
using RecallApp_DL.Models;
using RecallApp_DL.Repositories;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_BL.Services.Security
{
    public class AuthService: IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly PasswordService _passwordService;
        private readonly RefreshTokenService _refreshTokenService;
        private readonly string _jwtSecretKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;

        public AuthService(IUserRepository userRepository, PasswordService passwordService, string jwtSecretKey
            , string jwtIssuer, string jwtAudience,RefreshTokenService refreshTokenService, IRefreshTokenRepository refreshTokenRepository)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _refreshTokenService = refreshTokenService;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtSecretKey = jwtSecretKey;
            _jwtIssuer = jwtIssuer;
            _jwtAudience = jwtAudience;
        }
        public async Task<LoginResult?> LoginAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user is null)
                return null;
            bool isPasswordValid = _passwordService.VerifyPassword(password, user.PasswordHash);
            if (!isPasswordValid)
                return null;
            string accessToken = GenerateAccessToken(user);

            // Here you would typically save the refresh token to the database associated with the user
            string refreshToken = _refreshTokenService.GenerateToken();

            byte[] refreshTokenHash = _refreshTokenService.HashToken(refreshToken);

            DateTime refreshTokenExpiresAt = DateTime.UtcNow.AddDays(30);

            var refreshTokenModel = new RefreshTokenModel
            {
                UserId = user.Id,
                TokenHash = refreshTokenHash,
                ExpiresAtUtc = refreshTokenExpiresAt
            };

            await _refreshTokenRepository.CreateAsync(refreshTokenModel);

            return new LoginResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<LoginResult?> RefreshAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return null;


            byte[] oldTokenHash = _refreshTokenService.HashToken(refreshToken);

            string newRefreshToken = _refreshTokenService.GenerateToken();
            byte[] newTokenHash =_refreshTokenService.HashToken(newRefreshToken);

            DateTime newRefreshTokenExpiresAt = DateTime.UtcNow.AddDays(30);

            Guid? userId = await _refreshTokenRepository.RotateAsync(oldTokenHash,newTokenHash,newRefreshTokenExpiresAt);

            if (userId is null)
                return null;

            var user = await _userRepository.GetByIdAsync(userId.Value);

            if (user is null)
                return null;

            string accessToken = GenerateAccessToken(user);

            return new LoginResult
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken
            };

        }

        private string GenerateAccessToken(UserModel user)
        {
            byte[] keyBytes = Convert.FromBase64String(_jwtSecretKey);
            var singinKey = new SymmetricSecurityKey(keyBytes);
            var credentials = new SigningCredentials(singinKey, SecurityAlgorithms.HmacSha256);

            var calims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email,user.Email.ToString()),
                new Claim(ClaimTypes.NameIdentifier , user.Id.ToString())
            };
            var token = new JwtSecurityToken(issuer: _jwtIssuer, audience: _jwtAudience
                , claims: calims, expires: DateTime.UtcNow.AddMinutes(30), signingCredentials: credentials);
            string accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            return accessToken;
        }

        public async Task LogoutAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return;

            byte[] tokenHash = _refreshTokenService.HashToken(refreshToken);

            await _refreshTokenRepository.RevokeAsync(tokenHash);
        }

    }
}
