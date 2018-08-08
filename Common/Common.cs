using System;
using System.Linq;
using System.Text;
using System.Globalization;
/// <summary> 
/// Package for all common classes.
/// </summary>
namespace SocketCommon
{
    /// <summary>
    /// Handles byte and hex manipulation and calculations.
    /// </summary>
    public static class SocketUtil
    {
        /// <summary>
        /// 4-bit - 0001 : Represent Least Significant Bit
        /// </summary>
        public static readonly int CHECK_BIT_1 = 0x3;
        /// <summary>
        /// 4-bit - 0010
        /// </summary>
        public static readonly int CHECK_BIT_2 = 0x2;
        /// <summary>
        /// 4-bit - 0100
        /// </summary>
        public static readonly int CHECK_BIT_3 = 0x1;
        /// <summary>
        /// 4-bit - 1000 : Represent Most Significant Bit
        /// </summary>
        public static readonly int CHECK_BIT_4 = 0x0;
        /// <summary>
        /// Swaps the value of the two parameters.
        /// <code>
        /// T t = y;
        /// y = x;
        /// x = t;
        /// </code>
        /// </summary>
        /// <typeparam name="T">
        /// The element type of the passed parameters
        /// </typeparam>
        /// <param name="x"></param>
        /// <param name="y"></param>
        static public void Swap<T>(ref T x, ref T y)
        {
            T t = y;
            y = x;
            x = t;
        }
        /// <summary>
        /// Method overloading. <c>ByteArrayToHexString</c>
        /// for 1 parameter.
        /// </summary>
        /// <param name="data">Data from device</param>
        /// <returns>
        /// <c>ByteArrayToHexString(byte[] data, int start_idx, int length, bool hyphen)</c>
        /// </returns>
        static public string ByteArrayToHexString(byte[] data)
        {
            return SocketUtil.ByteArrayToHexString(data, 0, data.Length, false);
        }
        /// <summary>
        /// Converts byte array from device to hex string.
        /// </summary>
        /// <param name="data">Data from device</param>
        /// <param name="start_idx">The starting position within value</param>
        /// <param name="length">Length of data from device</param>
        /// <param name="hyphen">
        /// If data from device has hyphen format
        /// <example>
        /// Example: 
        /// <c>true</c> if data is "7E-01-00-64" ; 
        /// <c>false</c> if data is "7E010064";
        /// </example>
        /// </param>
        /// <returns>Hex string</returns>
        static public string ByteArrayToHexString(byte[] data, int start_idx, int length, bool hyphen)
        {
            if (hyphen)
                return BitConverter.ToString(data, start_idx, length);

            return BitConverter.ToString(data, start_idx, length).Replace("-", string.Empty);
        }
        /// <summary>
        /// Method overloading. <c>HexStringToByteArray</c>
        /// for 1 parameter.
        /// </summary>
        /// <param name="hex">Hex string</param>
        /// <returns>
        /// <c>HexStringToByteArray(string hex, int start_idx, int length)</c>
        /// </returns>
        static public byte[] HexStringToByteArray(string hex)
        {
            return SocketUtil.HexStringToByteArray(hex, 0, hex.Length);
        }
        /// <summary>
        /// Converts Hex string to byte array
        /// </summary>
        /// <param name="hex">Hex string</param>
        /// <param name="start_idx">The starting position within value</param>
        /// <param name="length">Length of value</param>
        /// <returns>byte[]</returns>
        static public byte[] HexStringToByteArray(string hex, int start_idx, int length)
        {
            return Enumerable.Range(start_idx, length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        /// <summary>
        /// Calculates data by XORing.
        /// <c>0x00 ^= byte</c>
        /// </summary>
        /// <param name="data">Byte array data</param>
        /// <param name="start_idx">The starting position within value</param>
        /// <param name="length">Length of value</param>
        /// <returns>Returns 0x00 in exception ; Returns XORed byte</returns>
        static public byte CalculateXorCheckSum(byte[] data, int start_idx, int length)
        {
            try
            {
                byte chksum = 0x00;

                for (int i = start_idx; i < length; i++)
                {
                    chksum ^= data[i];
                }
                return chksum;
            }
            catch { }
            return 0x00;
        }
        /// <summary>
        /// Calculates string data by XORing.
        /// Uses <see cref="CalculateXorCheckSum(byte[] data, int start_idx, int length)"/>.
        /// </summary>
        /// <param name="data">String data</param>
        /// <param name="start_idx">The starting position within value</param>
        /// <param name="length">Length of value</param>
        /// <returns>Returns empty string in exception ; Returns XORed string <paramref name="data"/></returns>
        static public string CalculateXorCheckSum(string data, int start_idx, int length)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                byte b = CalculateXorCheckSum(bytes, start_idx, length);
                return Convert.ToString(b, 16).PadLeft(2, '0').ToUpper();
            }
            catch { }
            return String.Empty;
        }

