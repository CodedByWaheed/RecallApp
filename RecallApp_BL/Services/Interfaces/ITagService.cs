using RecallApp_DL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_BL.Services.Interfaces
{
    public interface ITagService
    {
        Task<Guid> CreateAsync(Guid userId, string name);

        Task<List<TagModel>> GetByUserIdAsync(Guid userId);

        Task<bool> UpdateAsync(Guid userId, Guid tagId, string name);

        Task<bool> DeleteAsync(Guid userId, Guid tagId);

        Task AssignAsync(Guid userId, Guid savedItemId, Guid tagId);

        Task<bool> RemoveAsync(Guid userId, Guid savedItemId, Guid tagId);

        Task<List<TagModel>> GetBySavedItemIdAsync(Guid userId,Guid savedItemId);
    }
}
