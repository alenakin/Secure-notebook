using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static SecureNotebook.Encryption.IDEAOperations;

namespace SecureNotebook.Encryption
{
    public class IDEA
    {
        private const int KeySize = 128;
        private const int SubkeysCount = 52;
        private const int ShiftLength = 25;
        private const int BLOCK_LENGTH = 64;
        private const int SUBBLOCK_LENGTH = 16;
        private const int SUBKEY_LENGTH = 16;
        private const int SUBBLOCK_COUNT = 4;
        private const int mod = 65537;
        static Random rnd = new Random();

        private byte[] key;
        private int[] subkeys;
        private int[] dekeys;

        public IDEA(string key)
        {
            byte[] byteKey = Encoding.UTF8.GetBytes(key);

            if (byteKey.Length != KeySize / 8)
            {
                throw new ArgumentException($"Key must be {KeySize} bits");
            }

            subkeys = GetSubkeys(byteKey);
            dekeys = GetDekeys(subkeys);
        }

        #region Methods for getting round subkeys

        public int[] GetSubkeys(byte[] key)             //получение подключей
        {
            int[] subkeys = new int[SubkeysCount];
            byte[] byteKey = key;

            int i = 0, l = 0, m = 0;
            while (i != SubkeysCount)
            {
                if (l == 8)
                {
                    l = 0;
                    m = 0;
                    byteKey = LeftShift(byteKey, ShiftLength);
                }

                subkeys[i++] = BitConverter.ToUInt16(byteKey, m);
                l++;
                m += 2;
            }

            return subkeys;
        }

        private int[] GetDekeys(int[] keys)          //получение подключей для расшифровки
        {
            int[] k = new int[SubkeysCount];
            int j = 0, i = 48;

            k[j++] = MultiplicationInversion(keys[i]);
            k[j++] = AdditiveInversion(keys[i + 1]);
            k[j++] = AdditiveInversion(keys[i + 2]);
            k[j++] = MultiplicationInversion(keys[i + 3]);

            while (j != 46)
            {
                i -= 2;
                k[j++] = keys[i];
                k[j++] = keys[i + 1];
                i -= 4;

                k[j++] = MultiplicationInversion(keys[i]);
                k[j++] = AdditiveInversion(keys[i + 2]);
                k[j++] = AdditiveInversion(keys[i + 1]);
                k[j++] = MultiplicationInversion(keys[i + 3]);
            }

            i -= 2;
            k[j++] = keys[i];
            k[j++] = keys[i + 1];
            i -= 4;

            k[j++] = MultiplicationInversion(keys[i]);
            k[j++] = AdditiveInversion(keys[i + 1]);
            k[j++] = AdditiveInversion(keys[i + 2]);
            k[j++] = MultiplicationInversion(keys[i + 3]);

            return k;
        }

        #endregion

        private int[] getSubBlocks(byte[] block)              //делит блок данных 64 бит на 4 части по 16 бит
        {
            int[] parts = new int[4];
            for (int i = 0; i < 4; i++)
                parts[i] = BitConverter.ToUInt16(block, i * 2);
            return parts;
        }

        private byte[] addEnd(byte[] text)
        {
            List<byte> a = new List<byte>(text);
            int tail = a.Count % 8;
            if (tail != 0)
            {
                byte[] nules = new byte[8 - tail];
                a.AddRange(nules);
            }
            else
                tail = 8;
            byte[] end = new byte[8];
            end[0] = (byte)tail;
            a.AddRange(end);
            return a.ToArray();
        }

        private byte[] deleteEnd(List<byte> ctext)
        {
            int tail = ctext[ctext.Count - 8];
            int N = ctext.Count - 8 - (8 - tail);
            byte[] res = new byte[N];
            ctext.CopyTo(0, res, 0, N);
            return res;
        }

        //********************Cipher Block Chaining

        public byte[] encryptCBC(byte[] text, out byte[] IV)
        {
            IV = new byte[BLOCK_LENGTH / 8];    //вектор инициализации (синхропосылка)
            rnd.NextBytes(IV);

            byte[] temp = new byte[8];
            ulong t = 0;
            int[] keys = GetSubkeys(key);

            List<byte> res = new List<byte>();
            List<ulong> C = new List<ulong>();
            C.Add(BitConverter.ToUInt64(IV, 0));
            byte[] ntext = addEnd(text);
            int n = ntext.Length / 8;

            for (int i = 0; i < n; i++)
            {
                t = BitConverter.ToUInt64(ntext, i * 8) ^ C[i];
                temp = blockManipulations(BitConverter.GetBytes(t), keys);
                res.AddRange(temp);
                C.Add(BitConverter.ToUInt64(temp, 0));
            }
            return res.ToArray();
        }

