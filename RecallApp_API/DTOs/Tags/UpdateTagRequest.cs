using System.ComponentModel.DataAnnotations;

namespace RecallApp_API.DTOs.Tags
{
    public class UpdateTagRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;
    }
}
