using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;

namespace ZabbixLibrary
{
    public class ZabbixItemCreation
    {
        private List<CreateZabbixTrapperItem> _items = new List<CreateZabbixTrapperItem>();
        private ILogger _logger;
        public ZabbixItemCreation(ILogger logger) 
        {
            _logger = logger;
        }

        public void AddTrapperItem(CreateZabbixTrapperItem item)
        {
            _items.Add(item);
        }

        public void AddTrapperItems(List<CreateZabbixTrapperItem> items)
        {
            this._items.AddRange(items);
        }

        public void SaveTrapperItemsToFile(string filePath)
        {
            try
            {
                string json = JsonConvert.SerializeObject(_items, Formatting.Indented);
                File.WriteAllText(filePath, json);
                _logger.LogInformation("Items saved to file: " + filePath);
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to save items to file: " + filePath, e);
            }
        }
    }
}
