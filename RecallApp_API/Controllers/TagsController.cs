using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecallApp_API.DTOs.Tags;
using RecallApp_BL.Services;
using RecallApp_BL.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RecallApp_API.Controllers
{
    [Authorize(Policy = "ValidUser")]
    [Route("api/Tags")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        private bool TryGetUserId(out Guid userId)
        {
            string? value =User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(value, out userId);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTagRequest request)
        {
            if (!TryGetUserId(out Guid userId))
                return Unauthorized(new { 
                    message = "Invalid user identity." 
                });

            Guid tagId = await _tagService.CreateAsync(userId, request.Name);

            return CreatedAtAction(nameof(GetAll),new { id = tagId },new { id = tagId, name = request.Name.Trim() });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromServices] IAuthorizationService authorizationService)
        {
            if (!TryGetUserId(out Guid userId))
                return Unauthorized(new { message = "Invalid user identity." });

            var tags = await _tagService.GetByUserIdAsync(userId);
            
            var authResult = await authorizationService.AuthorizeAsync(
                User,
                tags.First().UserId,
                "UserOwner");

            if (!authResult.Succeeded)
                return Forbid(); // 403

            return Ok(tags);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id,[FromBody] UpdateTagRequest request)
        {
            if (!TryGetUserId(out Guid userId))
                return Unauthorized(new { message = "Invalid user identity." });


            bool updated = await _tagService.UpdateAsync(userId, id, request.Name);

            if (!updated)
                return NotFound(new { message = "Tag not found." });

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!TryGetUserId(out Guid userId))
                return Unauthorized(new { message = "Invalid user identity." });

            bool deleted = await _tagService.DeleteAsync(userId, id);

            if (!deleted)
                return NotFound(new { message = "Tag not found." });

            return NoContent();
        }
    }
}
