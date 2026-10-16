namespace RecallApp_API.DTOs.SavedItems
{
    public class UpdateSavedItemRequest
    {
        public byte Type { get; set; }

        public byte Priority { get; set; }

        public byte Status { get; set; }

        public string Content { get; set; } = string.Empty;
    }
}
