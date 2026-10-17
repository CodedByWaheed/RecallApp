
using Microsoft.Extensions.Logging;
using RecallApp_BL.Services.Interfaces;
using RecallApp_BL.Services.Model;
using RecallApp_DL.Interfaces;
using RecallApp_DL.Models;


namespace RecallApp_BL.Services
{
    public sealed class SavedItemService : ISavedItemService
    {
        private readonly ISavedItemRepository _savedItemRepository;
        private readonly IEncryptedContentRepository _encryptedContentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<SavedItemService> _logger;
        private readonly IAuditLogRepository _auditLog;
        private readonly CurrentRequestInfo _currentRequestInfo;

        public SavedItemService(ISavedItemRepository savedItemRepository,IEncryptedContentRepository encryptedContentRepository,
            IUnitOfWork unitOfWork,IEncryptionService encryptionService,ILogger<SavedItemService> logger,
            IAuditLogRepository auditLog,CurrentRequestInfo currentRequestInfo)
        {
            _savedItemRepository = savedItemRepository;
            _encryptedContentRepository = encryptedContentRepository;
            _unitOfWork = unitOfWork;
            _encryptionService = encryptionService;
            _logger = logger;
            _auditLog = auditLog;
            _currentRequestInfo = currentRequestInfo;
        }

        public async Task<Guid> CreateAsync(Guid userId,byte type,byte priority,byte status,string plaintext)
        {
            if (userId == Guid.Empty)
            {
                _logger.LogWarning(
                    "CreateAsync called with empty UserId.");

                throw new ArgumentException("User ID is required.");
            }

            if (type < 1 || type > 3)
            {
                _logger.LogWarning(
                    "CreateAsync called with invalid type {Type} for UserId {UserId}.",
                    type, userId);

                throw new ArgumentException("Invalid item type.");
            }

            if (priority < 1 || priority > 3)
            {
                _logger.LogWarning(
                    "CreateAsync called with invalid priority {Priority} for UserId {UserId}.",
                    priority, userId);

                throw new ArgumentException("Invalid priority.");
            }

            if (status < 1 || status > 4)
            {
                _logger.LogWarning(
                    "CreateAsync called with invalid status {Status} for UserId {UserId}.",
                    status, userId);

                throw new ArgumentException("Invalid status.");
            }

            if (string.IsNullOrWhiteSpace(plaintext))
            {
                _logger.LogWarning(
                    "CreateAsync called with empty plaintext for UserId {UserId}.",
                    userId);

                throw new ArgumentException("Content cannot be empty.");
            }

            Guid savedItemId = Guid.Empty;

            try
            {
                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    SavedItemModel savedItem = new SavedItemModel
                    {
                        UserId = userId,
                        Type = type,
                        Priority = priority,
                        Status = status
                    };

                    savedItemId =
                        await _savedItemRepository.CreateAsync(savedItem);

                    EncryptionResult encryptionResult =
                        _encryptionService.Encrypt(plaintext);

                    EncryptedContentModel encryptedContent =
                        new EncryptedContentModel
                        {
                            SavedItemId = savedItemId,
                            FieldType = 1,
                            Ciphertext = encryptionResult.Ciphertext,
                            Nonce = encryptionResult.Nonce,
                            Tag = encryptionResult.Tag
                        };

                    await _encryptedContentRepository.CreateAsync(
                        encryptedContent);
                });

                await WriteAuditSafelyAsync(
                    userId,
                    "CreateSavedItem",
                    "SavedItems",
                    savedItemId.ToString(),
                    "Success",
                    "Saved item created successfully.");

                return savedItemId;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create saved item for user {UserId}.",
                    userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "CreateSavedItem",
                    "SavedItems",
                    savedItemId == Guid.Empty
                        ? null
                        : savedItemId.ToString(),
                    "Failure",
                    "Saved item creation failed.");

