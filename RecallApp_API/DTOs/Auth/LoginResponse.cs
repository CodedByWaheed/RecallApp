namespace RecallApp_API.DTOs.Auth
{
    public sealed class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
