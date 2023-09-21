using System;
namespace RushHour.Domain.Abstractions.Wrappers
{
    public interface IHttpContextAccessorWrapper
    {
        string? GetUserRole();
        int GetLoggedUserId();
    }
}
