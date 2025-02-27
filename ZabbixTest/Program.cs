using System.Diagnostics;
using System;
using ZabbixLibrary;
using ZabbixLibrary.Tasks;


ZabbixConfig config = new ZabbixConfig();
config.LoadConfig("ZabbixConfig.json");
ILogger logger = new SerilogLogger();

ZabbixClient client = new(config, logger);
client.Config.HostName = "New Host";
client.Config.ServerAddress = "localhost";
client.Config.Port = 443;
client.Config.UseEncryption = true;
client.Config.PinCertificateBeforeCA = true;
client.Config.ServerCertificatePath = "nginx-selfsigned.crt";
client.Config.TargetHost = "localhost";

ZabbixItemCreation itemCreation = new ZabbixItemCreation(client.Logger);
itemCreation.AddTrapperItems(new RamUsageTask().GetCreateZabbixTrapperItems());
itemCreation.AddTrapperItems(new CpuUsageTask().GetCreateZabbixTrapperItems());
itemCreation.AddTrapperItems(new ThreadUsageTask().GetCreateZabbixTrapperItems());
itemCreation.SaveTrapperItemsToFile("itemsCreation.json");

client.Scheduler.AddTask(new RamUsageTask(), TimeSpan.FromSeconds(5));
client.Scheduler.AddTask(new CpuUsageTask(), TimeSpan.FromSeconds(5));
client.Scheduler.AddTask(new ThreadUsageTask(), TimeSpan.FromSeconds(5));
client.Scheduler.Start();

Console.WriteLine("Press any key to stop...");
Console.ReadKey();