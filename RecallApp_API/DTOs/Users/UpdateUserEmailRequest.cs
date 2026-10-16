using System.ComponentModel.DataAnnotations;

namespace RecallApp_API.DTOs.Users
{
    public sealed class UpdateUserEmailRequest
    {  
        [Required]
        [EmailAddress]
        [StringLength(320)]
        public string Email { get; set; } = string.Empty;
    }
}
