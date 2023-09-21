using RushHour.Domain.Abstractions.Wrappers;
using System.Text;

namespace RushHour.API.Wrappers
{
    public class EncodingWrapper : IEncodingWrapper
    {
        public byte[] GetBytes(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }
    }
}
