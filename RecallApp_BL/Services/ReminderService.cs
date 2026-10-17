using Microsoft.Extensions.Logging;

using RecallApp_BL.Services.Interfaces;
using RecallApp_DL.Interfaces;
using RecallApp_DL.Models;
using RecallApp_DL.Repositories.Interfaces;

namespace RecallApp_BL.Services
{
    public class ReminderService : IReminderService
    {
        private readonly IReminderRepository _reminderRepository;
        private readonly ILogger<ReminderService> _logger;
        private readonly IAuditLogRepository _auditLog;
        private readonly CurrentRequestInfo _currentRequestInfo;

        public ReminderService(IReminderRepository reminderRepository,ILogger<ReminderService> logger,
            IAuditLogRepository auditLog,CurrentRequestInfo currentRequestInfo)
        {
            _reminderRepository = reminderRepository;
            _logger = logger;
            _auditLog = auditLog;
            _currentRequestInfo = currentRequestInfo;
        }

        public async Task<Guid> CreateAsync(Guid userId,Guid savedItemId,DateTime remindAt)
        {
            ValidateRemindAt(remindAt);

            Guid? reminderId = null;

            try
            {
                reminderId = await _reminderRepository.CreateAsync(userId,savedItemId,remindAt);

                await WriteAuditSafelyAsync(
                    userId,
                    "CreateReminder",
                    "Reminders",
                    reminderId.Value.ToString(),
                    "Success",
                    "Reminder created successfully.");

                return reminderId.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create reminder for user {UserId} and saved item {SavedItemId}.",
                    userId,
                    savedItemId);

                await WriteAuditSafelyAsync(
                    userId,
                    "CreateReminder",
                    "Reminders",
                    reminderId?.ToString(),
                    "Failure",
                    "Reminder creation failed.");

                throw;
            }
        }

        public async Task<List<ReminderModel>> GetPendingByUserIdAsync(Guid userId)
        {
            try
            {
                List<ReminderModel> reminders =await _reminderRepository.GetPendingByUserIdAsync(userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "GetPendingReminders",
                    "Reminders",
                    null,
                    "Success",
                    "Pending reminders retrieved successfully.");

                return reminders;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to retrieve pending reminders for user {UserId}.",
                    userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "GetPendingReminders",
                    "Reminders",
                    null,
                    "Failure",
                    "Failed to retrieve pending reminders.");

                throw;
            }
        }

        public async Task<List<ReminderModel>> GetBySavedItemIdAsync(Guid userId,Guid savedItemId)
        {
            try
            {
                List<ReminderModel> reminders = await _reminderRepository.GetBySavedItemIdAsync(
                        userId,
                        savedItemId);

                await WriteAuditSafelyAsync(
                    userId,
                    "GetRemindersBySavedItem",
                    "Reminders",
                    savedItemId.ToString(),
                    "Success",
                    "Reminders for saved item retrieved successfully.");

                return reminders;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to retrieve reminders for saved item {SavedItemId}, user {UserId}.",
                    savedItemId,
                    userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "GetRemindersBySavedItem",
                    "Reminders",
                    savedItemId.ToString(),
                    "Failure",
                    "Failed to retrieve reminders for saved item.");

                throw;
            }
        }

        public async Task<bool> UpdateAsync(Guid userId,Guid reminderId,DateTime remindAt)
        {
            ValidateRemindAt(remindAt);

            try
            {
                bool updated = await _reminderRepository.UpdateAsync(userId,reminderId,remindAt);

                await WriteAuditSafelyAsync(
                    userId,
                    "UpdateReminder",
                    "Reminders",
                    reminderId.ToString(),
                    updated ? "Success" : "Failure",
                    updated
                        ? "Reminder updated successfully."
                        : "Reminder update did not complete.");

                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to update reminder {ReminderId} for user {UserId}.",
                    reminderId,
                    userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "UpdateReminder",
                    "Reminders",
                    reminderId.ToString(),
                    "Failure",
                    "Reminder update failed.");

                throw;
            }
        }

        public async Task<bool> SetCompletionAsync(Guid userId,Guid reminderId,bool isCompleted)
        {
            try
            {
                bool updated =await _reminderRepository.SetCompletionAsync(userId,reminderId,isCompleted);

                await WriteAuditSafelyAsync(
                    userId,
                    "SetReminderCompletion",
                    "Reminders",
                    reminderId.ToString(),
                    updated ? "Success" : "Failure",
                    updated
                        ? $"Reminder completion status set to {isCompleted}."
                        : "Reminder completion status update did not complete.");

                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to change completion status for reminder {ReminderId}, user {UserId}.",
                    reminderId,
                    userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "SetReminderCompletion",
                    "Reminders",
                    reminderId.ToString(),
                    "Failure",
                    "Reminder completion status update failed.");

                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid userId,Guid reminderId)
        {
            try
            {
                bool deleted = await _reminderRepository.DeleteAsync(userId,reminderId);

                await WriteAuditSafelyAsync(
                    userId,
                    "DeleteReminder",
                    "Reminders",
                    reminderId.ToString(),
                    deleted ? "Success" : "Failure",
                    deleted
                        ? "Reminder deleted successfully."
                        : "Reminder deletion did not complete.");

                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to delete reminder {ReminderId} for user {UserId}.",
                    reminderId,
                    userId);

                await WriteAuditSafelyAsync(
                    userId,
                    "DeleteReminder",
                    "Reminders",
                    reminderId.ToString(),
                    "Failure",
                    "Reminder deletion failed.");

                throw;
            }
        }

        private void ValidateRemindAt(DateTime remindAt)
        {
            if (remindAt <= DateTime.UtcNow)
            {
                _logger.LogWarning(
                    "Attempted to set a reminder for a past time: {RemindAt}.",
                    remindAt);

                throw new ArgumentException(
                    "Reminder time must be in the future.",
                    nameof(remindAt));
            }
        }

        private async Task WriteAuditSafelyAsync(Guid? userId,string action,string entityType,string? entityId,
            string outcome,string details)
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
