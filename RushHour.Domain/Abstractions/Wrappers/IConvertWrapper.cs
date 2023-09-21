namespace RushHour.Domain.Abstractions.Wrappers
{
    public interface IConvertWrapper
    {
        public byte[] FromHexString(string text);

        public string ToHexString(byte[] array);

        public double ToDouble(string text);
    }
}
