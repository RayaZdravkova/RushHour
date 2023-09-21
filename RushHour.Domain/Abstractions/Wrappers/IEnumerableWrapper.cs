namespace RushHour.Domain.Abstractions.Wrappers
{
    public interface IEnumerableWrapper
    {
        public bool SequenceEqual(string text, byte[] textToCompare);
    }
}
