using RecallApp_BL.Services.Interfaces;
using RecallApp_DL.Models;
using RecallApp_DL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace RecallApp_BL.Services
{
    public class ReminderService : IReminderService
    {
        private readonly IReminderRepository _reminderRepository;
        private readonly ILogger<ReminderService> _logger;

        public ReminderService(IReminderRepository reminderRepository, ILogger<ReminderService> logger)
        {
            _reminderRepository = reminderRepository;
            _logger = logger;
        }

        public async Task<Guid> CreateAsync(Guid userId,Guid savedItemId,DateTime remindAt)
        {
            ValidateRemindAt(remindAt);
            return await _reminderRepository.CreateAsync(userId, savedItemId, remindAt);
        }

        public async Task<List<ReminderModel>> GetPendingByUserIdAsync(Guid userId)
        {
            return await _reminderRepository.GetPendingByUserIdAsync(userId);
        }

        public async Task<List<ReminderModel>> GetBySavedItemIdAsync(Guid userId,Guid savedItemId)
        {
            return await _reminderRepository.GetBySavedItemIdAsync(userId, savedItemId);
        }

        public async Task<bool> UpdateAsync(Guid userId,Guid reminderId,DateTime remindAt)
        {
            ValidateRemindAt(remindAt);
            return await _reminderRepository.UpdateAsync(userId, reminderId, remindAt);
        }

        public async Task<bool> SetCompletionAsync(Guid userId,Guid reminderId,bool isCompleted)
        {
            return await _reminderRepository.SetCompletionAsync(userId, reminderId, isCompleted);
        }

        public async Task<bool> DeleteAsync(Guid userId,Guid reminderId)
        {
            
            return await _reminderRepository.DeleteAsync(userId, reminderId);
        }
        private void ValidateRemindAt(DateTime remindAt)
        {
            if (remindAt <= DateTime.UtcNow)
            {
                _logger.LogWarning("Attempted to set a reminder for a past time: {RemindAt}.", remindAt);
                throw new ArgumentException("Reminder time must be in the future.",nameof(remindAt));
            }
        }
    }
}