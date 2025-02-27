using System;
using System.Collections.Generic;
using System.Text;

namespace ZabbixLibrary
{
    public class CreateZabbixTrapperItem
    {
        public string name { get; set; }
        public string key { get; set; }
        public int value_type { get; set; }
        public string delay { get; set; }
        public string units { get; set; }

        public CreateZabbixTrapperItem(string name, string key, int value_type, string delay, string units)
        {
            this.name = name;
            this.key = key;
            this.value_type = value_type;
            this.delay = delay;
            this.units = units;
        }
    }
}
