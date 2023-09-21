namespace RushHour.Domain.Abstractions.Wrappers
{
    public interface IEncodingWrapper
    {
        public byte[] GetBytes(string text);
    }
}
