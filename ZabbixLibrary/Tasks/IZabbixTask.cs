using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZabbixLibrary.Tasks
{
    public interface IZabbixTask
    {
        Task<List<ZabbixTrapperItem>> Execute();
        List<CreateZabbixTrapperItem> GetCreateZabbixTrapperItems();
    }
}