                throw;
            }
        }

        public async Task<string?> GetContentAsync(Guid userId,Guid savedItemId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID is required.");

            if (savedItemId == Guid.Empty)
                throw new ArgumentException("Saved item ID is required.");

            try
            {
                List<EncryptedContentModel> encryptedContents =await _encryptedContentRepository
                        .GetBySavedItemIdAsync(userId, savedItemId);

                EncryptedContentModel? content = encryptedContents.FirstOrDefault(ec => ec.FieldType == 1);

                if (content is null)
                {
                    _logger.LogWarning(
                        "Saved item content was not found for SavedItemId {SavedItemId}.",
                        savedItemId);

                    await WriteAuditSafelyAsync(
                        userId,
                        "GetSavedItemContent",
                        "SavedItems",
                        savedItemId.ToString(),
                        "Failure",
                        "Saved item content was not found or was inaccessible.");

                    return null;
                }

                string decryptedContent = _encryptionService.Decrypt(
                    new EncryptionResult
                    {
                        Ciphertext = content.Ciphertext,
                        Nonce = content.Nonce,
                        Tag = content.Tag
                    });

                await WriteAuditSafelyAsync(
                    userId,
                    "GetSavedItemContent",
                    "SavedItems",
                    savedItemId.ToString(),
                    "Success",
                    "Saved item content retrieved successfully.");

                return decryptedContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to retrieve content for saved item {SavedItemId}, user {UserId}.",
                    savedItemId, userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "GetSavedItemContent",
                    "SavedItems",
                    savedItemId.ToString(),
                    "Failure",
                    "Failed to retrieve or decrypt saved item content.");

                throw;
            }
        }

        public async Task<List<SavedItemModel>> GetByUserIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                _logger.LogWarning(
                    "GetByUserIdAsync called with empty userId.");

                throw new ArgumentException("User ID is required.");
            }

            try
            {
                List<SavedItemModel> items =await _savedItemRepository.GetByUserIdAsync(userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "GetSavedItems",
                    "SavedItems",
                    null,
                    "Success",
                    "User's saved items retrieved successfully.");

                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to retrieve saved items for user {UserId}.",
                    userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "GetSavedItems",
                    "SavedItems",
                    null,
                    "Failure",
                    "Failed to retrieve user's saved items.");

                throw;
            }
        }

        public async Task UpdateAsync(Guid userId,Guid savedItemId,byte type,byte priority,
            byte status,string plaintext)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID is required.");

            if (savedItemId == Guid.Empty)
                throw new ArgumentException("Saved item ID is required.");

            if (type < 1 || type > 3)
                throw new ArgumentException("Invalid item type.");

            if (priority < 1 || priority > 3)
                throw new ArgumentException("Invalid priority.");

            if (status < 1 || status > 4)
                throw new ArgumentException("Invalid status.");

            if (string.IsNullOrWhiteSpace(plaintext))
                throw new ArgumentException("Content cannot be empty.");

            try
            {
                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    SavedItemModel savedItem = new SavedItemModel
                    {
                        Id = savedItemId,
                        UserId = userId,
                        Type = type,
                        Priority = priority,
                        Status = status
                    };

                    await _savedItemRepository.UpdateAsync(savedItem);

                    EncryptionResult encryptionResult =_encryptionService.Encrypt(plaintext);

                    EncryptedContentModel encryptedContent =
                        new EncryptedContentModel
                        {
                            SavedItemId = savedItemId,
                            FieldType = 1,
                            Ciphertext = encryptionResult.Ciphertext,
                            Nonce = encryptionResult.Nonce,
                            Tag = encryptionResult.Tag
                        };

                    await _encryptedContentRepository.UpdateAsync(encryptedContent);
                });

                await WriteAuditSafelyAsync(
                    userId,
                    "UpdateSavedItem",
                    "SavedItems",
                    savedItemId.ToString(),
                    "Success",
                    "Saved item updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to update saved item {SavedItemId} for user {UserId}.",
                    savedItemId, userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "UpdateSavedItem",
                    "SavedItems",
                    savedItemId.ToString(),
                    "Failure",
                    "Saved item update failed.");

                throw;
            }
        }

        public async Task<bool> ArchiveAsync(Guid userId,Guid savedItemId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID is required.");

            if (savedItemId == Guid.Empty)
                throw new ArgumentException("Saved item ID is required.");

            try
            {
                bool archived =await _savedItemRepository.ArchiveAsync(savedItemId, userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "ArchiveSavedItem",
                    "SavedItems",
                    savedItemId.ToString(),
                    archived ? "Success" : "Failure",
                    archived
                        ? "Saved item archived successfully."
                        : "Saved item could not be archived or was inaccessible.");

                return archived;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to archive saved item {SavedItemId} for user {UserId}.",
                    savedItemId, userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "ArchiveSavedItem",
                    "SavedItems",
                    savedItemId.ToString(),
                    "Failure",
                    "Saved item archiving failed.");

                throw;
            }
        }

        public async Task<bool> RestoreAsync(Guid userId,Guid savedItemId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID is required.");

            if (savedItemId == Guid.Empty)
                throw new ArgumentException("Saved item ID is required.");

            try
            {
                bool restored =await _savedItemRepository.RestoreAsync(savedItemId, userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "RestoreSavedItem",
                    "SavedItems",
                    savedItemId.ToString(),
                    restored ? "Success" : "Failure",
                    restored
                        ? "Saved item restored successfully."
                        : "Saved item could not be restored or was inaccessible.");

                return restored;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to restore saved item {SavedItemId} for user {UserId}.",
                    savedItemId, userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "RestoreSavedItem",
                    "SavedItems",
                    savedItemId.ToString(),
                    "Failure",
                    "Saved item restoration failed.");

                throw;
            }
        }

        public async Task<List<SavedItemModel>> GetArchivedByUserIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID is required.");

            try
            {
                List<SavedItemModel> items = await _savedItemRepository.GetArchivedByUserIdAsync(userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "GetArchivedSavedItems",
                    "SavedItems",
                    null,
                    "Success",
                    "Archived saved items retrieved successfully.");

                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to retrieve archived saved items for user {UserId}.",
                    userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "GetArchivedSavedItems",
                    "SavedItems",
                    null,
                    "Failure",
                    "Failed to retrieve archived saved items.");

                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid userId,Guid savedItemId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID is required.");

            if (savedItemId == Guid.Empty)
                throw new ArgumentException("Saved item ID is required.");

            try
            {
                bool deleted =
                    await _savedItemRepository.DeleteAsync(
                        savedItemId, userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "DeleteSavedItem",
                    "SavedItems",
                    savedItemId.ToString(),
                    deleted ? "Success" : "Failure",
                    deleted
                        ? "Saved item deleted successfully."
                        : "Saved item could not be deleted or was inaccessible.");

                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to delete saved item {SavedItemId} for user {UserId}.",
                    savedItemId, userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "DeleteSavedItem",
                    "SavedItems",
                    savedItemId.ToString(),
                    "Failure",
                    "Saved item deletion failed.");

                throw;
            }
        }

        public async Task<List<SavedItemModel>> GetByTagIdAsync(Guid userId,Guid tagId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID is required.");

            if (tagId == Guid.Empty)
                throw new ArgumentException("Tag ID is required.");

            try
            {
                List<SavedItemModel> items =
                    await _savedItemRepository.GetByTagIdAsync(
                        userId, tagId);

                await WriteAuditSafelyAsync(
                    userId,
                    "GetSavedItemsByTag",
                    "Tags",
                    tagId.ToString(),
                    "Success",
                    "Saved items associated with the tag retrieved successfully.");

                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to retrieve saved items for tag {TagId}, user {UserId}.",
                    tagId, userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "GetSavedItemsByTag",
                    "Tags",
                    tagId.ToString(),
                    "Failure",
                    "Failed to retrieve saved items associated with the tag.");

                throw;
            }
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
