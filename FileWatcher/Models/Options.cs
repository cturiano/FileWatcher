using System;
using CommandLine;

namespace FileWatcher.Models
{
    internal class Options : IEquatable<Options>
    {
        #region Properties

        [Option('d', "directory", Required = true, HelpText = "The file system directory to watch.  Can be a full, relative or network path.")]
        public string FilePath { get; set; }

        [Option('p', "pattern", Required = true, HelpText = "Files with this file extension will be monitored, e.g.: '*.txt', '*', 'Read*.txt'")]
        public string FilePattern { get; set; }

        [Option('t', "timer", Required = false, HelpText = "The frequency with which to scan the directory for changes.  Defaults to 10 seconds.")]
        public int Timer { get; set; } = 10000;

        #endregion

        #region Public Methods

        public bool Equals(Options other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other) || string.Equals(FilePath, other.FilePath) && string.Equals(FilePattern, other.FilePattern));
        }

        #endregion
    }
}