using System;
using System.Collections;

namespace SecureNotebook.Encryption
{
    public static class IDEAOperations
    {
        private const int Module = 65537;

        public static byte[] Xor(byte[] a, byte[] b)
        {
            return BitConverter.GetBytes(BitConverter.ToUInt64(a, 0) ^ BitConverter.ToUInt64(b, 0));
        }

        public static byte[] LeftShift(byte[] key, int shift)
        {
            bool[] boolKey = BytesToBoolArray(key);
            bool[] shiftedBoolKey = new bool[boolKey.Length];

            Array.Copy(boolKey, shift % boolKey.Length, shiftedBoolKey, 0, boolKey.Length - shift);
            Array.Copy(boolKey, 0, shiftedBoolKey, boolKey.Length - shift, shift);

            return BoolArrayToBytes(shiftedBoolKey);
        }

        public static int AdditionMod(int a, int b)
        {
            return (a + b) & 0xFFFF;
        }

        public static int MultiplicationMod(int a, int b)
        {
            long m = (long)a * b;

            if (m != 0)
            {
                return (int)(m % Module) & 0xFFFF;
            }
            else
            {
                if (a != 0 || b != 0)
                {
                    return (1 - a - b) & 0xFFFF;
                }

                return 0xFFFF + 1;
            }
        }

        public static int AdditiveInversion(int a)
        {
            return 65536 - a;
        }

        public static int BinaryPow(long a, int n)
        {
            long r = 1;
            while (n > 0)
            {
                if ((n & 1) != 0)
                {
                    r = (r * a) % Module;
                }

                n >>= 1;
                a = (a * a) % Module;
            }

            return (int)r;
        }

        public static int MultiplicationInversion(int x)
        {
            return BinaryPow(x, Module - 2);
        }

        #region Bytes - bools converting

        private static bool[] BytesToBoolArray(byte[] byteArray)
        {
            BitArray bitArray = new BitArray(byteArray);
            bool[] boolArray = new bool[bitArray.Length];
            bitArray.CopyTo(boolArray, 0);

            bool[] result = new bool[bitArray.Length];
            int currentResultIdx = 0;
            for (int i = 16; i <= boolArray.Length; i += 16)
            {
                for (int k = i - 1; k >= i - 16; k--)
                {
                    result[currentResultIdx++] = boolArray[k];
                }
            }

            return result;
        }

        private static byte[] BoolArrayToBytes(bool[] boolArray)
        {
            bool[] reversedBoolArray = new bool[boolArray.Length];

            int currentReversedIdx = 0;
            for (int i = 16; i <= boolArray.Length; i += 16)
            {
                for (int k = i - 1; k >= i - 16; k--)
                {
                    reversedBoolArray[currentReversedIdx++] = boolArray[k];
                }
            }

            BitArray bitArray = new BitArray(reversedBoolArray);
            byte[] byteArray = new byte[boolArray.Length / 8];
            bitArray.CopyTo(byteArray, 0);

            return byteArray;
        }

        #endregion
    }
}
