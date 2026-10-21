using RecallApp_BL.DTOs.LoginResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_BL.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(string email, string password);
        Task<LoginResult?> RefreshAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);
    }
}
