using System.ComponentModel.DataAnnotations;

namespace RecallApp_API.DTOs.SavedItems
{
    
    public sealed class CreateSavedItemRequest
    {
        //public Guid userId { get; set; }
        //public byte type { get; set; }
        //public byte priority { get; set; }
        //public byte status { get; set; }
        //public string plaintext { get; set; } = null!;

        [Required]
        [StringLength(100_000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;

        [Range(1, 3)]
        public byte Type { get; set; }

        [Range(1, 3)]
        public byte Priority { get; set; }

        [Range(1, 4)]
        public byte Status { get; set; }
    }
}
