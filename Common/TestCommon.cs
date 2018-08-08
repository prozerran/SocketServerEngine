
using System.Text;
using NUnit.Framework;

namespace SocketCommon
{
    /// <summary>
    /// Handles testing.
    /// </summary>
    [TestFixture]
    public class TestCommon
    {
        // declare any objects here

        [TestFixtureSetUp]
        public void Init()          // executes once
        { }

        [TestFixtureTearDown]
        public void Dispose()       // executes once
        { }

        [SetUp]
        public void Setup()         // executes for each test case        
        { }

        [TearDown]
        public void TearDown()      // executes for each test case
        { }

        [Test]
        public void ByteArrayToHexString()
        {
            byte[] data = new byte[] { 0x7e, 0x01, 0x00, 0x64 };

            string hexString = SocketUtil.ByteArrayToHexString(data, 0, data.Length, false);
            Assert.AreEqual("7E010064", hexString);

            hexString = SocketUtil.ByteArrayToHexString(data);
            Assert.AreEqual("7E010064", hexString);

            hexString = SocketUtil.ByteArrayToHexString(data, 0, data.Length, true);
            Assert.AreEqual("7E-01-00-64", hexString);
        }

        [Test]
        public void HexStringToByteArray()
        {
            byte[] bArray = SocketUtil.HexStringToByteArray("02a3");
            Assert.AreEqual(new byte[]{0x02, 0xa3}, bArray);
        }

        [Test]
        public void CalculateXorCheckSum_Bytes()
        {
            string hexString = "010000220130005907880144002c012f373031313142534a2d41362f463030303030303001d4c142383838383800";
            byte[] data = SocketUtil.HexStringToByteArray(hexString);
            byte chksum = SocketUtil.CalculateXorCheckSum(data, 0, data.Length);
            Assert.AreEqual(0x83, chksum);
        }

        [Test]
        public void CalculateCrcCheckSum_Bytes()
        {
            string hexString = "24240075590099FFFFFFFF99553037353534302E3030302C412C323231382E313534322C4E2C31313431352E373139322C452C3030302E302C3033342E332C3033303231362C2C2C412A36397C312E327C32322E317C343830307C303030302C303030307C3031353234383331387C303995580D0A";
            byte[] data = SocketUtil.HexStringToByteArray(hexString);
            string chksum = SocketUtil.CalculateCrc16_CCITT_CheckSum(data, 0, data.Length - 4).ToString("x2").ToUpper();
            Assert.AreEqual("9558", chksum);
        }

        [Test]
        public void CalculateCrcCheckSum_String()
        {
            string hexString = "24240072590865FFFFFFFF99553033353830352E30302C412C323233312E39333332322C4E2C31313430392E36333338392C452C302E3035312C2C3032303231362C2C2C442A37437C302E36387C33342E337C323830307C306561302C303030307C3030303030303030307C3132F9770D0A";
            string chksum = SocketUtil.CalculateCrc16_CCITT_CheckSum(hexString, 0, hexString.Length - 8);
            Assert.AreEqual("F977", chksum);
        }

        [Test]
        public void CalculateXorCheckSum_String()
        {
            string str = "C100367,M000008,O0002,I6302,D083308,A,N2220.2887,E11411.9350,T0.00,H68.67,Y100609,G06";
            string chksum = SocketUtil.CalculateXorCheckSum(str, 0, str.Length);
            Assert.AreEqual("2D", chksum);
        }

