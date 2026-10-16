using RecallApp_DL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_DL.Interfaces
{
    public interface IUserRepository
    {

        Task<UserModel?> GetByIdAsync(Guid ID);
        Task<Guid> CreateAsync(UserModel user);
        Task<UserModel?> GetByEmailAsync(string email);
        Task<bool> UpdateEmailAsync(Guid userId, string email);
        Task<bool> UpdatePasswordHashAsync(Guid userId, string passwordHash);
        Task<bool> DeleteAsync(Guid userId);
    }
}
