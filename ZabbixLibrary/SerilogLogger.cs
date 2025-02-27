using System;
using System.Collections.Generic;
using System.Text;
using Serilog;


namespace ZabbixLibrary
{
    public class SerilogLogger : ILogger
    {
        private Serilog.Core.Logger _logger;
        private ZabbixConfig _config;

        public SerilogLogger()
        {
            
        }

        public void LogInformation(string message)
        {
            _logger.Information(message);
        
        }

        public void LogWarning(string message)
        {
            _logger.Warning(message);
        }

        public void LogError(string message, Exception ex = null)
        {
            if (ex == null)
            {
                _logger.Error(message);
            }
            else
            {
                _logger.Error(ex, message);
            }
        }

        public void Initialize(ZabbixConfig config)
        {
            _config = config;

            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.Debug()
                .WriteTo.File(_config.LogFilePath, rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 1073741824) // 1GB
                .CreateLogger();
        }
    }

}
