
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RecallApp_BL.DTOs.SavedItemResponse;
using RecallApp_BL.DTOs.TagResponse;
using RecallApp_BL.Services.Interfaces;
using RecallApp_BL.Services.Model;
using RecallApp_DL.Interfaces;
using RecallApp_DL.Models;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;


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
        private readonly ITagRepository _tagRepositroy;
        private readonly HttpContext _context;

        public SavedItemService(ISavedItemRepository savedItemRepository,IEncryptedContentRepository encryptedContentRepository,
            IUnitOfWork unitOfWork,IEncryptionService encryptionService,ILogger<SavedItemService> logger,
            IAuditLogRepository auditLog,CurrentRequestInfo currentRequestInfo,ITagRepository tagRepository,
            IHttpContextAccessor contextAccessor)
        {
            _savedItemRepository = savedItemRepository;
            _encryptedContentRepository = encryptedContentRepository;
            _unitOfWork = unitOfWork;
            _encryptionService = encryptionService;
            _logger = logger;
            _auditLog = auditLog;
            _currentRequestInfo = currentRequestInfo;
            _tagRepositroy = tagRepository;
            _context = contextAccessor.HttpContext;
        }

        public async Task<Guid> CreateAsync(Guid userId,byte type,byte priority,byte status,string titlePlainText,string contentPlaintext , string notePlainText)
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
                _logger.LogWarning("CreateAsync called with invalid priority {Priority} for UserId {UserId}.",
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

            if (string.IsNullOrWhiteSpace(contentPlaintext))
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

                    savedItemId = await _savedItemRepository.CreateAsync(savedItem);

                    EncryptionResult contentEncryptionResult = _encryptionService.Encrypt(contentPlaintext);
                    EncryptionResult titleEncryptionResult = _encryptionService.Encrypt(titlePlainText);
                    EncryptionResult NoteEncryptionResult = _encryptionService.Encrypt(notePlainText);

                    EncryptedContentModel encryptedContent =
                        new EncryptedContentModel
                        {
                            SavedItemId = savedItemId,
                            FieldType = 1,
                            Ciphertext = contentEncryptionResult.Ciphertext,
                            Nonce = contentEncryptionResult.Nonce,
                            Tag = contentEncryptionResult.Tag
                        };
                    EncryptedContentModel encryptedTitle =
                        new EncryptedContentModel
                        {
                            SavedItemId = savedItemId,
                            FieldType = 2,
                            Ciphertext = titleEncryptionResult.Ciphertext,
                            Nonce = titleEncryptionResult.Nonce,
                            Tag = titleEncryptionResult.Tag
                        };
                    EncryptedContentModel encryptedNote =
                        new EncryptedContentModel
                        {
                            SavedItemId = savedItemId,
                            FieldType = 3,
                            Ciphertext = NoteEncryptionResult.Ciphertext,
                            Nonce = NoteEncryptionResult.Nonce,
                            Tag = NoteEncryptionResult.Tag
                        };

                    await _encryptedContentRepository.CreateAsync(encryptedContent);
                    await _encryptedContentRepository.CreateAsync(encryptedTitle);
                    await _encryptedContentRepository.CreateAsync(encryptedNote);
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

        //public async Task<string?> GetContentAsync(Guid userId,Guid savedItemId)
        //{
        //    if (userId == Guid.Empty)
        //        throw new ArgumentException("User ID is required.");

        //    if (savedItemId == Guid.Empty)
        //        throw new ArgumentException("Saved item ID is required.");

        //    try
        //    {
        //        List<EncryptedContentModel> encryptedContents =await _encryptedContentRepository
        //                .GetBySavedItemIdAsync(userId, savedItemId);

        //        EncryptedContentModel? content = encryptedContents.FirstOrDefault(ec => ec.FieldType == 1);

        //        if (content is null)
        //        {
        //            _logger.LogWarning(
        //                "Saved item content was not found for SavedItemId {SavedItemId}.",
        //                savedItemId);

        //            await WriteAuditSafelyAsync(
        //                userId,
        //                "GetSavedItemContent",
        //                "SavedItems",
        //                savedItemId.ToString(),
        //                "Failure",
        //                "Saved item content was not found or was inaccessible.");

        //            return null;
        //        }

        //        string decryptedContent = _encryptionService.Decrypt(
        //            new EncryptionResult
        //            {
        //                Ciphertext = content.Ciphertext,
        //                Nonce = content.Nonce,
        //                Tag = content.Tag
        //            });

        //        await WriteAuditSafelyAsync(
        //            userId,
        //            "GetSavedItemContent",
        //            "SavedItems",
        //            savedItemId.ToString(),
        //            "Success",
        //            "Saved item content retrieved successfully.");

        //        return decryptedContent;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(
        //            ex,
        //            "Failed to retrieve content for saved item {SavedItemId}, user {UserId}.",
        //            savedItemId, userId);

        //        await WriteAuditSafelyAsync(
        //            userId,
        //            "GetSavedItemContent",
        //            "SavedItems",
        //            savedItemId.ToString(),
        //            "Failure",
        //            "Failed to retrieve or decrypt saved item content.");

        //        throw;
        //    }
        //}
        private string? GetDecrypted(List<EncryptedContentModel> encryptedContents , int fieldType)
        {
            EncryptedContentModel? encrypted = encryptedContents.FirstOrDefault(ec => ec.FieldType == fieldType);

            if (encrypted is null)
                return "";

            string decrypted = _encryptionService.Decrypt(
                new EncryptionResult
                {
                    Ciphertext = encrypted.Ciphertext,
                    Nonce = encrypted.Nonce,
                    Tag = encrypted.Tag
                });
            return decrypted;
        }
        
        public async Task<SavedItem?> GetSavedItemAsync(Guid userId , Guid savedItemId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID is required.");

            if (savedItemId == Guid.Empty)
                throw new ArgumentException("Saved item ID is required.");

            try
            {
                List<EncryptedContentModel> encryptedContents = await _encryptedContentRepository
                        .GetBySavedItemIdAsync(userId, savedItemId);

                string? content = GetDecrypted(encryptedContents , 1);
                if (content is null){
                
                    _logger.LogWarning(
                        "Saved item content was not found for SavedItemId {SavedItemId}.",
                        savedItemId);

                    await WriteAuditSafelyAsync(
                        userId,
                        "GetSavedItemContent",
                        "SavedItems",
                        savedItemId.ToString(),
                        "Failure",
                        "Saved item content was not found or was inaccessible."
                    );

                    return null;
                
                }
                string? title = GetDecrypted(encryptedContents , 2);
                string? note = GetDecrypted(encryptedContents , 3);

                await WriteAuditSafelyAsync(
                    userId,
                    "GetSavedItemContent",
                    "SavedItems",
                    savedItemId.ToString(),
                    "Success",
                    "Saved item retrieved successfully.");
                SavedItemModel savedItemModel = await _savedItemRepository.GetByIdAsync(savedItemId);
                if (savedItemModel is null)
                {

                    _logger.LogWarning(
                        "Saved item was not found for SavedItemId {SavedItemId}.",
                        savedItemId);

                    await WriteAuditSafelyAsync(
                        userId,
                        "GetSavedItem",
                        "SavedItems",
                        savedItemId.ToString(),
                        "Failure",
                        "Saved item was not found or was inaccessible."
                    );

                    return null;

                }

                List<TagModel> ltagModel = await _tagRepositroy.GetBySavedItemIdAsync(userId,savedItemId);
                List<string> ltags = ltagModel.Select(t => t.Name).ToList();
               
                return new SavedItem
                {
                    // savedItem = savedItemModel,
                    Id = savedItemModel.Id,
                    Content = content,
                    Title = title ?? "",
                    Note = note ?? "",
                    Type= savedItemModel.Type,
                    Priority = savedItemModel.Priority,
                    Status = savedItemModel.Status,
                    Tags = ltags,
                    CreatedAt = savedItemModel.CreatedAt,
                    UpdatedAt = savedItemModel.UpdatedAt
                    


                };

                //return decryptedContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to retrieve saved item {SavedItemId}, user {UserId}.",
                    savedItemId, userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "GetSavedItem",
                    "SavedItems",
                    savedItemId.ToString(),
                    "Failure",
                    "Failed to retrieve or decrypt saved item content.");

                throw;
            }

            throw new NotImplementedException("Get Saved Item not implemnted yet");
        }

        private List<SavedItem> DecryptSavedItems(List<SavedItemModel> savedItemModel)
        {
            List<SavedItem> decryptedSavedItems = new List<SavedItem>();
            foreach (SavedItemModel item in savedItemModel)
            {
                decryptedSavedItems.Add(new SavedItem
                {
                    Id = item.Id,
                    Priority = item.Priority,
                    Status = item.Status,
                    Type = item.Type,
                    Title = GetDecrypted(item.encryptedContents , 2) ?? "",
                    Content = GetDecrypted(item.encryptedContents , 1) ?? "",
                    Note = GetDecrypted(item.encryptedContents , 3) ?? "",
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt
                });
            }
            return decryptedSavedItems;
        }

        public async Task<List<SavedItem>> GetByUserIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("GetByUserIdAsync called with empty userId.");

                throw new ArgumentException("User ID is required.");
            }

            try
            {
                List<SavedItemModel> encryptedItems = await _savedItemRepository.GetByUserIdAsync(userId);
                //foreach (SavedItemModel item in encryptedItems) {
                //    List<EncryptedContentModel> encryptedContents = await _encryptedContentRepository
                //        .GetBySavedItemIdAsync(userId, item.Id);
                //    item.encryptedContents = encryptedContents;
                //}
                List<SavedItem> decryptedItems = DecryptSavedItems(encryptedItems);

                //Do i need Tags Here ??
                await WriteAuditSafelyAsync(
                    userId,
                    "GetSavedItems",
                    "SavedItems",
                    null,
                    "Success",
                    "User's saved items retrieved successfully.");

                return decryptedItems;
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
            byte status,string titlePlaintext ,string contentPlaintext , string notePlaintext)
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

            //if (string.IsNullOrWhiteSpace(contentPlaintext))
            //    throw new ArgumentException("Content cannot be empty.");

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

                    if (contentPlaintext is not null)
                    {
                        EncryptionResult contentEncryptionResult = _encryptionService.Encrypt(contentPlaintext);
                        EncryptedContentModel encryptedContent =
                            new EncryptedContentModel
                            {
                                SavedItemId = savedItemId,
                                FieldType = 1,
                                Ciphertext = contentEncryptionResult.Ciphertext,
                                Nonce = contentEncryptionResult.Nonce,
                                Tag = contentEncryptionResult.Tag
                            };
                        await _encryptedContentRepository.UpdateAsync(encryptedContent);
                    }
                    if (titlePlaintext is not null)
                    {
                        EncryptionResult titleEncryptionResult = _encryptionService.Encrypt(titlePlaintext);
                        EncryptedContentModel encryptedTitle =
                            new EncryptedContentModel
                            {
                                SavedItemId = savedItemId,
                                FieldType = 2,
                                Ciphertext = titleEncryptionResult.Ciphertext,
                                Nonce = titleEncryptionResult.Nonce,
                                Tag = titleEncryptionResult.Tag
                            };
                        await _encryptedContentRepository.UpdateAsync(encryptedTitle);

                    }
                    if (notePlaintext is not null)
                    {
                        EncryptionResult NoteEncryptionResult = _encryptionService.Encrypt(notePlaintext);
                        EncryptedContentModel encryptedNote =
                            new EncryptedContentModel
                            {
                                SavedItemId = savedItemId,
                                FieldType = 3,
                                Ciphertext = NoteEncryptionResult.Ciphertext,
                                Nonce = NoteEncryptionResult.Nonce,
                                Tag = NoteEncryptionResult.Tag
                            };
                        await _encryptedContentRepository.UpdateAsync(encryptedNote);

                    }

                });

                await WriteAuditSafelyAsync(
                    userId,
                    "UpdateSavedItem",
                    "SavedItems",
                    savedItemId.ToString(),
                    "Success",
                    "Saved item Updated successfully.");

                //return savedItemId;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to Update saved item for user {UserId}.",
                    userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "UpdateSavedItem",
                    "SavedItems",
                    savedItemId == Guid.Empty
                        ? null
                        : savedItemId.ToString(),
                    "Failure",
                    "Saved item Update failed.");

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
                    UserAgent = _currentRequestInfo.UserAgent!,
                    TraceId = _context!.TraceIdentifier

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
