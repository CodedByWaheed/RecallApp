using Microsoft.Extensions.Logging;
using RecallApp_BL.Services.Interfaces;
using RecallApp_DL.Interfaces;
using RecallApp_DL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_BL.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;
        private readonly ILogger _logger;

        public TagService(ITagRepository tagRepository ,ILogger logger)
        {
            _tagRepository = tagRepository;
            _logger = logger;
        }

        public async Task<Guid> CreateAsync(Guid userId, string name)
        {
            name = ValidateName(name);

            return await _tagRepository.CreateAsync(userId, name);
        }

        public async Task<List<TagModel>> GetByUserIdAsync(Guid userId)
        {
            return await _tagRepository.GetByUserIdAsync(userId);
        }

        public async Task<bool> UpdateAsync(Guid userId,Guid tagId,string name)
        {
            name = ValidateName(name);

            return await _tagRepository.UpdateAsync(tagId, userId, name);
        }

        public async Task<bool> DeleteAsync(Guid userId, Guid tagId)
        {
            return await _tagRepository.DeleteAsync(tagId, userId);
        }

        public async Task AssignAsync(Guid userId,Guid savedItemId,Guid tagId)
        {
            await _tagRepository.AssignAsync(userId, savedItemId, tagId);
        }

        public async Task<bool> RemoveAsync(Guid userId,Guid savedItemId,Guid tagId)
        {
            return await _tagRepository.RemoveAsync(userId, savedItemId, tagId);
        }

        public async Task<List<TagModel>> GetBySavedItemIdAsync(Guid userId,Guid savedItemId)
        {
            return await _tagRepository.GetBySavedItemIdAsync(userId, savedItemId);
        }

        private string ValidateName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("Attempted to create or update a tag with an empty or whitespace name.");
                throw new ArgumentException("Tag name is required.");

            }

            name = name.Trim();

            if (name.Length > 100)
            {
                _logger.LogWarning("Attempted to create or update a tag with a name exceeding 100 characters: {TagName}", name);
                throw new ArgumentException("Tag name cannot exceed 100 characters.");
            }
            return name;
        }
    }
}
