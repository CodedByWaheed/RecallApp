using System.ComponentModel.DataAnnotations;

namespace RecallApp_API.DTOs.SavedItems
{
    public class UpdateSavedItemRequest
    {
        
        public byte Type { get; set; }
        public byte Priority { get; set; }
        public byte Status { get; set; }
      //  [StringLength(100_000, MinimumLength = 0)]
        public string Title { get; set; } = string.Empty;
      //  [StringLength(100_000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;
       // [StringLength(100_000, MinimumLength = 0)]
        public string Note { get; set; } = string.Empty;
    }
}
