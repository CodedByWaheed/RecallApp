using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecallApp_API.DTOs.Tags;
using RecallApp_BL.Services;
using RecallApp_BL.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RecallApp_API.Controllers
{
    [Authorize(Policy = "ValidUser")]
    [ApiController]
    [Route("api/saveditems/{savedItemId:guid}/tags")]
    public class SavedItemTagsController : ControllerBase
    {
        private readonly ITagService _tagService;
        private readonly ISavedItemService _savedItemService;
        public SavedItemTagsController(ITagService tagService , ISavedItemService savedItemService  )
        {
            _tagService = tagService;
            _savedItemService = savedItemService;
        }

        private bool TryGetUserId(out Guid userId)
        {
            string? value = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(value, out userId);
        }

        [HttpPost]
        public async Task<IActionResult> Assign(Guid savedItemId,[FromBody] AssignTagRequest request)
        {
            if (!TryGetUserId(out Guid userId))
                return Unauthorized(new { message = "Invalid user identity." });


            await _tagService.AssignAsync(userId, savedItemId, request.TagId);

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetTags(Guid savedItemId, [FromServices] IAuthorizationService authorizationService)
        {
            if (!TryGetUserId(out Guid userId))
                return Unauthorized(new { message = "Invalid user identity." });

            var savedItem = await _savedItemService.GetSavedItemAsync(userId, savedItemId);
            var authResult = await authorizationService.AuthorizeAsync(
                User,
                savedItem?.UserId,
                "UserOwner");

            if (!authResult.Succeeded)
                return Forbid(); // 403
            var tags = await _tagService.GetBySavedItemIdAsync(userId, savedItemId);

            return Ok(tags);
        }

        [HttpDelete("{tagId:guid}")]
        public async Task<IActionResult> Remove(Guid savedItemId,Guid tagId , [FromServices] IAuthorizationService authorizationService)
        {
            if (!TryGetUserId(out Guid userId))
                return Unauthorized(new { message = "Invalid user identity." });

            var savedItem = await _savedItemService.GetSavedItemAsync(userId, savedItemId);
            var authResult = await authorizationService.AuthorizeAsync(
                User,
                savedItem?.UserId,
                "UserOwner");

            if (!authResult.Succeeded)
                return Forbid(); // 403
            bool removed = await _tagService.RemoveAsync(userId, savedItemId, tagId);

            if (!removed)
                return NotFound(new
                {
                    message = "Tag assignment not found."
                });

            return NoContent();
        }
    }
}