using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecallApp_API.DTOs;
using RecallApp_API.DTOs.SavedItems;
using RecallApp_BL.DTOs.SavedItemResponse;
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

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetSavedItem(Guid id , [FromServices] IAuthorizationService authorizationService)
        {
           
            if (!TryGetUserId(out Guid userId))
            {
                return Unauthorized(new { message = "Invalid user identity." });
            }
            // Ownership will be enforced in the service/repository layer
            // before returning decrypted content.

            SavedItem? savedItem = await _savedItemService.GetSavedItemAsync(userId,id);
            if (savedItem is null)
                return NotFound();

            var authResult = await authorizationService.AuthorizeAsync(
                User,
                savedItem.UserId,
                "UserOwner");

            if (!authResult.Succeeded)
                return Forbid(); // 403
           

            return Ok(new { savedItem });
        }

        [HttpGet()]
        public async Task<IActionResult> GetAll([FromServices] IAuthorizationService authorizationService)
        {
            if (!TryGetUserId(out Guid userId))
            {
                return Unauthorized(new { message = "Invalid user identity." });
            }
            List<SavedItem>? savedItems = await _savedItemService.GetByUserIdAsync(userId);

            if (savedItems is null )
                return NoContent();
           
            var authResult = await authorizationService.AuthorizeAsync(
                User,
                savedItems.First().UserId,
                "UserOwner");

            if (!authResult.Succeeded)
                return Forbid(); // 403
            

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
                request.Status,request.Title, request.Content , request.Note);
            return CreatedAtAction(nameof(GetSavedItem), new { id = savedItemId }, new { id = savedItemId });
        }

        [HttpPost("{id:guid}/archive")]
        public async Task<IActionResult> Archive(Guid id, [FromServices] IAuthorizationService authorizationService)
        {
            if (!TryGetUserId(out Guid userId))
            {
                return Unauthorized(new { message = "Invalid user identity." });
            }
            var savedItem = await _savedItemService.GetSavedItemAsync(userId, id);
            var authResult = await authorizationService.AuthorizeAsync(
                User,
                savedItem?.UserId,
                "UserOwner");

            if (!authResult.Succeeded)
                return Forbid(); // 403
            bool archived = await _savedItemService.ArchiveAsync(userId, id);

            if (!archived)
                return NotFound();

            return NoContent();
        }

        [HttpPost("{id:guid}/restore")]
        public async Task<IActionResult> Restore(Guid id, [FromServices] IAuthorizationService authorizationService)
        {
            if (!TryGetUserId(out Guid userId))
            {
                return Unauthorized(new { message = "Invalid user identity." });
            }
            var savedItem = await _savedItemService.GetSavedItemAsync(userId, id);
            var authResult = await authorizationService.AuthorizeAsync(
                User,
                savedItem?.UserId,
                "UserOwner");

            if (!authResult.Succeeded)
                return Forbid(); // 403
            bool restored = await _savedItemService.RestoreAsync(userId, id);

            if (!restored)
                return NotFound();

            return NoContent();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id,[FromBody] UpdateSavedItemRequest request ,[FromServices] IAuthorizationService authorizationService)
        {
            if (!TryGetUserId(out Guid userId))
            {
                return Unauthorized(new { message = "Invalid user identity." });
            }
            var savedItem = await _savedItemService.GetSavedItemAsync(userId, id);
            var authResult = await authorizationService.AuthorizeAsync(
                User,
                savedItem?.UserId,
                "UserOwner");

            if (!authResult.Succeeded)
                return Forbid(); // 403
            await _savedItemService.UpdateAsync(userId,id,request.Type,request.Priority,request.Status,request.Title,request.Content,request.Note);

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, [FromServices] IAuthorizationService authorizationService)
        {
            if (!TryGetUserId(out Guid userId))
            {
                return Unauthorized(new { message = "Invalid user identity." });
            }
            SavedItem? savedItem = await _savedItemService.GetSavedItemAsync(userId, id);
            var authResult = await authorizationService.AuthorizeAsync(
                User,
                savedItem?.UserId,
                "UserOwner");

            if (!authResult.Succeeded)
                return Forbid(); // 403
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
