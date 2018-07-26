using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using FileWatcher.Exceptions;
using NUnit.Framework;

namespace FileWatcherTest.ExceptionsTests
{
    [TestFixture]
    public class ValidationExceptionTests
    {
        [Test]
        public void ConstructorStringExceptionTest()
        {
            const string expectedMessage = "message";
            var innerEx = new Exception("foo");
            var sut = new ValidationException(expectedMessage, innerEx);

            Assert.IsNull(sut.ResourceReferenceProperty);
            Assert.AreEqual(innerEx, sut.InnerException);
            Assert.AreEqual(expectedMessage, sut.Message);
        }

        [Test]
        public void ConstructorStringTest()
        {
            const string expectedMessage = "message";
            var sut = new ValidationException(expectedMessage);

            Assert.IsNull(sut.ResourceReferenceProperty);
            Assert.IsNull(sut.InnerException);
            Assert.AreEqual(expectedMessage, sut.Message);
        }

        [Test]
        public void DefaultConstructorTest()
        {
            const string expectedMessage = "Exception of type 'FileWatcher.Exceptions.ValidationException' was thrown.";
            var sut = new ValidationException();

            Assert.IsNull(sut.ResourceReferenceProperty);
            Assert.IsNull(sut.InnerException);
            Assert.AreEqual(expectedMessage, sut.Message);
        }

        [Test]
        public void GetObjectDataNullTest()
        {
            var sut = new ValidationException("message") {ResourceReferenceProperty = "MyReferenceProperty"};
            Assert.Throws<ArgumentNullException>(() => sut.GetObjectData(null, new StreamingContext()));
        }

        [Test]
        public void SerializationDeserializationTest()
        {
            var innerEx = new Exception("foo");
            var originalException = new ValidationException("message", innerEx) {ResourceReferenceProperty = "MyReferenceProperty"};
            var buffer = new byte[4096];
            var ms = new MemoryStream(buffer);
            var ms2 = new MemoryStream(buffer);
            var formatter = new BinaryFormatter();

            formatter.Serialize(ms, originalException);
            var deserializedException = (ValidationException)formatter.Deserialize(ms2);

            Assert.AreEqual(originalException.ResourceReferenceProperty, deserializedException.ResourceReferenceProperty);
            Assert.AreEqual(originalException.InnerException.Message, deserializedException.InnerException.Message);
            Assert.AreEqual(originalException.Message, deserializedException.Message);
        }
    }
}