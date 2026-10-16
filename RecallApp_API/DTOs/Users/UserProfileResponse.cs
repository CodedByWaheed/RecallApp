namespace RecallApp_API.DTOs.Users
{
    public sealed class UserProfileResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
