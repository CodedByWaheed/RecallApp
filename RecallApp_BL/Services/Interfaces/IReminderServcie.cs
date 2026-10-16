using RecallApp_DL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_BL.Services.Interfaces
{
    public interface IReminderService
    {
        Task<Guid> CreateAsync(Guid userId,Guid savedItemId,DateTime remindAt);

        Task<List<ReminderModel>> GetPendingByUserIdAsync(Guid userId);

        Task<List<ReminderModel>> GetBySavedItemIdAsync(Guid userId,Guid savedItemId);

        Task<bool> UpdateAsync(Guid userId,Guid reminderId,DateTime remindAt);

        Task<bool> SetCompletionAsync(Guid userId,Guid reminderId,bool isCompleted);

        Task<bool> DeleteAsync(Guid userId,Guid reminderId);
    }
}
