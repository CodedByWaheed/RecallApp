using RecallApp_DL.Models;

namespace RecallApp_DL.Repositories.Interfaces
{
    public interface IReminderRepository
    {
        Task<Guid> CreateAsync(Guid userId,Guid savedItemId,DateTime remindAt);
        Task<List<ReminderModel>> GetPendingByUserIdAsync(Guid userId);
        Task<List<ReminderModel>> GetBySavedItemIdAsync(Guid userId,Guid savedItemId);
        Task<bool> UpdateAsync(Guid userId,Guid reminderId,DateTime remindAt);
        Task<bool> SetCompletionAsync(Guid userId,Guid reminderId,bool isCompleted);
        Task<bool> DeleteAsync(Guid userId,Guid reminderId);
    }
}