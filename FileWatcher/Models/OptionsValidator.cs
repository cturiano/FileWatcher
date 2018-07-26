using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Security;
using FileWatcher.Properties;

namespace FileWatcher.Models
{
    /// <summary>
    ///     Class to validate options
    ///     It's a Lazy Singleton just for fun.  Lazy is not necessary because instantiation isn't very complicated.
    /// </summary>
    internal sealed class OptionsValidator
    {
        #region Static Fields and Constants

        private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();
        private static readonly Lazy<OptionsValidator> Lazy = new Lazy<OptionsValidator>(() => new OptionsValidator());

        #endregion

        #region Constructors

        private OptionsValidator()
        {
            Errors = new ConcurrentBag<string>();
        }

        #endregion

        #region Properties

        public ConcurrentBag<string> Errors { get; }

        public static OptionsValidator Instance => Lazy.Value;

        #endregion

        #region Public Methods

        public void ClearErrors()
        {
            while (!Errors.IsEmpty)
            {
                Errors.TryTake(out _);
            }
        }

        public bool Validate(Options options)
        {
            return CheckDirectory(options.FilePath) && CheckExtension(options.FilePattern) && CheckTimer(options.Timer);
        }

        #endregion

        #region Private Methods

        private bool CheckDirectory(string directory)
        {
            try
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                Path.GetFullPath(directory);
            }
            catch (ArgumentNullException)
            {
                Errors.Add(Resources.DirectoryNull);
            }
            catch (ArgumentException)
            {
                Errors.Add(string.Format(Resources.DirectoryZeroInvalid, directory));
            }
            catch (SecurityException)
            {
                Errors.Add(string.Format(Resources.DirectoryPermissions, directory));
            }
            catch (NotSupportedException)
            {
                Errors.Add(string.Format(Resources.DirectoryColon, directory));
            }
            catch (PathTooLongException)
            {
                Errors.Add(string.Format(Resources.DirectoryTooLong, directory));
            }

            if (!Directory.Exists(directory))
            {
                Errors.Add(string.Format(Resources.DirectoryNotExist, directory));
            }

            return Errors.Count == 0;
        }

        private bool CheckExtension(string filePattern)
        {
            if (string.IsNullOrEmpty(filePattern) || string.IsNullOrWhiteSpace(filePattern))
            {
                Errors.Add(string.Format(Resources.FilePatternNullWSEmpty, filePattern));
            }
            else
            {
                if (InvalidPathChars.Any(filePattern.Contains))
                {
                    Errors.Add(string.Format(Resources.FilePatternInvalidChars, filePattern));
                }
            }

            return Errors.Count == 0;
        }

        private bool CheckTimer(int timer)
        {
            if (timer <= 0)
            {
                Errors.Add(string.Format(Resources.TimerLessThanZero, timer));
            }

            if (timer > 1000000)
            {
                Errors.Add(string.Format(Resources.TimerTooLarge, timer));
            }

            return Errors.Count == 0;
        }

        #endregion
    }
}