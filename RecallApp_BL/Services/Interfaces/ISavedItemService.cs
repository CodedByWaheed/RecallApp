using RecallApp_BL.DTOs.SavedItemResponse;
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
        Task<Guid> CreateAsync(Guid userId,byte type,byte priority,byte status,string titlePlainText,string contentPlaintext , string notePlainText);
        //Task<string?> GetContentAsync(Guid userId,Guid savedItemId);
        Task<SavedItem?>GetSavedItemAsync(Guid userId, Guid savedItemId);
        Task<List<SavedItem>> GetByUserIdAsync(Guid userId);
        Task UpdateAsync(Guid userId,Guid savedItemId,byte type,byte priority,byte status,string titlePlaintext,string contentPlaintext , string notePlaintext);
        Task<bool> ArchiveAsync(Guid userId, Guid savedItemId);
        Task<bool> RestoreAsync(Guid userId, Guid savedItemId);
        Task<List<SavedItemModel>> GetArchivedByUserIdAsync(Guid userId);
        Task<bool> DeleteAsync(Guid userId, Guid savedItemId);
        Task<List<SavedItemModel>> GetByTagIdAsync(Guid userId,Guid tagId);
    }
}
