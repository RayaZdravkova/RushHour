using RushHour.Domain.Abstractions.Wrappers;

namespace RushHour.API.Wrappers
{
    public class ConvertWrapper : IConvertWrapper
    {
        public byte[] FromHexString(string text)
        {
            return Convert.FromHexString(text);
        }     
        public string ToHexString(byte[] array)
        {
            return Convert.ToHexString(array);
        } 
        public double ToDouble(string text)
        {
            return Convert.ToDouble(text);
        }
    }
}
