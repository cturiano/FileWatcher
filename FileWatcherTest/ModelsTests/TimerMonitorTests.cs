using System.Collections.Generic;
using System.IO;
using System.Threading;
using FileWatcher.Factories;
using NUnit.Framework;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace FileWatcherTest.ModelsTests
{
    [TestFixture]
    internal class TimerMonitorTests : MonitorTestsBase
    {
        private static void CheckResults(List<string> lines, string type, string fileName, int count)
        {
            Assert.IsFalse(count != 0 ? string.IsNullOrEmpty(lines.Find(s => s.Equals($"{type}: {Path.GetFileName(fileName)}, {count}"))) : string.IsNullOrEmpty(lines.Find(s => s.Equals($"{type}: {Path.GetFileName(fileName)}"))));
        }

        private static void CheckLine(string line, string type, string fileName, int count)
        {
            if (line == null || type == null || fileName == null)
            {
                Assert.Fail();
            }

            Assert.IsTrue(line.Contains(type));
            Assert.IsTrue(line.Contains(Path.GetFileName(fileName)));
            if (count != 0)
            {
                Assert.IsTrue(line.Contains(count.ToString()));
            }
        }

        [TestCase(10, 1500)]
        [TestCase(1000000, 2000)]
        public void BeginMonitorBasicTest(int numLines, int interval)
        {
            MonitorTestSetUp();
            var cts = new CancellationTokenSource();
            Monitor = MonitorFactory.CreateMonitor(MonitorType.Timer, TestHelper.CreateOptions(TestOutputDirectory, "*.txt", interval), WriteToFile, cts.Token);

            Monitor.BeginMonitor();

            AddAFile(numLines);
            Assert.AreEqual(2, AddedFiles.Count);

            // sleep here so the timer has fired once
            Thread.Sleep(interval + 500);

            // check the event output file for 1 created event
            var lines = ReadLines(AddedFiles[0]);
            Assert.AreEqual(1, lines.Count);
            CheckLine(lines[0], "Created", AddedFiles[1], numLines);

            // change the file
            AppendAllText(AddedFiles[1], "11\n");

            // sleep here so the timer has fired again
            Thread.Sleep(interval);
            lines = ReadLines(AddedFiles[0]);
            Assert.AreEqual(2, lines.Count);
            CheckLine(lines[0], "Created", AddedFiles[1], numLines);
            CheckLine(lines[1], "Changed", AddedFiles[1], 1);

            // change the file
            WriteAllText(AddedFiles[1], "0\n");

            // sleep here so the timer has fired again
            Thread.Sleep(interval);
            lines = ReadLines(AddedFiles[0]);
            Assert.AreEqual(3, lines.Count);
            CheckLine(lines[0], "Created", AddedFiles[1], numLines);
            CheckLine(lines[1], "Changed", AddedFiles[1], 1);
            CheckLine(lines[2], "Changed", AddedFiles[1], -numLines);

            // delete the file
            File.Delete(AddedFiles[1]);

            // sleep here so the timer has fired again
            Thread.Sleep(interval);
            lines = ReadLines(AddedFiles[0]);
            Assert.AreEqual(4, lines.Count);
            CheckLine(lines[0], "Created", AddedFiles[1], numLines);
            CheckLine(lines[1], "Changed", AddedFiles[1], 1);
            CheckLine(lines[2], "Changed", AddedFiles[1], -numLines);
            CheckLine(lines[3], "Deleted", AddedFiles[1], 0);

            cts.Cancel(false);
        }

        [TestCase(10, 1500)]
        [TestCase(1000000, 2000)]
        public void BeginMonitorMultipleFilesTest(int numLines, int interval)
        {
            MonitorTestSetUp();
            var cts = new CancellationTokenSource();
            Monitor = MonitorFactory.CreateMonitor(MonitorType.Timer, TestHelper.CreateOptions(TestOutputDirectory, "*.txt", interval), WriteToFile, cts.Token);

            Monitor.BeginMonitor();

            AddAFile(numLines);
            Assert.AreEqual(2, AddedFiles.Count);

            // sleep here so the timer has fired once
            Thread.Sleep(interval + 7500);

            // check the event output file for 1 created event
            var lines = ReadLines(AddedFiles[0]);
            Assert.AreEqual(1, lines.Count);
            CheckResults(lines, "Created", AddedFiles[1], numLines);

            AddAFile(numLines);
            Assert.AreEqual(3, AddedFiles.Count);

            // change the file
            AppendAllText(AddedFiles[1], "11\n");

            // sleep here so the timer has fired again
            Thread.Sleep(interval);
            lines = ReadLines(AddedFiles[0]);
            Assert.AreEqual(3, lines.Count);
            CheckResults(lines, "Created", AddedFiles[1], numLines);
            CheckResults(lines, "Changed", AddedFiles[1], 1);
            CheckResults(lines, "Created", AddedFiles[2], numLines);

            // change the file
            WriteAllText(AddedFiles[1], "0\n");
            WriteAllText(AddedFiles[2], "0\n");

            // sleep here so the timer has fired again
            Thread.Sleep(interval);
            lines = ReadLines(AddedFiles[0]);
            Assert.AreEqual(5, lines.Count);
            CheckResults(lines, "Created", AddedFiles[1], numLines);
            CheckResults(lines, "Changed", AddedFiles[1], 1);
            CheckResults(lines, "Created", AddedFiles[2], numLines);
            CheckResults(lines, "Changed", AddedFiles[1], -numLines);
            CheckResults(lines, "Changed", AddedFiles[2], -(numLines - 1));

            // delete the file
            File.Delete(AddedFiles[1]);
            File.Delete(AddedFiles[2]);

            // sleep here so the timer has fired again
            Thread.Sleep(interval);
            lines = ReadLines(AddedFiles[0]);
            Assert.AreEqual(7, lines.Count);
            CheckResults(lines, "Created", AddedFiles[1], numLines);
            CheckResults(lines, "Changed", AddedFiles[1], 1);
            CheckResults(lines, "Created", AddedFiles[2], numLines);
            CheckResults(lines, "Changed", AddedFiles[1], -numLines);
            CheckResults(lines, "Changed", AddedFiles[2], -(numLines - 1));
            CheckResults(lines, "Deleted", AddedFiles[2], 0);
            CheckResults(lines, "Deleted", AddedFiles[1], 0);

            cts.Cancel(false);
        }
    }
}