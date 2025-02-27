using System.Net;

namespace ZabbixLibrary
{
    public static class SecurityProtocolTypeExtensions
    {
        public const SecurityProtocolType Tls13 = (SecurityProtocolType)SslProtocolsExtensions.Tls13;
        public const SecurityProtocolType SystemDefault = (SecurityProtocolType)SslProtocolsExtensions.SystemDefault;
    }
}
