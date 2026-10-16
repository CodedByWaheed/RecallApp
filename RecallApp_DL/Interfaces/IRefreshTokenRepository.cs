using RecallApp_DL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_DL.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task CreateAsync(RefreshTokenModel model);
        Task RevokeAsync(byte[] tokenHash);
        Task<RefreshTokenModel?> GetByTokenHashAsync(byte[] tokenHash);
        Task<Guid?> RotateAsync(byte[] oldTokenHash, byte[] newTokenHash, DateTime newExpiresAtUtc);
    }
}
