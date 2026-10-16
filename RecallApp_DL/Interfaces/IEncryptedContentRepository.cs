using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecallApp_DL.Models;

namespace RecallApp_DL.Interfaces
{
    public interface IEncryptedContentRepository
    {
        Task CreateAsync(EncryptedContentModel model);
        Task<List<EncryptedContentModel>> GetBySavedItemIdAsync(Guid userId , Guid savedItemId);
        Task UpdateAsync(EncryptedContentModel model);
    }
}
