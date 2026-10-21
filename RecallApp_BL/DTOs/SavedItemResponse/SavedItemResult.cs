using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecallApp_BL.DTOs.TagResponse;
using RecallApp_DL.Models;

namespace RecallApp_BL.DTOs.SavedItemResponse
{
    public sealed class SavedItem
    {
        //public SavedItemModel savedItem = null!;
        //List<EncryptedContentModel> encryptedContents = null!;
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string Note { get; set; } = null!;
        public byte Type { get; set; }
        public byte Priority { get; set; }
        public byte Status { get; set; }
        public List<string> Tags { get; set; } = null!;

    }
}