        [Test]
        public void AnalyzeHexCharAsBit()
        {
            // In 4-bit binary representation...
            //
            // 0001 = CHECK_BIT_1   LSB : least significant bit
            // 0010 = CHECK_BIT_2
            // 0100 = CHECK_BIT_3
            // 1000 = CHECK_BIT_4   MSB : most significant bit

            // 0 = 0000
            Assert.AreEqual(0, SocketUtil.AnalyzeHexCharAsBit('0', SocketUtil.CHECK_BIT_1));
            Assert.AreEqual(0, SocketUtil.AnalyzeHexCharAsBit('0', SocketUtil.CHECK_BIT_2));
            Assert.AreEqual(0, SocketUtil.AnalyzeHexCharAsBit('0', SocketUtil.CHECK_BIT_3));
            Assert.AreEqual(0, SocketUtil.AnalyzeHexCharAsBit('0', SocketUtil.CHECK_BIT_4));

            // 5 = 0101
            Assert.AreEqual(1, SocketUtil.AnalyzeHexCharAsBit('5', SocketUtil.CHECK_BIT_1));
            Assert.AreEqual(0, SocketUtil.AnalyzeHexCharAsBit('5', SocketUtil.CHECK_BIT_2));
            Assert.AreEqual(1, SocketUtil.AnalyzeHexCharAsBit('5', SocketUtil.CHECK_BIT_3));
            Assert.AreEqual(0, SocketUtil.AnalyzeHexCharAsBit('5', SocketUtil.CHECK_BIT_4));

            // A = 1010
            Assert.AreEqual(0, SocketUtil.AnalyzeHexCharAsBit('A', SocketUtil.CHECK_BIT_1));
            Assert.AreEqual(1, SocketUtil.AnalyzeHexCharAsBit('A', SocketUtil.CHECK_BIT_2));
            Assert.AreEqual(0, SocketUtil.AnalyzeHexCharAsBit('A', SocketUtil.CHECK_BIT_3));
            Assert.AreEqual(1, SocketUtil.AnalyzeHexCharAsBit('A', SocketUtil.CHECK_BIT_4));

            // F = 1111
            Assert.AreEqual(1, SocketUtil.AnalyzeHexCharAsBit('F', SocketUtil.CHECK_BIT_1));
            Assert.AreEqual(1, SocketUtil.AnalyzeHexCharAsBit('F', SocketUtil.CHECK_BIT_2));
            Assert.AreEqual(1, SocketUtil.AnalyzeHexCharAsBit('F', SocketUtil.CHECK_BIT_3));
            Assert.AreEqual(1, SocketUtil.AnalyzeHexCharAsBit('F', SocketUtil.CHECK_BIT_4));
        }

        [Test]
        public void GetShortHexValue()
        {
            Assert.AreEqual(0, SocketUtil.GetShortHexValue('0'));
            Assert.AreEqual(1, SocketUtil.GetShortHexValue('1'));
            Assert.AreEqual(2, SocketUtil.GetShortHexValue('2'));
            Assert.AreEqual(3, SocketUtil.GetShortHexValue('3'));
            Assert.AreEqual(4, SocketUtil.GetShortHexValue('4'));
            Assert.AreEqual(5, SocketUtil.GetShortHexValue('5'));
            Assert.AreEqual(6, SocketUtil.GetShortHexValue('6'));
            Assert.AreEqual(7, SocketUtil.GetShortHexValue('7'));
            Assert.AreEqual(8, SocketUtil.GetShortHexValue('8'));
            Assert.AreEqual(9, SocketUtil.GetShortHexValue('9'));
            Assert.AreEqual(10, SocketUtil.GetShortHexValue('A'));
            Assert.AreEqual(11, SocketUtil.GetShortHexValue('B'));
            Assert.AreEqual(12, SocketUtil.GetShortHexValue('C'));
            Assert.AreEqual(13, SocketUtil.GetShortHexValue('D'));
            Assert.AreEqual(14, SocketUtil.GetShortHexValue('E'));
            Assert.AreEqual(15, SocketUtil.GetShortHexValue('F'));
        }

        [Test]
        public void SubBytesAsString()
        {
            byte[] data = new byte[] { 0x7e, 0x01, 0x00, 0x64 };
            string ss = data.SubBytesAsString(0, 2);
            byte[] bArray = Encoding.UTF8.GetBytes(ss);
            Assert.AreEqual(new byte[] { 0x7e, 0x01 }, bArray);
        }

        [Test]
        public void ReFormatToDateTime()
        {
            string result = SocketUtil.ReFormatToDateTime("100116", "ddMMyy", "yyMMdd");
            Assert.AreEqual("160110", result);

            result = SocketUtil.ReFormatToDateTime("100116", "ddMMyy", "MMddyy");
            Assert.AreEqual("011016", result);

            result = SocketUtil.ReFormatToDateTime("100116", "sshhmm", "hhmmss");
            Assert.AreEqual("011610", result);
        }
    }
}
