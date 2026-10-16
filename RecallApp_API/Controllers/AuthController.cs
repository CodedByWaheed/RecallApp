using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecallApp_API.DTOs.Auth;
using RecallApp_BL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace RecallApp_API.Controllers
{
    
    [Route("api/Auth")]
    [ApiController]
    public sealed class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [EnableRateLimiting("AuthPolicy")]
        [HttpPost("Login")]
        public async Task<ActionResult<LoginResponse?>> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request.Email, request.Password);

            if (result is null)
                return Unauthorized(new {
                    message = "Invalid Cedintial."
                });

            return Ok(result);
        }

        [EnableRateLimiting("RefreshPolicy")]
        [HttpPost("refresh")] 
        public async Task<ActionResult<LoginResponse>> Refresh([FromBody] RefreshTokenRequest request) 
        {
            var result = await _authService.RefreshAsync(request.RefreshToken); 
            if (result is null) {
                return Unauthorized(new {
                    message = "Invalid or expired refresh token." 
                }); }
            return Ok(new LoginResponse {
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken 
            }); }
        
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request) { 
            await _authService.LogoutAsync(request.RefreshToken); 
            return NoContent(); 
        }

    }
}
