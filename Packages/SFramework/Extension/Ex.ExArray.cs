using System;


namespace Ex
{
    public static class ExArray
    {
        public static byte[] CopyOf(byte[] data, int newLength)
        {
            byte[] tmp = new byte[newLength];
            Array.Copy(data, 0, tmp, 0, System.Math.Min(newLength, data.Length));
            return tmp;
        }
    }
}
