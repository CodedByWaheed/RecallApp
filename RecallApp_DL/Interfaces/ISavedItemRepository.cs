using RecallApp_DL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_DL.Interfaces
{
    public interface ISavedItemRepository
    {
        Task<SavedItemModel> GetByIdAsync(Guid ID);
        Task<Guid> CreateAsync(SavedItemModel model);
        Task<List<SavedItemModel>> GetByUserIdAsync(Guid userId);
        Task UpdateAsync(SavedItemModel model);
        Task<bool> ArchiveAsync(Guid savedItemId, Guid userId);
        Task<bool> RestoreAsync(Guid savedItemId, Guid userId);
        Task<List<SavedItemModel>> GetArchivedByUserIdAsync(Guid userId);
        Task<bool> DeleteAsync(Guid savedItemId, Guid userId);
        Task<List<SavedItemModel>> GetByTagIdAsync(Guid userId,Guid tagId);
    }
}
