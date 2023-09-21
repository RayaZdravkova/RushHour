using RushHour.Domain.Abstractions.Wrappers;
using System.Security.Claims;

namespace RushHour.API.Wrappers
{
    public class HttpContextAccessorWrapper : IHttpContextAccessorWrapper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public HttpContextAccessorWrapper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public string? GetUserRole() 
        {
            string? userRole = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

            return userRole;
        }

        public int GetLoggedUserId()
        {
            return Convert.ToInt32(_httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
    }
}
