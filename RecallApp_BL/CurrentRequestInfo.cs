using Microsoft.AspNetCore.Http;

public sealed class CurrentRequestInfo
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentRequestInfo(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? IpAddress => _httpContextAccessor.HttpContext?
            .Connection.RemoteIpAddress?
            .ToString();

    public string? UserAgent => _httpContextAccessor.HttpContext?
            .Request.Headers["User-Agent"]
            .ToString();
}