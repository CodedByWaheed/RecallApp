using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_DL.Models
{
    public sealed class RefreshTokenModel
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public byte[] TokenHash { get; set; } = null!;

        public DateTime ExpiresAtUtc { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? RevokedAtUtc { get; set; }

        public byte[]? ReplacedByTokenHash { get; set; }
    }
}