        public byte[] decryptCBC(byte[] ctext, byte[] IV)
        {
            int n = ctext.Length / 8;
            int[] keys = GetSubkeys(key);
            int[] dekeys = GetDekeys(keys);

            byte[] temp = new byte[8];
            byte[] D = new byte[8];
            List<byte> res = new List<byte>();
            List<ulong> C = new List<ulong>();
            C.Add(BitConverter.ToUInt64(IV, 0));

            for (int i = 0; i < n; i++)
                C.Add(BitConverter.ToUInt64(ctext, i * 8));

            for (int i = 0; i < n; i++)
            {
                D = blockManipulations(BitConverter.GetBytes(C[i + 1]), dekeys);
                res.AddRange(BitConverter.GetBytes(C[i] ^ BitConverter.ToUInt64(D, 0)));
            }

            return deleteEnd(res);
        }

        public string encryptCBC(string text, out byte[] IV)
        {
            byte[] textB = Encoding.Default.GetBytes(text);
            return Encoding.Default.GetString(encryptCBC(textB, out IV));
        }

        public string decryptCBC(string ctext, byte[] IV)
        {
            byte[] textB = Encoding.Default.GetBytes(ctext);
            return Encoding.Default.GetString(decryptCBC(textB, IV));
        }

        private byte[] encrypt(byte[] block, int[] keys)
        {
            int n = block.Length / 8;
            byte[] temp = new byte[8];
            List<byte> res = new List<byte>();
            for (int i = 0; i < n; i++)
            {
                Array.Copy(block, i * 8, temp, 0, 8);
                res.AddRange(blockManipulations(temp, keys));
            }
            return res.ToArray();
        }

        private byte[] blockManipulations(byte[] block, int[] keys)
        {
            int[] b = getSubBlocks(block);
            int[] res = new int[4];
            int[] k = new int[6];

            for (int i = 0; i < 8; i++)
            {
                Array.Copy(keys, i * 6, k, 0, 6);
                b = makeRound(k, b);
            }

            res[0] = MultiplicationMod(b[0], keys[48]);
            res[1] = AdditionMod(b[2], keys[49]);
            res[2] = AdditionMod(b[1], keys[50]);
            res[3] = MultiplicationMod(b[3], keys[51]);

            byte[] r = getTextSeq(res);
            return r;
        }

        private byte[] getTextSeq(int[] a)               //объединение подблоков в последовательность байтов
        {
            byte[] res = new byte[8];
            for (int i = 0; i < a.Count(); i++)
                Array.Copy(BitConverter.GetBytes(a[i]), 0, res, i * 2, 2);
            return res;
        }

        private int[] makeRound(int[] K, int[] T)       //действия в одном раунде
        {
            int[] NB = new int[4];
            int A = 0, B = 0, C = 0, D = 0, E = 0, F = 0;

            A = MultiplicationMod(T[0], K[0]);
            B = AdditionMod(T[1], K[1]);
            C = AdditionMod(T[2], K[2]);
            D = MultiplicationMod(T[3], K[3]);

            E = A ^ C;
            F = B ^ D;

            NB[0] = A ^ MultiplicationMod(AdditionMod(F, MultiplicationMod(E, K[4])), K[5]);                                 //NB[0] = A ^ ((F + E * K[4]) * K[5]);
            NB[1] = C ^ MultiplicationMod(AdditionMod(F, MultiplicationMod(E, K[4])), K[5]);                                 //NB[1] = C ^ ((F + E * K[4]) * K[5]);
            NB[2] = B ^ AdditionMod(MultiplicationMod(E, K[4]), MultiplicationMod(AdditionMod(F, MultiplicationMod(E, K[4])), K[5]));       //NB[2] = B ^ (E * K[4] + (F + E * K[4]) * K[5]);
            NB[3] = D ^ AdditionMod(MultiplicationMod(E, K[4]), MultiplicationMod(AdditionMod(F, MultiplicationMod(E, K[4])), K[5]));       //NB[3] = D ^ (E * K[4] + (F + E * K[4]) * K[5]);

            return NB;
        }
    }
}
