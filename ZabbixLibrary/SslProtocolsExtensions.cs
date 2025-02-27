using System.Security.Authentication;

namespace ZabbixLibrary
{
    public static class SslProtocolsExtensions
    {
        public const SslProtocols Tls13 = (SslProtocols)12288;
        public const SslProtocols SystemDefault = (SslProtocols)0;
    }
}
