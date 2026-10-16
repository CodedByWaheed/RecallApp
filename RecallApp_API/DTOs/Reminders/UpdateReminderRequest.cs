using System.ComponentModel.DataAnnotations;

namespace RecallApp_API.DTOs.Reminder
{
    public class UpdateReminderRequest
    {
        [Required]
        public DateTimeOffset RemindAt { get; set; }
    }
}
