using Newtonsoft.Json;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace ZabbixLibrary
{
    public class ZabbixConfig
    {
        /// <summary>
        /// Address of the Zabbix server. Default is "localhost".
        /// </summary>
        public string ServerAddress { get; set; }
        /// <summary>
        /// Port of the Zabbix server. Default is 10051.
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// Name of the Zabbix host.
        /// </summary>
        public string HostName { get; set; }
        /// <summary>
        /// If true, the sender will use encryption. Default is false.
        /// </summary>
        public bool UseEncryption { get; set; }
        /// <summary>
        /// Target host for an ssl certificate.
        /// </summary>
        public string TargetHost { get; set; }
        /// <summary>
        /// Thumbprint of the server ssl certificate. Only applicable if <see cref="UseEncryption"/> is true
        /// and when using a self signed certificate.
        /// </summary> 
        public string ServerCertificateThumbprint { get; set; }
        /// <summary>
        /// Path to the server certificate file. Only applicable if <see cref="UseEncryption"/> is true.
        /// This is used if server certificate validation requires pinning.
        /// </summary>
        public string ServerCertificatePath { get; set; }
        /// <summary>
        /// Thumbprint of the client ssl certificate. Only applicable if <see cref="UseEncryption"/> is true.
        /// </summary>
        public string ClientCertificateThumbprint { get; set; }
        /// <summary>
        /// Name of the store where the client certificate is located. Only applicable if <see cref="UseEncryption"/> is true
        /// and <see cref="ClientCertificateThumbprint"/> is provided;
        /// </summary>
        public StoreName ClientCertificateStoreName { get; set; }
        /// <summary>
        /// Location of the store where the client certificate is located. Only applicable if <see cref="UseEncryption"/> is true
        /// and <see cref="ClientCertificateThumbprint"/> is provided;
        /// </summary>
        public StoreLocation ClientCertificateStoreLocation { get; set; }
        /// <summary>
        /// Path to the ssl client certificate. Only applicable if <see cref="UseEncryption"/> is true.
        /// This takes precedence over <see cref="ClientCertificateThumbprint"/>. 
        /// </summary>
        public string ClientCertificatePath { get; set; }
        /// <summary>
        /// Password for the ssl client certificate. Only applicable if <see cref="UseEncryption"/> is true.
        /// </summary>
        public string ClientCertificatePassword { get; set; }
        /// <summary>
        /// SSL/TLS protocols to be used for encrypted connections. Only applicable if <see cref="UseEncryption"/> is true.
        /// By default, when creating a <see cref="ZabbixConfig"/> instance, this is actively set to <see cref="SslProtocols.None"/>.
        /// This allows the system to choose the most secure protocol available.
        /// Only change this if you want to enforce a specific protocol. TLS 1.3 is available as <see cref="SslProtocolsExtensions.Tls13"/>.
        /// </summary>
        public SslProtocols SslProtocols { get; set; }
        /// <summary>
        /// Matching the server certificate thumbprint or file before the CA check.
        /// Used for self signed certificates. If the self signed certificate is not pinned, the CA check commence anyway.
        /// </summary>
        public bool PinCertificateBeforeCA { get; set; }
        /// <summary>
        /// Matching the server certificate thumbprint or file after the CA check.
        /// Used for pinning trusted certificates.
        /// </summary>
        public bool PinCertificateAfterCA { get; set; }
        /// <summary>
        /// If true, the sender will output debug information with INFO level. Default is false.
        /// </summary>
        public bool Debug { get; set; }
        /// <summary>
        /// Path to the log file. Default is empty.
        /// </summary>
        public string LogFilePath { get; set; }
        public ZabbixConfig()
        {
            ServerAddress = "localhost";
            Port = 10051;
            HostName = "";
            TargetHost = "localhost";
            UseEncryption = false;
            ServerCertificateThumbprint = string.Empty;
            SslProtocols = SslProtocols.None;
            Debug = false;
        }

        public ZabbixConfig(string serverAddress, int port, string hostName)
        {
            ServerAddress = serverAddress;
            Port = port;
            HostName = hostName;
            TargetHost = "localhost";
            UseEncryption = false;
            ServerCertificateThumbprint = string.Empty;
            SslProtocols = SslProtocols.None;
        }

        public void SaveConfig(string filePath)
        {
            var json = JsonConvert.SerializeObject(this);
            System.IO.File.WriteAllText(filePath, json);
        }

        public void LoadConfig(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                var json = System.IO.File.ReadAllText(filePath);
                var config = JsonConvert.DeserializeObject<ZabbixConfig>(json);
                ServerAddress = config.ServerAddress;
                Port = config.Port;
                HostName = config.HostName;
                TargetHost = config.TargetHost;
                UseEncryption = config.UseEncryption;
                ServerCertificateThumbprint = config.ServerCertificateThumbprint;
                ServerCertificatePath = config.ServerCertificatePath;
                ClientCertificateThumbprint = config.ClientCertificateThumbprint;
                ClientCertificateStoreName = config.ClientCertificateStoreName;
                ClientCertificateStoreLocation = config.ClientCertificateStoreLocation;
                ClientCertificatePath = config.ClientCertificatePath;
                ClientCertificatePassword = config.ClientCertificatePassword;
                SslProtocols = config.SslProtocols;
                PinCertificateBeforeCA = config.PinCertificateBeforeCA;
                PinCertificateAfterCA = config.PinCertificateAfterCA;
                Debug = config.Debug;
                LogFilePath = config.LogFilePath;
            }
            else
            {
                SaveConfig(filePath);
            }
        }

    }
}
