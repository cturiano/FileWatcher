using System.Linq;
using FileWatcher.Models;
using NUnit.Framework;

namespace FileWatcherTest.ModelsTests
{
    [TestFixture]
    internal class OptionsValidatorTests
    {
        [TestCase(null, "Directory is null.")]
        [TestCase("", "Directory is zero length or contains invalid characters: {0}.")]
        [TestCase("C:\\Windows\\>", "Directory is zero length or contains invalid characters: {0}.")]
        [TestCase("**", "Directory is zero length or contains invalid characters: {0}.")]
        [TestCase("C:\\ssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss", "The given directory is too long: {0}.")]
        [TestCase("C:\\Windows\\:", "Directory contains a colon that is not part of a volume identifier: {0}.")]
        [TestCase("C:\\DoesNotExist", "Directory does not exist: {0}.")]
        [TestCase("\\Server\\Share", "Directory does not exist: {0}.")]
        public void ValidateDirectoryFailTest(string directory, string errorMessage)
        {
            OptionsValidator.Instance.ClearErrors();
            var options = new Options {FilePath = directory, FilePattern = ".exe"};
            Assert.IsFalse(OptionsValidator.Instance.Validate(options));

            Assert.GreaterOrEqual(OptionsValidator.Instance.Errors.Count, 1);
            Assert.IsTrue(OptionsValidator.Instance.Errors.Contains(string.Format(errorMessage, directory)));
        }

        [TestCase("C:\\Windows")]
        [TestCase("C:\\Windows\\System32")]
        [TestCase("\\Windows\\System32")]
        public void ValidateDirectorySucceedTest(string directory)
        {
            OptionsValidator.Instance.ClearErrors();
            var options = new Options {FilePath = directory, FilePattern = ".exe"};
            Assert.IsTrue(OptionsValidator.Instance.Validate(options));
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("\n")]
        public void ValidateFilePatternFailTest(string filePattern)
        {
            OptionsValidator.Instance.ClearErrors();
            var options = new Options {FilePath = "C:\\Windows", FilePattern = filePattern};
            Assert.IsFalse(OptionsValidator.Instance.Validate(options));
        }

        [TestCase(".exe")]
        [TestCase("?exe")]
        public void ValidateFilePatternSucceedTest(string filePattern)
        {
            OptionsValidator.Instance.ClearErrors();
            var options = new Options {FilePath = "C:\\Windows", FilePattern = filePattern};
            Assert.IsTrue(OptionsValidator.Instance.Validate(options));
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(1000001)]
        [TestCase(-1000001)]
        public void ValidateTimeIntervalFailTest(int interval)
        {
            OptionsValidator.Instance.ClearErrors();
            var options = new Options {FilePath = "C:\\Windows", FilePattern = "*.txt", Timer = interval};
            Assert.IsFalse(OptionsValidator.Instance.Validate(options));
        }

        [TestCase(1)]
        [TestCase(10000)]
        [TestCase(1000000)]
        public void ValidateTimeIntervalSucceedTest(int interval)
        {
            OptionsValidator.Instance.ClearErrors();
            var options = new Options {FilePath = "C:\\Windows", FilePattern = "*.txt", Timer = interval};
            Assert.IsTrue(OptionsValidator.Instance.Validate(options));
        }

        [Test]
        public void ConstructorsAndPropertiesTests()
        {
            Assert.IsNotNull(OptionsValidator.Instance);
            Assert.IsNotNull(OptionsValidator.Instance.Errors);
        }
    }
}