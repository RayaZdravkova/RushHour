using RushHour.Domain.Abstractions.Wrappers;
using System.Security.Cryptography;

namespace RushHour.API.Wrappers
{
    public class RandomNumberGeneratorWrapper : IRandomNumberGeneratorWrapper
    {
        public byte[] GetBytes(int count)
        {
            return RandomNumberGenerator.GetBytes(count);
        }
    }
}
