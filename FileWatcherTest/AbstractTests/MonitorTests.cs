using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileWatcher.Models;
using FileWatcherTest.ModelsTests;
using NUnit.Framework;
using Monitor = FileWatcher.Abstract.Monitor;

namespace FileWatcherTest.AbstractTests
{
    [TestFixture]
    public class MonitorTests : MonitorTestsBase
    {
        private class TestMonitor : Monitor
        {
            #region Constructors

            public TestMonitor(Options options, Action<string> outputFunction, CancellationToken ct) : base(options, outputFunction, ct)
            {
            }

            #endregion

            #region Public Methods

            public override void BeginMonitor()
            {
            }

            public async Task<int> GetLineCountPassThru(string fileName)
            {
                return await GetLineCount(fileName);
            }

            public async Task<List<string>> GetLinesPassThru(string fileName)
            {
                return await GetLines(fileName);
            }

            #endregion
        }

        private static void CheckLines(int expectedCount, IReadOnlyList<string> lines)
        {
            Assert.AreEqual(expectedCount, lines.Count);

            for (var i = 0; i < expectedCount; i++)
            {
                Assert.AreEqual(i.ToString(), lines[i]);
            }
        }

        [Test]
        public void ConstructorAndPropertiesTest()
        {
            Monitor = new TestMonitor(TestHelper.CreateOptions(), WriteToFile, new CancellationToken());
            Assert.IsNotNull(Monitor);
            Assert.IsNotNull(Monitor.CancellationToken);
            Assert.IsNotNull(Monitor.FileLineMap);
            Assert.IsNotNull(Monitor.FilePath);
            Assert.IsNotNull(Monitor.FilePattern);
        }

        [Test]
        public void GetLineCountTest()
        {
            MonitorTestSetUp();
            const int numLines = 10;
            var monitor = new TestMonitor(TestHelper.CreateOptions(), WriteToFile, new CancellationToken());
            AddAFile(numLines);
            Assert.AreEqual(2, AddedFiles.Count);
            Assert.AreEqual(numLines, monitor.GetLineCountPassThru(AddedFiles[1]).Result);

            var cts = new CancellationTokenSource();
            // lock the file on another task
            Task.Run(() =>
                     {
                         using (File.Open(AddedFiles[1], FileMode.Open, FileAccess.ReadWrite))
                         {
                         }
                     },
                     cts.Token);

            // task that waits for GetLines to return
            var t = Task.Factory.StartNew(async () => { Assert.AreEqual(numLines, await monitor.GetLineCountPassThru(AddedFiles[1])); });

            // sleep the test thread for a second just for fun
            Thread.Sleep(1000);

            // now cancel the file locking task and the GetLines should proceed
            cts.Cancel(false);

            t.Wait();

            MonitorTestCleanUp();
        }

        [Test]
        public void GetLinesTest()
        {
            MonitorTestSetUp();
            const int numLines = 10;
            var monitor = new TestMonitor(TestHelper.CreateOptions(), WriteToFile, new CancellationToken());
            AddAFile(numLines);
            Assert.AreEqual(2, AddedFiles.Count);
            var lines = monitor.GetLinesPassThru(AddedFiles[1]).Result;

            CheckLines(numLines, lines);

            var cts = new CancellationTokenSource();

            // lock the file on another task
            Task.Run(() =>
                     {
                         using (File.Open(AddedFiles[1], FileMode.Open, FileAccess.ReadWrite))
                         {
                         }
                     },
                     cts.Token);

            // task that waits for GetLines to return
            var t = Task.Factory.StartNew(async () =>
                                          {
                                              lines = await monitor.GetLinesPassThru(AddedFiles[1]);
                                              CheckLines(numLines, lines);
                                          });

            // sleep the test thread for a second just for fun
            Thread.Sleep(1000);

            // now cancel the file locking task and the GetLines should proceed
            cts.Cancel(false);

            t.Wait();

            MonitorTestCleanUp();
        }
    }
}