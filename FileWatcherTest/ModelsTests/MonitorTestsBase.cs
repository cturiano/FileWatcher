using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using FileWatcher.Interfaces;
using NUnit.Framework;

namespace FileWatcherTest.ModelsTests
{
    public class MonitorTestsBase
    {
        #region Properties

        protected List<string> AddedFiles { get; private set; }

        protected IMonitor Monitor { get; set; }

        protected string PathToEventOutputFile { get; private set; }

        protected string TestOutputDirectory { get; private set; }

        #endregion

        #region Public Methods

        [TearDown]
        public void MonitorTestCleanUp()
        {
            try
            {
                if (File.Exists(PathToEventOutputFile))
                {
                    SpinWait.SpinUntil(() =>
                                       {
                                           try
                                           {
                                               using (var fs = File.Open(PathToEventOutputFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                                               {
                                                   fs.Close();
                                                   Directory.Delete(TestOutputDirectory, true);
                                                   return true;
                                               }
                                           }
                                           catch (Exception)
                                           {
                                               return false;
                                           }
                                       });
                }
            }
            catch (Exception e)
            {
                // cleaning up so just bury all exceptions
            }
        }

        public void MonitorTestSetUp()
        {
            // create file to receive FileSystemWatcher event output
            TestOutputDirectory = Path.Combine(Path.GetTempPath() + Path.GetRandomFileName());
            Directory.CreateDirectory(TestOutputDirectory);

            PathToEventOutputFile = Path.Combine(TestOutputDirectory, "BeginMonitorTestOutput.log");
            using (File.Create(PathToEventOutputFile))
            {
            }

            AddedFiles = new List<string> {PathToEventOutputFile};
        }

        #endregion

        #region Internal Methods

        internal void AddAFile(int numLines)
        {
            string filePath;
            do
            {
                filePath = Path.Combine(TestOutputDirectory, Path.GetRandomFileName() + ".txt");
            } while (File.Exists(filePath));

            var s = string.Join("\n", Enumerable.Range(0, numLines));
            using (var sw = new StreamWriter(filePath))
            {
                sw.WriteLine(s);
            }

            AddedFiles.Add(filePath);
        }

        internal void AppendAllText(string fileName, string textToWrite)
        {
            SpinWait.SpinUntil(() =>
                               {
                                   try
                                   {
                                       using (var fs = File.Open(fileName, FileMode.Open, FileAccess.Write, FileShare.None))
                                       {
                                           fs.Close();
                                           File.AppendAllText(fileName, textToWrite);
                                           return true;
                                       }
                                   }
                                   catch (Exception)
                                   {
                                       return false;
                                   }
                               });
        }

        internal List<string> ReadLines(string fileName)
        {
            List<string> lines = null;
            SpinWait.SpinUntil(() =>
                               {
                                   try
                                   {
                                       using (var fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                                       {
                                           fs.Close();
                                           lines = File.ReadLines(fileName).ToList();
                                           return true;
                                       }
                                   }
                                   catch (Exception)
                                   {
                                       return false;
                                   }
                               });

            return lines;
        }

        internal void WriteAllText(string fileName, string textToWrite)
        {
            SpinWait.SpinUntil(() =>
                               {
                                   try
                                   {
                                       using (var fs = File.Open(fileName, FileMode.Open, FileAccess.Write, FileShare.None))
                                       {
                                           fs.Close();
                                           File.WriteAllText(fileName, textToWrite);
                                           return true;
                                       }
                                   }
                                   catch (Exception)
                                   {
                                       return false;
                                   }
                               });
        }

        internal void WriteToFile(string textToWrite)
        {
            AppendAllText(PathToEventOutputFile, textToWrite + Environment.NewLine);
        }

        #endregion
    }
}