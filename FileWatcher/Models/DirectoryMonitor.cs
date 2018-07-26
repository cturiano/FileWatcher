using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Monitor = FileWatcher.Abstract.Monitor;

namespace FileWatcher.Models
{
    internal class DirectoryMonitor : Monitor
    {
        #region Fields

        private readonly FileSystemWatcher _watcher;

        #endregion

        #region Constructors

        internal DirectoryMonitor(Options options, Action<string> outputFunction, CancellationToken ct) : base(options, outputFunction, ct)
        {
            _watcher = new FileSystemWatcher(FilePath, FilePattern)
                       {
                           NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite
                       };

            _watcher.Changed += FileEventHandler;
            _watcher.Deleted += FileEventHandler;
            _watcher.Error += FileError;
        }

        #endregion

        #region Public Methods

        public override void BeginMonitor()
        {
            Task.Factory.StartNew(() =>
                                  {
                                      _watcher.EnableRaisingEvents = true;
                                      OutputFunction.Invoke("Monitor running.");

                                      while (true)
                                      {
                                          if (CancellationToken.IsCancellationRequested)
                                          {
                                              OutputFunction.Invoke("Monitoring has been cancelled.");

                                              // do any clean up here

                                              CancellationToken.ThrowIfCancellationRequested();
                                          }
                                      }
                                  },
                                  CancellationToken,
                                  TaskCreationOptions.LongRunning,
                                  TaskScheduler.Default);
        }

        #endregion

        #region Private Methods

        private void FileError(object sender, ErrorEventArgs args)
        {
            OutputFunction.Invoke($"File Error: {args.GetException().Message}");
        }

        private async void FileEventHandler(object sender, FileSystemEventArgs args)
        {
            await Task.Factory.StartNew(async () =>
                                        {
                                            var numLines = 0;
                                            var changeType = WatcherChangeTypes.Deleted;

                                            if (args.ChangeType == WatcherChangeTypes.Changed)
                                            {
                                                changeType = WatcherChangeTypes.Created;

                                                var oldNumLines = 0;
                                                var newNumLines = await GetLineCount(args.FullPath);

                                                var kvp = FileLineMap.FirstOrDefault(i => i.Key.Name == args.FullPath);
                                                if (kvp.Key != default(FileInfo))
                                                {
                                                    oldNumLines = kvp.Value;
                                                    changeType = WatcherChangeTypes.Changed;
                                                }

                                                numLines = newNumLines - oldNumLines;
                                                FileLineMap[new FileInfo(args.FullPath)] = newNumLines;
                                            }

                                            var sb = new StringBuilder($"{changeType}: {args.Name}");

                                            if (changeType != WatcherChangeTypes.Deleted)
                                            {
                                                sb.Append($", {numLines}");
                                            }

                                            OutputFunction.Invoke(sb.ToString());
                                        },
                                        CancellationToken);
        }

        #endregion
    }
}