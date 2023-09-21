using System.Security.Cryptography;

namespace RushHour.Domain.Abstractions.Wrappers
{
    public interface IRfc2898DeriveBytesWrapper
    {
        public byte[] Pbkdf2(string password, byte[] salt, int iterations, HashAlgorithmName hashAlgorithm, int outputLegth);

    }
}
