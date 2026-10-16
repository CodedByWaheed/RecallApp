using System.ComponentModel.DataAnnotations;

namespace RecallApp_API.DTOs.Users
{
    public class RegisterUserRequest
    {
        [Required]
        [EmailAddress]
        [StringLength(320)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [MaxLength(128)]
        public string Password { get; set; } = string.Empty;
    }
}
