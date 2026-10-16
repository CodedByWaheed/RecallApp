using System.ComponentModel.DataAnnotations;

namespace RecallApp_API.DTOs.Tags
{
    public class AssignTagRequest
    {
        [Required]
        public Guid TagId { get; set; }
    }
}
