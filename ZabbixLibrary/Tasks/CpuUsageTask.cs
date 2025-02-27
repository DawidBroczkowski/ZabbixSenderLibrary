using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace ZabbixLibrary.Tasks
{
    public class CpuUsageTask : IZabbixTask
    {
        public async Task<List<ZabbixTrapperItem>> Execute()
        {
            var cpuCounter = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName);
            var value = cpuCounter.NextValue();
            await Task.Delay(1000);
            value = cpuCounter.NextValue();

            return new List<ZabbixTrapperItem>()
            {
                new ZabbixTrapperItem()
                {
                    Key = "cpu.usage",
                    Value = value.ToString(CultureInfo.InvariantCulture)
                }
            };
        }

        public List<CreateZabbixTrapperItem> GetCreateZabbixTrapperItems()
        {
            return new List<CreateZabbixTrapperItem>() { new CreateZabbixTrapperItem("CPU usage", "cpu.usage", 3, "0", "%")};
        }
    }
}
