using RecallApp_BL.Services.Model;
using RecallApp_DL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_BL.Services.Interfaces
{
    public interface ISavedItemService
    {
        Task<Guid> CreateAsync(Guid userId,byte type,byte priority,byte status,string plaintext);
        Task<string?> GetContentAsync(Guid userId,Guid savedItemId);
        Task<List<SavedItemModel>> GetByUserIdAsync(Guid userId);
        Task UpdateAsync(Guid userId,Guid savedItemId,byte type,byte priority,byte status,string plaintext);
        Task<bool> ArchiveAsync(Guid userId, Guid savedItemId);
        Task<bool> RestoreAsync(Guid userId, Guid savedItemId);
        Task<List<SavedItemModel>> GetArchivedByUserIdAsync(Guid userId);
        Task<bool> DeleteAsync(Guid userId, Guid savedItemId);
        Task<List<SavedItemModel>> GetByTagIdAsync(Guid userId,Guid tagId);
    }
}
