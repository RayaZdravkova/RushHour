using RushHour.Domain.Abstractions.Wrappers;

namespace RushHour.API.Wrappers
{
    public class GuidWrapper : IGuidWrapper
    {
        public Guid NewGuid()
        {
            return Guid.NewGuid();
        }
    }
}
