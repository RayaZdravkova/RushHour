using RushHour.Domain.Abstractions.Wrappers;

namespace RushHour.API.Wrappers
{
    public class DateTimeWrapper : IDateTimeWrapper
    {
       public DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}
