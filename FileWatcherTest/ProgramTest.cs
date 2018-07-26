using FileWatcher;
using NUnit.Framework;

namespace FileWatcherTest
{
    [TestFixture]
    public class ProgramTest
    {
        [TestCase("")]
        [TestCase("-p")]
        [TestCase("-p *.exe")]
        [TestCase("-d")]
        [TestCase("-d c:/windows")]
        [TestCase("-d c:/windows -p")]
        [TestCase("--pattern")]
        [TestCase("--directory")]
        [TestCase("--pattern --directory")]
        [TestCase("help")]
        [TestCase("version")]
        public void MainFailTest(string commandLineParameters)
        {
            Program.Main(commandLineParameters.Split(' '));
        }

        [TestCase("-d 'c:/Windows',-p \'*.exe\'")]
        [TestCase("--directory c:/Windows,--pattern *.exe")]
        [TestCase("--help")]
        [TestCase("--version")]
        public void MainSuccessTest(string commandLineParameters)
        {
            Assert.DoesNotThrow(() => Program.Main(commandLineParameters.Split(',')));
        }
    }
}