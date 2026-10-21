using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_DL.Models
{
    public class SavedItemModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public byte Type { get; set; }
        public byte Priority { get; set; }
        public byte Status   { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<EncryptedContentModel> encryptedContents { get; set; } = null!;
    }
}
