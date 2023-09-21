using RushHour.Domain.Abstractions.Wrappers;
using System.Security.Cryptography;

namespace RushHour.API.Wrappers
{
    public class Rfc2898DeriveBytesWrapper : IRfc2898DeriveBytesWrapper
    {
        private readonly IEncodingWrapper _encoding;
        public Rfc2898DeriveBytesWrapper(IEncodingWrapper encodingWrapper)
        {
                _encoding = encodingWrapper;
        }
        public byte[] Pbkdf2(string password, byte[] salt, int iterations, HashAlgorithmName hashAlgorithm, int outputLegth)
        {
           return Rfc2898DeriveBytes.Pbkdf2(
                _encoding.GetBytes(password),
                 salt,
                 iterations,
                 hashAlgorithm,
                 outputLegth);
        }

    }
}
