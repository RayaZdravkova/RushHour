namespace RushHour.Domain.Abstractions.Wrappers
{
    public interface IRandomNumberGeneratorWrapper
    {
        public byte[] GetBytes(int count);
    }
}
