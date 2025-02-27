using System;
using System.Collections.Generic;
using System.Text;

namespace ZabbixLibrary
{
    public interface ILogger
    {
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message, Exception ex = null);
        void Initialize(ZabbixConfig config);
    }

}
