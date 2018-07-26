using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using FileWatcher.Models;

namespace FileWatcherTest
{
    internal static class TestHelper
    {
        #region Internal Methods

        internal static DirectoryMonitor CreateDirectoryMonitor(CancellationToken ct, Options options = null, Action<string> outputFunction = null)
        {
            if (options == null)
            {
                options = CreateOptions(Path.GetTempPath());
            }

            if (outputFunction == null)
            {
                outputFunction = s => Debug.WriteLine(s);
            }

            return new DirectoryMonitor(options, outputFunction, ct);
        }

        internal static Options CreateOptions(string directory = null, string pattern = null, int interval = 1000)
        {
            return new Options {FilePath = directory ?? "C:\\Windows", FilePattern = pattern ?? "*.txt", Timer = interval};
        }

        internal static Options CreateOptions(Options copyMe)
        {
            return new Options {FilePath = copyMe.FilePath, FilePattern = copyMe.FilePattern, Timer = copyMe.Timer};
        }

        #endregion
    }
}