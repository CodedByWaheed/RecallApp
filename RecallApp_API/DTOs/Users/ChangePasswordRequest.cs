using System.ComponentModel.DataAnnotations;

namespace RecallApp_API.DTOs.Users
{
    public sealed class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [MaxLength(128)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
