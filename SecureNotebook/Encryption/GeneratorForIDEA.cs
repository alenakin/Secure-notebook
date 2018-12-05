using System;
using System.Text;

namespace SecureNotebook.Encryption
{
    public static class GeneratorForIDEA
    {
        private const int KeyLength = 128;
        private const int IVLength = 64;
        private static Random rnd = new Random();

        public static string GetKey()
        {
            byte[] byteKey = new byte[KeyLength / 8];
            rnd.NextBytes(byteKey);

            return Encoding.UTF8.GetString(byteKey);
        }

        public static long GetIV()
        {
            byte[] IV = new byte[IVLength / 8];
            rnd.NextBytes(IV);

            return BitConverter.ToInt64(IV, 0);
        }
    }
}