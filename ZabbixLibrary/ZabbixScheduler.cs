using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System;
using ZabbixLibrary.Tasks;
using ZabbixLibrary;
using System.Linq;

namespace ZabbixLibrary
{
    public class ZabbixScheduler
    {
        private readonly ZabbixSender _sender;
        private readonly Dictionary<IZabbixTask, Timer> _taskTimers = new Dictionary<IZabbixTask, Timer>();
        private readonly Dictionary<IZabbixTask, TimeSpan> _tasks = new Dictionary<IZabbixTask, TimeSpan>();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ILogger _logger;

        public ZabbixScheduler(ZabbixSender sender, ILogger logger)
        {
            _sender = sender;
            _logger = logger;
        }

        public void AddTask(IZabbixTask task, TimeSpan interval)
        {
            _tasks[task] = interval;
        }

        public void RemoveTask(IZabbixTask task)
        {
            _tasks.Remove(task);
        }

        public void Start()
        {
            foreach (var kvp in _tasks)
            {
                var task = kvp.Key;
                var interval = kvp.Value;
                var timer = new Timer(async _ =>
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }

                    List<ZabbixTrapperItem> items = await task.Execute();
                    if (items != null)
                    {
                        try
                        {
                            await _sender.SendItemsAsync(items);
                        }
                        catch (Exception ex)
                        {
                            foreach (var item in items)
                            {
                                _logger.LogError($"Error sending item (" + item.Key + " = " + item.Value +": " + ex.Message);
                            }
                        }
                    }
                }, null, TimeSpan.Zero, interval);

                _taskTimers[task] = timer;
            }
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();

            foreach (var timer in _taskTimers.Values)
            {
                timer.Dispose();
            }

            _taskTimers.Clear();
        }
    }
}