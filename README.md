# Zabbix Sender Library
This library is a simple implementation of the Zabbix sender protocol in .NET Standard 2.0. It allows the user to send trapper items to the Zabbix server with optional TLS encryption. It also contains a task scheduler implementation, which allows periodical sending of data.

## Sender usage
First create a ZabbixConfig object. The config properties are explained in summaries in the ZabbixConfig.cs file.
```csharp
ZabbixConfig config = new ZabbixConfig();
config.HostName = "New Host";
config.ServerAddress = "localhost";
config.Port = 443;
config.UseEncryption = true;
config.PinCertificateBeforeCA = true;
config.ServerCertificatePath = "nginx-selfsigned.crt";
config.TargetHost = "localhost";
```

Create a ZabbixSender object, pass in the config and an implementation of ILogger. Serilog is included in the library.
```csharp
ILogger logger = new SerilogLogger();
ZabbixSender sender = new ZabbixSender(config, logger);
```

Use one of the methods to either send a single key + value pair or a list of ZabbixTrapperItem.
```csharp
string response = await sender.SendAsync("Key", "Value").

// OR

ZabbixTrapperItem item = new ZabbixTrapperItem() { Key = "a", Value = "1" };
string response = await sender.SendAsync(item);

// OR

List<ZabbixTrapperItem> trapperItems = new List<ZabbixTrapperItem>();
trapperItems.Add(new ZabbixTrapperItem() { Key = "b", Value = "2" });
trapperItems.Add(new ZabbixTrapperItem() { Key = "c", Value = "3" });
string response = await sender.SendItemsAsync(trapperItems);
```
## Scheduler usage
The scheduler allows for a periodical execution of tasks which retrieve data to send to the server.
### Task
To use the ZabbixScheduler you need to create a class implementing the IZabbixTask interface.
```csharp
public interface IZabbixTask
{
    Task<List<ZabbixTrapperItem>> Execute();
    List<CreateZabbixTrapperItem> GetCreateZabbixTrapperItems();
}
```
The execution of this task has to return a list of ZabbixTrapperItem objects that the scheduler will send to the server. 


Implementation of the second method in the interface is optional. It is used to store and return the trapper items as they are stored in the server - it can be used to retrieve and serialize the data needed to create the trapper items in the server using a script. For example:
```csharp
public List<CreateZabbixTrapperItem> GetCreateZabbixTrapperItems()
{
    return new List<CreateZabbixTrapperItem>() { new CreateZabbixTrapperItem("RAM usage", "ram.usage", 3, "0", "%") };
}
```
There are 3 sample tasks included in the library - CpuUsageTask, RamUsageTask and ThreadUsageTask. This functionality can be used as follows:
```csharp
ZabbixItemCreation itemCreation = new ZabbixItemCreation(logger);
itemCreation.AddTrapperItems(new RamUsageTask().GetCreateZabbixTrapperItems());
itemCreation.AddTrapperItems(new CpuUsageTask().GetCreateZabbixTrapperItems());
itemCreation.AddTrapperItems(new ThreadUsageTask().GetCreateZabbixTrapperItems());
itemCreation.SaveTrapperItemsToFile("itemsCreation.json");
```
### Scheduler
To use the scheduler, first create a ZabbixClient object. A new instance of the sender and scheduler will be created inside.
```csharp
ZabbixClient client = new(config, logger);

client.Scheduler.AddTask(new RamUsageTask(), TimeSpan.FromSeconds(5));
client.Scheduler.AddTask(new CpuUsageTask(), TimeSpan.FromSeconds(5));
client.Scheduler.AddTask(new ThreadUsageTask(), TimeSpan.FromSeconds(5));
client.Scheduler.Start();

// To stop the scheduler
client.Scheduler.Stop();
```
