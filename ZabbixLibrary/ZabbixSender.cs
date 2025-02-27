using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ZabbixLibrary
{
    public class ZabbixSender
    {
        private ZabbixConfig _config;
        private readonly byte[] _header = new byte[] { 0x5A, 0x42, 0x58, 0x44, 0x01 }; // "ZBXD\1"
        private ILogger _logger;

        public ZabbixSender(ZabbixConfig config, ILogger logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<string> SendAsync(string key, string value)
        {
            var data = constructData(_config.HostName, key, value);
            var packet = constructPacket(data);
            string response;
            using (var client = new TcpClient(_config.ServerAddress, _config.Port))
            {
                try
                {
                    if (_config.UseEncryption)
                    {
                        response = await sendEncryptedAsync(client, packet);
                    }
                    else
                    {
                        response = await sendAsync(client, packet);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error sending data to Zabbix server", ex);
                    throw new Exception("Error sending data to Zabbix server", ex);
                }
            }
            if (_config.Debug)
            {
                _logger.LogInformation("Sent: " + key + " = " + value);
                _logger.LogInformation("Received: " + response);
            }
            return response;
        }

        public async Task<string> SendAsync(ZabbixTrapperItem item)
        {
            return await SendAsync(item.Key, item.Value);
        }

        public async Task<string> SendItemsAsync(List<ZabbixTrapperItem> items)
        {
            if (_config.Debug)
            {
                foreach (var item in items)
                {
                    _logger.LogInformation("Sending: " + item.Key + " = " + item.Value);
                }
            }
            var data = constructMultipleData(_config.HostName, items);
            var packet = constructPacket(data);
            string response;
            using (var client = new TcpClient(_config.ServerAddress, _config.Port))
            {
                try
                {
                    if (_config.UseEncryption)
                    {
                        response = await sendEncryptedAsync(client, packet);
                    }
                    else
                    {
                        response = await sendAsync(client, packet);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error sending data to Zabbix server", ex);
                    throw new Exception("Error sending data to Zabbix server", ex);
                }
            }
            if (_config.Debug)
            {
                _logger.LogInformation("Sent multiple items: " + JsonConvert.SerializeObject(items));
                _logger.LogInformation("Received: " + response);
            }
            return response;
        }

        // The following method is invoked by the RemoteCertificateValidationDelegate.
        public static bool ValidateServerCertificate(
                  ZabbixSender zabbixSender,
                  X509Certificate certificate,
                  X509Chain chain,
                  SslPolicyErrors sslPolicyErrors)
        {
            var config = zabbixSender._config;

            // For self-signed certificates
            if (config.PinCertificateBeforeCA)
            {
                if (!string.IsNullOrEmpty(config.ServerCertificatePath))
                {
                    var cert = zabbixSender.loadServerCertificate();
                    if (cert != null && certificate.Equals(cert))
                    {
                        return true;
                    }
                }
                else if (!string.IsNullOrEmpty(config.ServerCertificateThumbprint))
                {
                    string certThumbprint = certificate.GetCertHashString().ToUpperInvariant();
                    string thumbprint = config.ServerCertificateThumbprint.Replace(" ", string.Empty).ToUpperInvariant();
                    if (certThumbprint == thumbprint)
                    {
                        return true;
                    }
                }
            }

            // Check for any SSL policy errors.
            if (!config.PinCertificateAfterCA && sslPolicyErrors == SslPolicyErrors.None)
                return true;

            if (config.PinCertificateAfterCA && sslPolicyErrors == SslPolicyErrors.None)
            {
                string expectedThumbprint = config.ServerCertificateThumbprint;

                // Normalize the thumbprint string.
                expectedThumbprint = expectedThumbprint.Replace(" ", string.Empty).ToUpperInvariant();

                // Retrieve the thumbprint of the certificate presented by the server.
                string actualThumbprint = certificate.GetCertHashString().ToUpperInvariant();

                // Trust the certificate if the thumbprints match.
                return expectedThumbprint == actualThumbprint;
            }
            return false;
        }

        private byte[] constructData(string host, string key, string value)
        {
            var payload = new
            {
                request = "sender data",
                data = new[] { new { host, key, value } }
            };
            var json = JsonConvert.SerializeObject(payload);
            return Encoding.UTF8.GetBytes(json);
        }

        private byte[] constructMultipleData(string host, List<ZabbixTrapperItem> items)
        {
            var dataList = new List<object>();
            foreach (var item in items)
            {
                dataList.Add(new { host, key = item.Key, value = item.Value });
            }
            var payload = new
            {
                request = "sender data",
                data = dataList
            };
            var json = JsonConvert.SerializeObject(payload);
            return Encoding.UTF8.GetBytes(json);
        }

        private byte[] constructPacket(byte[] data)
        {
            var length = BitConverter.GetBytes(data.Length);

            var packet = new byte[_header.Length + 8 + data.Length];
            Buffer.BlockCopy(_header, 0, packet, 0, _header.Length);
            Buffer.BlockCopy(length, 0, packet, _header.Length, 4); // First 4 bytes of length
            Buffer.BlockCopy(new byte[4], 0, packet, _header.Length + 4, 4); // Zero padding
            Buffer.BlockCopy(data, 0, packet, _header.Length + 8, data.Length);

            return packet;
        }

        private string validateResponse(byte[] response, int bytesRead)
        {
            if (bytesRead < 13)
                return null;

            var responseHeader = new byte[5];
            Buffer.BlockCopy(response, 0, responseHeader, 0, 5);

            if (!compareByteArrays(responseHeader, _header))
                return null;
            var responseData = Encoding.UTF8.GetString(response, 13, bytesRead - 13);
            return responseData;
        }

        private bool compareByteArrays(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
                return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                    return false;
            }

            return true;
        }

        private async Task<string> sendEncryptedAsync(TcpClient client, byte[] packet)
        {
            var zabbixSenderInstance = this; // Capture the current instance
            var sslStream = new SslStream(client.GetStream(), false, (sender, certificate, chain, sslPolicyErrors) =>
            {
                // Use the captured instance and its config
                return ValidateServerCertificate(zabbixSenderInstance, certificate, chain, sslPolicyErrors);
            }, null);

            var cert = loadClientCertificate();
            var certificates = new X509CertificateCollection();
            if (cert != null)
            {
                certificates.Add(cert);
            }
            await sslStream.AuthenticateAsClientAsync(_config.TargetHost, certificates, _config.SslProtocols, true);
            await sslStream.WriteAsync(packet, 0, packet.Length);
            var response = new byte[512];
            var bytesRead = await sslStream.ReadAsync(response, 0, response.Length);
            var responseData = validateResponse(response, bytesRead);
            if (responseData == null)
            {
                _logger.LogError("Invalid response from Zabbix server");
                throw new Exception("Invalid response from Zabbix server");
            }
            return responseData;
        }

        private async Task<string> sendAsync(TcpClient client, byte[] packet)
        {
            using (var stream = client.GetStream())
            {
                await stream.WriteAsync(packet, 0, packet.Length);
                var response = new byte[512];
                var bytesRead = await stream.ReadAsync(response, 0, response.Length);
                var responseData = validateResponse(response, bytesRead);
                if (responseData == null)
                {
                    _logger.LogError("Invalid response from Zabbix server");
                    throw new Exception("Invalid response from Zabbix server");
                }
                return responseData;
            }
        }

        private X509Certificate2 loadClientCertificate()
        {
            // Attempt to load the certificate from a file if a path is provided.
            if (!string.IsNullOrEmpty(_config.ClientCertificatePath))
            {
                return new X509Certificate2(_config.ClientCertificatePath, _config.ClientCertificatePassword);
            }
            // If no path is provided, but a thumbprint is, try to load the certificate from the system store.
            else if (!string.IsNullOrEmpty(_config.ClientCertificateThumbprint))
            {
                using (var store = new X509Store(_config.ClientCertificateStoreName, _config.ClientCertificateStoreLocation))
                {
                    store.Open(OpenFlags.ReadOnly);
                    var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, _config.ServerCertificateThumbprint, false);
                    if (certificates.Count > 0)
                    {
                        return certificates[0];
                    }
                }
            }
            return null;
        }

        private X509Certificate2 loadServerCertificate()
        {
            if (!string.IsNullOrEmpty(_config.ServerCertificatePath))
            {
                try
                {
                    return new X509Certificate2(_config.ServerCertificatePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error loading server certificate", ex);
                    return null;
                }
            }
            return null;
        }
    }
}
