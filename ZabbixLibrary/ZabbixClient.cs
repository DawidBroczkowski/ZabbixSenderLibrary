namespace ZabbixLibrary
{
    public class ZabbixClient
    {
        public ZabbixConfig Config { get; } = new ZabbixConfig();
        public ZabbixSender Sender { get; }
        public ZabbixScheduler Scheduler { get; }
        public ILogger Logger { get; set; }

        public ZabbixClient(ZabbixConfig config, ILogger logger)
        {
            Config = config;
            Logger = logger;
            Logger.Initialize(Config);
            Sender = new ZabbixSender(Config, Logger);
            Scheduler = new ZabbixScheduler(Sender, Logger);
        }

    }
}
