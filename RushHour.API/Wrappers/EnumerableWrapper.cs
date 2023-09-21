using RushHour.Domain.Abstractions.Wrappers;

namespace RushHour.API.Wrappers
{
    public class EnumerableWrapper : IEnumerableWrapper
    {
        private readonly IConvertWrapper _convertWrapper;
        public EnumerableWrapper(IConvertWrapper convertWrapper)
        {
            _convertWrapper = convertWrapper;
        }
        public bool SequenceEqual(string text, byte[] textToCompare)
        {
            return textToCompare.SequenceEqual(_convertWrapper.FromHexString(text));
        }
    }
}
