using System.ComponentModel.DataAnnotations;

namespace RecallApp_API.DTOs.Reminder
{
    public class CreateReminderRequest
    {
        [Required]
        public Guid SavedItemId { get; set; }

        [Required]
        public DateTimeOffset RemindAt { get; set; }
    }
}
