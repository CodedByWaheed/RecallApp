using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_BL.Services.Model
{
    public class EncryptionResult
    {
        public byte[] Ciphertext { get; init; } = null!;
        public byte[] Nonce { get; init; } = null!;
        public byte[] Tag { get; init; } = null!;
    }
}
