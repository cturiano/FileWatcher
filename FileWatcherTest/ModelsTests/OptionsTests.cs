using NUnit.Framework;

namespace FileWatcherTest.ModelsTests
{
    [TestFixture]
    public class OptionsTests
    {
        [Test]
        public void ConstructorsAndParametersTest()
        {
            var zero = TestHelper.CreateOptions(null, null, 13);
            var one = TestHelper.CreateOptions("", "");
            var two = TestHelper.CreateOptions("\\Server\\Share", "*.txt", 7);
            var three = TestHelper.CreateOptions(two);

            Assert.AreEqual(one, one);
            Assert.AreEqual(one, one);
            Assert.AreEqual(two, three);

            Assert.AreNotEqual(one, null);
            Assert.AreNotEqual(one, null);
            Assert.AreNotEqual(one, two);

            Assert.AreEqual("C:\\Windows", zero.FilePath);
            Assert.AreEqual("*.txt", zero.FilePattern);
            Assert.AreEqual(13, zero.Timer);

            Assert.AreEqual("", one.FilePath);
            Assert.AreEqual("", one.FilePattern);
            Assert.AreEqual(1000, one.Timer);

            Assert.AreEqual("\\Server\\Share", two.FilePath);
            Assert.AreEqual("*.txt", two.FilePattern);
            Assert.AreEqual(7, two.Timer);

            Assert.AreEqual(three.FilePath, two.FilePath);
            Assert.AreEqual(three.FilePattern, two.FilePattern);
            Assert.AreEqual(three.Timer, two.Timer);
        }

        [Test]
        public void EqualsTest()
        {
            var zero = TestHelper.CreateOptions(null, null);
            var one = TestHelper.CreateOptions("", "");
            var two = TestHelper.CreateOptions("\\Server\\Share", "txt");
            var three = TestHelper.CreateOptions(two);

            Assert.IsTrue(zero.Equals(zero));
            Assert.IsFalse(zero.Equals(null));
            Assert.IsFalse(zero.Equals(null));
            Assert.IsFalse(zero.Equals(one));
            Assert.IsFalse(zero.Equals((object)one));
            Assert.IsFalse(two.Equals((object)three));
            Assert.IsFalse(one.Equals(two));
            Assert.IsTrue(two.Equals(three));
        }
    }
}