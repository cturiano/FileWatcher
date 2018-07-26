using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace FileWatcher.Interfaces
{
    public interface IMonitor
    {
        #region Properties

        CancellationToken CancellationToken { get; }

        ConcurrentDictionary<FileInfo, int> FileLineMap { get; }

        string FilePath { get; }

        string FilePattern { get; }

        Action<string> OutputFunction { get; }

        #endregion

        #region Public Methods

        void BeginMonitor();

        #endregion
    }
}