using System;
using System.Threading;
using FileWatcher.Interfaces;
using FileWatcher.Models;

namespace FileWatcher.Factories
{
    internal static class MonitorFactory
    {
        #region Internal Methods

        internal static IMonitor CreateMonitor(MonitorType type, Options options, Action<string> outputFunction, CancellationToken token)
        {
            IMonitor monitor;
            switch (type)
            {
                case MonitorType.Timer:
                    monitor = new TimerMonitor(options, outputFunction, token);
                    break;
                case MonitorType.FileSystemWatcher:
                    monitor = new DirectoryMonitor(options, outputFunction, token);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return monitor;
        }

        #endregion
    }
}