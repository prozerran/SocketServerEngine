
namespace SocketCommon
{
    public static class SocketParserConstants
    {
        public static readonly int MAX_SIZE_READ_PACKET_BUFFER = 4096;
        public static readonly int MAX_SIZE_DATAGRAM = 256;
        public static readonly int READ_PACKET_TIMEOUT_SHORT = 1000 * 60 * 5;       // 5 minutes
        public static readonly int READ_PACKET_TIMEOUT_LONG = 1000 * 60 * 60 * 2;   // 2 hour
    }

    public static class DeviceModels
    {
        public const string ALL = "ALL";
        public const string Device = "Device";
    }
}
