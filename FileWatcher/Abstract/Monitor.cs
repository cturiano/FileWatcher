using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileWatcher.Interfaces;
using FileWatcher.Models;

namespace FileWatcher.Abstract
{
    internal abstract class Monitor : IMonitor
    {
        #region Constructors

        protected Monitor(Options options, Action<string> outputFunction, CancellationToken ct)
        {
            FilePath = options.FilePath;
            FilePattern = options.FilePattern;
            OutputFunction = outputFunction;
            CancellationToken = ct;
            FileLineMap = new ConcurrentDictionary<FileInfo, int>();
        }

        #endregion

        #region Properties

        public CancellationToken CancellationToken { get; }

        public ConcurrentDictionary<FileInfo, int> FileLineMap { get; }

        public string FilePath { get; }

        public string FilePattern { get; }

        public Action<string> OutputFunction { get; }

        #endregion

        #region Public Methods

        public abstract void BeginMonitor();

        #endregion

        #region Protected Methods

        protected async Task<int> GetLineCount(string fileName)
        {
            return (await GetLines(fileName)).Count;
        }

        protected async Task<List<string>> GetLines(string fileName)
        {
            return await Task.Factory.StartNew(() =>
                                               {
                                                   // spin here until we have access to the file
                                                   SpinWait.SpinUntil(() =>
                                                                      {
                                                                          try
                                                                          {
                                                                              using (var fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                                                                              {
                                                                                  return fs.Length > 0;
                                                                              }
                                                                          }
                                                                          catch (Exception)
                                                                          {
                                                                              return false;
                                                                          }
                                                                      });

                                                   return File.ReadLines(fileName).ToList();
                                               },
                                               CancellationToken);
        }

        protected void RemoveDeletedFiles()
        {
            Parallel.ForEach(FileLineMap,
                             async kvp =>
                             {
                                 // new to wait on refresh to determine if the file still exists
                                 await Task.Run(() => kvp.Key.Refresh(), CancellationToken);

                                 if (!kvp.Key.Exists && FileLineMap.TryRemove(kvp.Key, out _))
                                 {
                                     OutputFunction.Invoke($"Deleted: {kvp.Key.Name}");
                                 }
                             });
        }

        #endregion
    }
}