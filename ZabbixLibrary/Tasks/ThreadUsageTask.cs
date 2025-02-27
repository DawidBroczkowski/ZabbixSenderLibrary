using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ZabbixLibrary.Tasks
{
    public class ThreadUsageTask : IZabbixTask
    {
        public Task<List<ZabbixTrapperItem>> Execute()
        {
            using (Process proc = Process.GetCurrentProcess())
            {
                return Task.FromResult(new List<ZabbixTrapperItem>()
                {
                    new ZabbixTrapperItem
                    {
                        Key = "thread.count",
                        Value = proc.Threads.Count.ToString()
                    } 
                });
            }
        }

        public List<CreateZabbixTrapperItem> GetCreateZabbixTrapperItems()
        {
            return new List<CreateZabbixTrapperItem>() { new CreateZabbixTrapperItem("Thread count", "thread.count", 3, "0", "") };
        }
    }
}
