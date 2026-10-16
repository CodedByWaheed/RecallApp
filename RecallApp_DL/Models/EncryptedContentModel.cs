using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_DL.Models
{
    public class EncryptedContentModel
    {
        public Guid Id { get; set; }

        public Guid SavedItemId { get; set; }

        public byte FieldType { get; set; }

        public byte[] Ciphertext { get; set; } = null!;

        public byte[] Nonce { get; set; } = null!;

        public byte[] Tag { get; set; } = null!;

    }
}
