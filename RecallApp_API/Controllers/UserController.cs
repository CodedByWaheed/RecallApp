
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RecallApp_API.DTOs.Users;
using RecallApp_BL.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RecallApp_API.Controllers
{
    [Authorize(Policy = "ValidUser")]
    [ApiController]
    [Route("api/users")]
    public sealed class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [EnableRateLimiting("AuthPolicy")]
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            Guid userId = await _userService.RegisterAsync(request.Email,request.Password);

            return StatusCode(StatusCodes.Status201Created, new
            {
                id = userId,
                email = request.Email.Trim().ToLowerInvariant()
            });
        }

        private bool TryGetUserId(out Guid userId)
        {
            string? value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(value, out userId);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            if (!TryGetUserId(out Guid userId))
                return Unauthorized();

            var user = await _userService.GetProfileAsync(userId);

            if (user is null)
                return NotFound();

            return Ok(new UserProfileResponse
            {
                Id = user.Id,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            });
        }

        [HttpPut("me/email")]
        public async Task<IActionResult> UpdateMyEmail([FromBody] UpdateUserEmailRequest request)
        {
            if (!TryGetUserId(out Guid userId))
                return Unauthorized();

            bool updated = await _userService.UpdateEmailAsync(userId,request.Email);

            if (!updated)
                return NotFound(new { message = "User not found." });

            return NoContent();
        }


        [HttpPut("me/password")]
        public async Task<IActionResult> ChangeMyPassword([FromBody] ChangePasswordRequest request)
        {
            if (!TryGetUserId(out Guid userId))
                return Unauthorized();

            bool changed = await _userService.ChangePasswordAsync(userId,request.CurrentPassword,request.NewPassword);

            if (!changed)
                return NotFound(new { message = "User not found." });

            return NoContent();
        }


        [HttpDelete("me")]
        public async Task<IActionResult> DeleteMyAccount([FromBody] DeleteAccountRequest request)
        {
            if (!TryGetUserId(out Guid userId))
                return Unauthorized();

            bool deleted = await _userService.DeleteAccountAsync(userId,request.CurrentPassword);

            if (!deleted)
                return NotFound(new { message = "User not found." });

            return NoContent();
        }


    }
}
