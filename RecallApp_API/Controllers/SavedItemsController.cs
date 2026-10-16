using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecallApp_API.DTOs;
using RecallApp_API.DTOs.SavedItems;
using RecallApp_BL.Services.Interfaces;
using RecallApp_BL.Services.Security;
using RecallApp_DL.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RecallApp_API.Controllers
{
    [Authorize(Policy = "ValidUser")]
    [Route("api/SavedItems")]
    [ApiController]
    public class SavedItemsController : ControllerBase
    {
        private readonly ISavedItemService _savedItemService;
        private readonly ICurrentUserService _currentUser;
        public SavedItemsController(ISavedItemService savedItemService, ICurrentUserService currentUser)
        {
            _savedItemService = savedItemService;
            _currentUser = currentUser;
        }
        private bool TryGetUserId(out Guid userId)
        {
            userId = _currentUser.UserId ?? Guid.Empty;
            return userId != Guid.Empty;
        }

        [HttpGet("{id:guid}/content")]
        public async Task<IActionResult> GetContent(Guid id)
        {
           
            if (!TryGetUserId(out Guid userId))
            {
                return Unauthorized(new { message = "Invalid user identity." });
            }
            // Ownership will be enforced in the service/repository layer
            // before returning decrypted content.
            string? content = await _savedItemService.GetContentAsync(userId,id);
            if (content is null)
                return NotFound();

            return Ok(new { id, content });
        }

        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            if (!TryGetUserId(out Guid userId))
            {
                return Unauthorized(new { message = "Invalid user identity." });
            }

            List<SavedItemModel> savedItems = await _savedItemService.GetByUserIdAsync(userId);

            return Ok(savedItems);
        }

        [HttpGet("archived")]
        public async Task<IActionResult> GetArchived()
        {
            if (!TryGetUserId(out Guid userId))
            {
                return Unauthorized(new { message = "Invalid user identity." });
            }

            List<SavedItemModel> savedItems = await _savedItemService.GetArchivedByUserIdAsync(userId);

            return Ok(savedItems);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody]CreateSavedItemRequest request)
        {
            if (!TryGetUserId(out Guid userId))
            {
                return Unauthorized(new { message = "Invalid user identity." });
            }
            Guid savedItemId = await _savedItemService.CreateAsync(userId, request.Type, request.Priority,
                request.Status, request.Content);
            return CreatedAtAction(nameof(GetContent), new { id = savedItemId }, new { id = savedItemId });
        }

        [HttpPost("{id:guid}/archive")]
        public async Task<IActionResult> Archive(Guid id)
        {
            if (!TryGetUserId(out Guid userId))
            {
                return Unauthorized(new { message = "Invalid user identity." });
            }

            bool archived = await _savedItemService.ArchiveAsync(userId, id);

            if (!archived)
                return NotFound();

            return NoContent();
        }

        [HttpPost("{id:guid}/restore")]
        public async Task<IActionResult> Restore(Guid id)
        {
            if (!TryGetUserId(out Guid userId))
            {
                return Unauthorized(new { message = "Invalid user identity." });
            }

            bool restored = await _savedItemService.RestoreAsync(userId, id);

            if (!restored)
                return NotFound();

            return NoContent();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id,[FromBody] UpdateSavedItemRequest request)
        {
            if (!TryGetUserId(out Guid userId))
            {
                return Unauthorized(new { message = "Invalid user identity." });
            }

            await _savedItemService.UpdateAsync(userId,id,request.Type,request.Priority,request.Status,request.Content);

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!TryGetUserId(out Guid userId))
            {
                return Unauthorized(new { message = "Invalid user identity." });
            }

            bool deleted = await _savedItemService.DeleteAsync(userId, id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
        [HttpGet("by-tag/{tagId:guid}")]
        public async Task<IActionResult> GetByTag(Guid tagId)
        {
            if (!TryGetUserId(out Guid userId))
            {
                return Unauthorized(new { message = "Invalid user identity." });
            }

            var savedItems = await _savedItemService.GetByTagIdAsync(userId, tagId);

            return Ok(savedItems);
        }
    }
}
