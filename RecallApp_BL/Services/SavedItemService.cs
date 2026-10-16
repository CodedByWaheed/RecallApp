using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public SavedItemService(ISavedItemRepository savedItemRepository, IEncryptedContentRepository encryptedContentRepository
            , IUnitOfWork unitOfWork, IEncryptionService encryptionService,ILogger<SavedItemService> logger)
        {
            _savedItemRepository = savedItemRepository;
            _encryptedContentRepository = encryptedContentRepository;
            _unitOfWork = unitOfWork;
            _encryptionService = encryptionService;
            _logger = logger;
        }

        public async Task<Guid> CreateAsync(Guid userId, byte type, byte priority, byte status, string plaintext)
        {
            if (userId == Guid.Empty) { 
                _logger.LogWarning("CreateAsync called with empty UserId.");
                throw new ArgumentException("User ID is required.");
            }

            if (type < 1 || type > 3)
            {
                _logger.LogWarning("CreateAsync called with invalid type {Type} for UserId {UserId}.", type, userId);
                throw new ArgumentException("Invalid item type.");
            }

            if (priority < 1 || priority > 3)
            {
                _logger.LogWarning("CreateAsync called with invalid priority {Priority} for UserId {UserId}.", priority, userId);
                throw new ArgumentException("Invalid priority.");
            }


            if (status < 1 || status > 4)
            {
                _logger.LogWarning("CreateAsync called with invalid status {Status} for UserId {UserId}.", status, userId);
                throw new ArgumentException("Invalid status.");
            }
            if (string.IsNullOrWhiteSpace(plaintext)) { 
                _logger.LogWarning("CreateAsync called with empty plaintext for UserId {UserId}.", userId);
                throw new ArgumentException("Content cannot be empty.");
            }

            Guid savedItemId = Guid.Empty;

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
                EncryptionResult encryptionResult = _encryptionService.Encrypt(plaintext);
                EncryptedContentModel encryptedContent = new EncryptedContentModel
                {
                    SavedItemId = savedItemId,
                    FieldType = 1,
                    Ciphertext = encryptionResult.Ciphertext,
                    Nonce = encryptionResult.Nonce,
                    Tag = encryptionResult.Tag
                };
                await _encryptedContentRepository.CreateAsync(encryptedContent);
            });

            return savedItemId;
            
        }
        public async Task<string?> GetContentAsync(Guid userId ,Guid savedItemId)
        {
            List<EncryptedContentModel> encryptedContents = 
                await _encryptedContentRepository.GetBySavedItemIdAsync(userId, savedItemId);

            EncryptedContentModel? content = encryptedContents.FirstOrDefault(ec => ec.FieldType == 1);
            if (content is null)
            {
                _logger.LogWarning("Saved item content was not found for SavedItemId {SavedItemId}",savedItemId);
                return null;
            }
                

            return _encryptionService.Decrypt(new EncryptionResult
            {
                Ciphertext = content.Ciphertext,
                Nonce = content.Nonce,
                Tag = content.Tag
            });
        }
        public async Task<List<SavedItemModel>> GetByUserIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("GetByUserIdAsync called with empty userId.");
                throw new ArgumentException("User ID is required.");
            }
                

            return await _savedItemRepository.GetByUserIdAsync(userId);
        }
        public async Task UpdateAsync(Guid userId,Guid savedItemId,byte type,byte priority,byte status,string plaintext)
        {
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("UpdateAsync called with empty userId.");
                throw new ArgumentException("User ID is required.");
            }

            if (savedItemId == Guid.Empty)
            {
                _logger.LogWarning("UpdateAsync called with empty savedItemId.");
                throw new ArgumentException("Saved item ID is required.");
            }

            if (type < 1 || type > 3)
            {
                _logger.LogWarning("UpdateAsync called with invalid type {Type} for SavedItemId {SavedItemId}.", type, savedItemId);
                throw new ArgumentException("Invalid item type.");

            }

            if (priority < 1 || priority > 3)
            {
                _logger.LogWarning("UpdateAsync called with invalid priority {Priority} for SavedItemId {SavedItemId}.", priority, savedItemId);
                throw new ArgumentException("Invalid priority.");
            }
            if (status < 1 || status > 4)
            {
                _logger.LogWarning("UpdateAsync called with invalid status {Status} for SavedItemId {SavedItemId}.", status, savedItemId);
                throw new ArgumentException("Invalid status.");
            }

            if (string.IsNullOrWhiteSpace(plaintext))
                throw new ArgumentException("Content cannot be empty.");

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

                EncryptedContentModel encryptedContent = new EncryptedContentModel
                {
                    SavedItemId = savedItemId,
                    FieldType = 1,
                    Ciphertext = encryptionResult.Ciphertext,
                    Nonce = encryptionResult.Nonce,
                    Tag = encryptionResult.Tag
                };

                await _encryptedContentRepository.UpdateAsync(encryptedContent);
            });
        }
        public async Task<bool> ArchiveAsync(Guid userId,Guid savedItemId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID is required.");

            if (savedItemId == Guid.Empty)
                throw new ArgumentException("Saved item ID is required.");

            return await _savedItemRepository.ArchiveAsync(savedItemId,userId);
        }
        public async Task<bool> RestoreAsync(Guid userId,Guid savedItemId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID is required.");

            if (savedItemId == Guid.Empty)
                throw new ArgumentException("Saved item ID is required.");

            return await _savedItemRepository.RestoreAsync(savedItemId,userId);
        }
        public async Task<List<SavedItemModel>> GetArchivedByUserIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID is required.");

            return await _savedItemRepository.GetArchivedByUserIdAsync(userId);
        }
        public async Task<bool> DeleteAsync(Guid userId,Guid savedItemId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID is required.");

            if (savedItemId == Guid.Empty)
                throw new ArgumentException("Saved item ID is required.");

            return await _savedItemRepository.DeleteAsync(savedItemId,userId);
        }
        public async Task<List<SavedItemModel>> GetByTagIdAsync(Guid userId,Guid tagId)
        {
            return await _savedItemRepository.GetByTagIdAsync(userId, tagId);
        }
    }   
}
