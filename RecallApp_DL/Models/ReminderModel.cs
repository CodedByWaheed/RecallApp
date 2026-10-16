using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_DL.Models
{
    public sealed class ReminderModel
    {
        public Guid Id { get; set; }
        public Guid SavedItemId { get; set; }
        public DateTime RemindAt { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
