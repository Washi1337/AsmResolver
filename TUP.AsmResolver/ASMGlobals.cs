using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.PE;
namespace TUP.AsmResolver
{
    class ASMGlobals
    {


        internal static string ByteArrayToHexString(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i <= bytes.Length - 1; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString().ToUpper();
        }
        internal static string ByteArrayToDecimalString(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i <= bytes.Length - 1; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return Int64.Parse(builder.ToString(),System.Globalization.NumberStyles.AllowHexSpecifier).ToString();
        }

        internal static byte[] HexStringToByteArray(string hexstring)
        {
            switch (hexstring.Length)
            {
                case 2:
                    byte bnumber = byte.Parse(hexstring, System.Globalization.NumberStyles.AllowHexSpecifier);
                    return new byte[] {bnumber};
                case 4:
                    short snumber = short.Parse(hexstring, System.Globalization.NumberStyles.AllowHexSpecifier);
                    return BitConverter.GetBytes(snumber);
                case 8:
                    int inumber = int.Parse(hexstring, System.Globalization.NumberStyles.AllowHexSpecifier);
                    return BitConverter.GetBytes(inumber);
                case 16:
                    long lnumber = long.Parse(hexstring, System.Globalization.NumberStyles.AllowHexSpecifier);
                    return BitConverter.GetBytes(lnumber);
            }

            return null;
        }

        internal static string StringBefore(string str, string val)
        {
            int index = str.LastIndexOf(val);
            if (index == -1)
            {
                return "";
            }
            else
            {
                return str.Substring(0, index);
            }
        }
        internal static string StringAfter(string str, string val)
        {
            int index = str.LastIndexOf(val);
            if (index == -1)
            {
                return "";
            }
            else
            {
                return str.Substring(index + val.Length);
            }
        }
        internal static byte[] MergeBytes(byte[] bytearray1, byte[] bytearray2)
        {
            if (bytearray1 == null)
                return bytearray2;
            if (bytearray2 == null)
                return bytearray1;

            byte[] merged = bytearray1;
            Array.Resize(ref merged, merged.Length + bytearray2.Length);
            for (int i = 0; i < bytearray2.Length; i++)
                merged[i + bytearray1.Length] = bytearray2[i];

            return merged;
        }

        internal static long ReadDword(byte[] bytearray)
        {
            long number = long.Parse(ByteArrayToHexString(bytearray), System.Globalization.NumberStyles.AllowHexSpecifier);

            if (number >= 0x80000000)
            {
                number = (0xFFFFFFFF - number + 1 ) * - 1;
            }

            return number;
        }


       

      
        internal static double Floor(double value, int digits)
        {
            if ((digits < -15) || (digits > 15))
                throw new ArgumentOutOfRangeException("digits", "Rounding digits must be between -15 and 15, inclusive.");

            double x = value;
            while (x >= (0x1000))
            {
                x -= 0x1000;
            }
            return x;
        }

        internal static string GetStringByOffset(PeImage peImage, int offset)
        {
            return GetStringByOffset(peImage, offset, 0);
        }
        internal static string GetStringByOffset(PeImage peImage, int offset, byte stopcharacter)
        {
            int tempposition = (int)peImage.stream.Position;
            peImage.SetOffset(offset);
            List<byte> bytes = new List<byte>();
            try
            {
                byte currentByte = peImage.ReadByte();
                while (currentByte != stopcharacter)
                {
                    bytes.Add(currentByte);
                    currentByte = peImage.ReadByte();
                }

                return Encoding.ASCII.GetString(bytes.ToArray());

            }
            catch
            {
                throw;
            }
            finally
            {
                peImage.stream.Position = tempposition;
                bytes.Clear();
            }
        }

        internal static sbyte ByteToSByte(byte b)
        {
            sbyte result = 0;
            if (b > 0x7F)
                result = (sbyte)((0xFF - b + 1) * -1);
            else
                result = (sbyte)b;
            return result;
        }

    }
}
