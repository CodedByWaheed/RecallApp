using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.JsonWebTokens;

namespace RecallApp_BL.Services.Security
{
    public sealed class CurrentUserService:ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                ClaimsPrincipal? user =_httpContextAccessor.HttpContext?.User;

                string? userIdValue =user?.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? user?.FindFirstValue(JwtRegisteredClaimNames.Sub);

                return Guid.TryParse(userIdValue, out Guid userId)? userId: null;
            }
        }
    }
}
