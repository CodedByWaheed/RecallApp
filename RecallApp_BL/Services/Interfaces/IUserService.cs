
using RecallApp_DL.Models;

namespace RecallApp_BL.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserModel?> GetProfileAsync(Guid userId);
        Task<Guid> RegisterAsync(string email, string password);
        Task<bool> UpdateEmailAsync(Guid userId,string email);
        Task<bool> ChangePasswordAsync(Guid userId,string currentPassword,string newPassword);
        Task<bool> DeleteAccountAsync( Guid userId,string currentPassword);
    }
}