        static public short AnalyzeHexCharAsBit(char cHexChar, int nCheckBit)
        {
            try
            {
                SByte digit = Convert.ToSByte(cHexChar.ToString(), 16);    // to base 16. Sample = A
                char[] array = Convert.ToString(digit, 2).PadLeft(4, '0').ToArray(); // to 4bit binary. Sample = 1010
                return (short)(array[nCheckBit] == '1' ? 1 : 0);    // check which bit value
            }
            catch
            {
                throw new FormatException();
            }
        }

        static public short AnalyzeHexCharAsBitInvert(char cHexChar, int nCheckBit)
        {
            // Below method written by Ahamed to Invert the result(eg: LowBat) @ 22012016
            try
            {
                SByte digit = Convert.ToSByte(cHexChar.ToString(), 16);    // to base 16. Sample = A
                char[] array = Convert.ToString(digit, 2).PadLeft(4, '0').ToArray(); // to 4bit binary. Sample = 1010

                return (short)(array[nCheckBit] == '1' ? 0 : 1);    // check which bit value
            }
            catch
            {
                throw new FormatException();
            }
        }
        /// <summary>
        /// Converts hex character to a
        /// 16 base 16 bit signed integer.
        /// </summary>
        /// <param name="cHexChar">Hex character</param>
        /// <returns>Returns Int16</returns>
        static public short GetShortHexValue(char cHexChar)
        {
            try
            {
                return Convert.ToInt16(cHexChar.ToString(), 16);
            }
            catch
            {
                throw new FormatException();
            }
        }
        /// <summary>
        /// Encodes byte array to ASCII string.
        /// </summary>
        /// <param name="data">Byte array data</param>
        /// <param name="start_idx">The starting position within value</param>
        /// <param name="length">Length of value</param>
        /// <returns>Returns string</returns>
        static public string SubBytesAsString(this byte[] data, int start_idx, int length)
        {
            try
            {
                return Encoding.ASCII.GetString(data, start_idx, length);
            }
            catch { }
            return string.Empty;
        }
        /// <summary>
        /// Converts string DateTime to a specific format.
        /// </summary>
        /// <param name="date">String DateTime to be formatted</param>
        /// <param name="src_format">Current format of string DateTime</param>
        /// <param name="target_format">Target format of string DateTime</param>
        /// <returns>Returns string DateTime using <paramref name="target_format"/> format.</returns>
        static public string ReFormatToDateTime(string date, string src_format, string target_format)
        {
            try
            {
                return DateTime.ParseExact(date, src_format, CultureInfo.InvariantCulture).ToString(target_format);
            }
            catch { }
            return string.Empty;
        }
        /// <summary>
        /// Parses byte array data to UInt16 structure.
        /// </summary>
        /// <param name="bytes">Byte array data</param>
        /// <param name="start_idx">The starting position within value</param>
        /// <param name="length">Length of value</param>
        /// <returns>Returns ushort type data, converted from <paramref name="bytes"/></returns>
        static public ushort CalculateCrc16_CCITT_CheckSum(byte[] bytes, int start_idx, int length)
        {
            try
            {
                const ushort poly = 4129;
                ushort[] table = new ushort[256];
                ushort initialValue = 0xffff;
                ushort temp, a;
                ushort crc = initialValue;
                for (int i = 0; i < table.Length; ++i)
                {
                    temp = 0;
                    a = (ushort)(i << 8);
                    for (int j = 0; j < 8; ++j)
                    {
                        if (((temp ^ a) & 0x8000) != 0)
                            temp = (ushort)((temp << 1) ^ poly);
                        else
                            temp <<= 1;
                        a <<= 1;
                    }
                    table[i] = temp;
                }
                for (int i = start_idx; i < length; ++i)
                {
                    crc = (ushort)((crc << 8) ^ table[((crc >> 8) ^ (0xff & bytes[i]))]);
                }
                return crc;
            }
            catch { }
            return 0;
        }
        /// <summary>
        /// Parses hex string data to a string with UInt16 structure.
        /// </summary>
        /// <param name="hex_str">Hex string data</param>
        /// <param name="start_idx">The starting position within value</param>
        /// <param name="length">Length of value</param>
        /// <returns>Returns string</returns>
        static public string CalculateCrc16_CCITT_CheckSum(string hex_str, int start_idx, int length)
        {
            try
            {
                byte[] data = SocketUtil.HexStringToByteArray(hex_str, start_idx, length);
                return SocketUtil.CalculateCrc16_CCITT_CheckSum(data, 0, data.Length).ToString("X2");
            }
            catch { }
            return String.Empty;
        } 
    }
}
