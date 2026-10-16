using System.ComponentModel.DataAnnotations;

namespace RecallApp_API.DTOs.Users
{
    public class DeleteAccountRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;
    }
}
