
using RecallApp_BL.Services.Interfaces;
using RecallApp_DL.Interfaces;
using RecallApp_DL.Models;
using RecallApp_DL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace RecallApp_BL.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;
        private readonly ILogger<TagService> _logger;
        private readonly IAuditLogRepository _auditLog;
        private readonly CurrentRequestInfo _currentRequestInfo;

        public TagService(ITagRepository tagRepository,ILogger<TagService> logger,
            IAuditLogRepository auditLog,CurrentRequestInfo currentRequestInfo)
        {
            _tagRepository = tagRepository;
            _logger = logger;
            _auditLog = auditLog;
            _currentRequestInfo = currentRequestInfo;
        }

        public async Task<Guid> CreateAsync(Guid userId, string name)
        {
            name = ValidateName(name);

            Guid? tagId = null;

            try
            {
                tagId = await _tagRepository.CreateAsync(userId, name);

                await WriteAuditSafelyAsync(
                    userId,
                    "CreateTag",
                    "Tags",
                    tagId.Value.ToString(),
                    "Success",
                    "Tag created successfully.");

                return tagId.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create tag for user {UserId}.",
                    userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "CreateTag",
                    "Tags",
                    tagId?.ToString(),
                    "Failure",
                    "Tag creation failed.");

                throw;
            }
        }

        public async Task<List<TagModel>> GetByUserIdAsync(Guid userId)
        {
            return await _tagRepository.GetByUserIdAsync(userId);
        }

        public async Task<bool> UpdateAsync(Guid userId,Guid tagId,string name)
        {
            name = ValidateName(name);

            try
            {
                bool updated = await _tagRepository.UpdateAsync(
                    tagId,
                    userId,
                    name);

                await WriteAuditSafelyAsync(
                    userId,
                    "UpdateTag",
                    "Tags",
                    tagId.ToString(),
                    updated ? "Success" : "Failure",
                    updated
                        ? "Tag updated successfully."
                        : "Tag update did not complete.");

                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to update tag {TagId} for user {UserId}.",
                    tagId,
                    userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "UpdateTag",
                    "Tags",
                    tagId.ToString(),
                    "Failure",
                    "Tag update failed.");

                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid userId,Guid tagId)
        {
            try
            {
                bool deleted = await _tagRepository.DeleteAsync(tagId,userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "DeleteTag",
                    "Tags",
                    tagId.ToString(),
                    deleted ? "Success" : "Failure",
                    deleted
                        ? "Tag deleted successfully."
                        : "Tag deletion did not complete.");

                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to delete tag {TagId} for user {UserId}.",
                    tagId,
                    userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "DeleteTag",
                    "Tags",
                    tagId.ToString(),
                    "Failure",
                    "Tag deletion failed.");

                throw;
            }
        }

        public async Task AssignAsync(Guid userId,Guid savedItemId,Guid tagId)
        {
            try
            {
                await _tagRepository.AssignAsync(
                    userId,
                    savedItemId,
                    tagId);

                await WriteAuditSafelyAsync(
                    userId,
                    "AssignTag",
                    "SavedItemTags",
                    $"{savedItemId}:{tagId}",
                    "Success",
                    "Tag assigned to saved item successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to assign tag {TagId} to saved item {SavedItemId} for user {UserId}.",
                    tagId,
                    savedItemId,
                    userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "AssignTag",
                    "SavedItemTags",
                    $"{savedItemId}:{tagId}",
                    "Failure",
                    "Tag assignment failed.");

                throw;
            }
        }

        public async Task<bool> RemoveAsync(Guid userId,Guid savedItemId,Guid tagId)
        {
            try
            {
                bool removed = await _tagRepository.RemoveAsync(
                    userId,
                    savedItemId,
                    tagId);

                await WriteAuditSafelyAsync(
                    userId,
                    "RemoveTag",
                    "SavedItemTags",
                    $"{savedItemId}:{tagId}",
                    removed ? "Success" : "Failure",
                    removed
                        ? "Tag removed from saved item successfully."
                        : "Tag removal did not complete.");

                return removed;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to remove tag {TagId} from saved item {SavedItemId} for user {UserId}.",
                    tagId,
                    savedItemId,
                    userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "RemoveTag",
                    "SavedItemTags",
                    $"{savedItemId}:{tagId}",
                    "Failure",
                    "Tag removal failed.");

                throw;
            }
        }

        public async Task<List<TagModel>> GetBySavedItemIdAsync(Guid userId,Guid savedItemId)
        {
            return await _tagRepository.GetBySavedItemIdAsync(
                userId,
                savedItemId);
        }

        private string ValidateName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning(
                    "Attempted to create or update a tag with an empty or whitespace name.");

                throw new ArgumentException("Tag name is required.");
            }

            name = name.Trim();

            if (name.Length > 100)
            {
                _logger.LogWarning(
                    "Attempted to create or update a tag with a name exceeding 100 characters.");

                throw new ArgumentException(
                    "Tag name cannot exceed 100 characters.");
            }

            return name;
        }

        private async Task WriteAuditSafelyAsync(Guid? userId,string action,string entityType,
            string? entityId,string outcome,string details)
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
                    UserAgent = _currentRequestInfo.UserAgent!
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
