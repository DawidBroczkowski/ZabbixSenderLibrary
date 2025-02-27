using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ZabbixLibrary.Tasks
{
    public class RamUsageTask : IZabbixTask
    {
        public Task<List<ZabbixTrapperItem>> Execute()
        {
            double memory = 0.0;
            using (Process proc = Process.GetCurrentProcess())
            {
                memory = proc.PrivateMemorySize64 / (1024 * 1024);
            }

            return Task.FromResult(new List<ZabbixTrapperItem>()
            {
                new ZabbixTrapperItem()
                {
                    Key = "ram.usage",
                    Value = memory.ToString()
                }
            });
        }

        public List<CreateZabbixTrapperItem> GetCreateZabbixTrapperItems()
        {
            return new List<CreateZabbixTrapperItem>() { new CreateZabbixTrapperItem("RAM usage", "ram.usage", 3, "0", "%") };
        }
    }
}
