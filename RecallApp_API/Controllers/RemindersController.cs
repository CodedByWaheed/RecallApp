using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecallApp_API.DTOs.Reminder;
using RecallApp_BL.Services.Interfaces;
using RecallApp_BL.Services.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RecallApp_API.Controllers
{
    [Authorize(Policy = "ValidUser")]
    [ApiController]
    [Route("api/reminders")]
    public class RemindersController : ControllerBase
    {
        private readonly IReminderService _reminderService;
        private readonly ICurrentUserService _currentUser;

        public RemindersController(IReminderService reminderService, ICurrentUserService currentUser)
        {
            _reminderService = reminderService;
            _currentUser = currentUser;
        }

        private bool TryGetUserId(out Guid userId)
        {
            string? value = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(value, out userId);
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReminderRequest request)
        {
            if (!TryGetUserId(out Guid userId))
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });

            Guid reminderId = await _reminderService.CreateAsync(userId, request.SavedItemId,request.RemindAt.UtcDateTime);

            return StatusCode(StatusCodes.Status201Created, new
            {
                id = reminderId,
                savedItemId = request.SavedItemId,
                remindAt = request.RemindAt.ToUniversalTime(),
                isCompleted = false
            });
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            if (!TryGetUserId(out Guid userId))
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });

            var reminders = await _reminderService.GetPendingByUserIdAsync(userId);

            return Ok(reminders);
        }

        [HttpGet("by-saved-item/{savedItemId:guid}")]
        public async Task<IActionResult> GetBySavedItem(Guid savedItemId)
        {
            if (!TryGetUserId(out Guid userId))
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });

            var reminders = await _reminderService.GetBySavedItemIdAsync(userId, savedItemId);

            return Ok(reminders);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id,[FromBody] UpdateReminderRequest request)
        {
            if (!TryGetUserId(out Guid userId))
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });

            bool updated = await _reminderService.UpdateAsync(userId,id,request.RemindAt.UtcDateTime);

            if (!updated)
                return NotFound(new
                {
                    message = "Reminder not found."
                });

            return NoContent();
        }

        [HttpPatch("{id:guid}/completion")]
        public async Task<IActionResult> SetCompletion(Guid id,[FromBody] bool isCompleted)
        {
            if (!TryGetUserId(out Guid userId))
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });

            bool updated = await _reminderService.SetCompletionAsync(userId, id, isCompleted);

            if (!updated)
                return NotFound(new
                {
                    message = "Reminder not found."
                });

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!TryGetUserId(out Guid userId))
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });

            bool deleted = await _reminderService.DeleteAsync(userId, id);

            if (!deleted)
                return NotFound(new
                {
                    message = "Reminder not found."
                });

            return NoContent();
        }
    }
}