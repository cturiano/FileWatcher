using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Monitor = FileWatcher.Abstract.Monitor;
using Timer = System.Timers.Timer;

namespace FileWatcher.Models
{
    internal class TimerMonitor : Monitor
    {
        #region Fields

        private readonly int _interval;

        #endregion

        #region Constructors

        public TimerMonitor(Options options, Action<string> outputFunction, CancellationToken ct) : base(options, outputFunction, ct)
        {
            _interval = options.Timer;
        }

        #endregion

        #region Public Methods

        public override void BeginMonitor()
        {
            Task.Factory.StartNew(() =>
                                  {
                                      var t = new Timer(_interval) {AutoReset = true};
                                      t.Elapsed += async delegate
                                                   {
                                                       try
                                                       {
                                                           await Timer_Elapsed();
                                                       }
                                                       catch (Exception e)
                                                       {
                                                           OutputFunction.Invoke(e.Message);
                                                       }
                                                       finally
                                                       {
                                                           // if anything happens, restart the timer
                                                           t.Start();
                                                       }
                                                   };

                                      t.Start();
                                  },
                                  CancellationToken,
                                  TaskCreationOptions.LongRunning,
                                  TaskScheduler.Default);
        }

        #endregion

        #region Private Methods

        private Task ProcessFiles()
        {
            return Task.Run(async () =>
                            {
                                var fileInfos = new DirectoryInfo(FilePath).GetFiles(FilePattern);

                                foreach (var fileInfo in fileInfos)
                                {
                                    var newNumLines = await GetLineCount(fileInfo.FullName);
                                    var oldFileInfoPair = FileLineMap.FirstOrDefault(fi => fi.Key.Name == fileInfo.Name);

                                    if (!oldFileInfoPair.Equals(default(KeyValuePair<FileInfo, int>)) && oldFileInfoPair.Key.LastWriteTime != fileInfo.LastWriteTime)
                                    {
                                        // update the # of lines (other FileInfo properties will be refreshed in RemoveDeletedFiles)
                                        if (FileLineMap.TryUpdate(oldFileInfoPair.Key, newNumLines, oldFileInfoPair.Value))
                                        {
                                            OutputFunction.Invoke($"Changed: {fileInfo.Name}, {newNumLines - oldFileInfoPair.Value}");
                                        }
                                    }
                                    // new file
                                    else
                                    {
                                        if (oldFileInfoPair.Key?.Name != fileInfo.Name && FileLineMap.TryAdd(fileInfo, newNumLines))
                                        {
                                            OutputFunction.Invoke($"Created: {fileInfo.Name}, {newNumLines}");
                                        }
                                    }
                                }

                                // no need to wait for this
                                Task.Factory.StartNew(RemoveDeletedFiles, CancellationToken);
                            },
                            CancellationToken);
        }

        private async Task Timer_Elapsed()
        {
            await ProcessFiles();
        }

        #endregion
    }
}