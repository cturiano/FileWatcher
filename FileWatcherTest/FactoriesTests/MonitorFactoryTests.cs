using System.Threading;
using FileWatcher.Factories;
using FileWatcher.Models;
using NUnit.Framework;

namespace FileWatcherTest.FactoriesTests
{
    [TestFixture]
    public class MonitorFactoryTests
    {
        [TestCase(MonitorType.Timer)]
        [TestCase(MonitorType.FileSystemWatcher)]
        public void CreateMonitorTest(MonitorType monitorType)
        {
            var monitor = MonitorFactory.CreateMonitor(monitorType, TestHelper.CreateOptions(), null, new CancellationToken(true));
            switch (monitorType)
            {
                case MonitorType.Timer:
                    Assert.IsInstanceOf<TimerMonitor>(monitor);
                    break;
                case MonitorType.FileSystemWatcher:
                    Assert.IsInstanceOf<DirectoryMonitor>(monitor);
                    break;
            }
        }
    }
}