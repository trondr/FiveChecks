using System;
using NUnit.Framework;

namespace Compliance.Notifications.Common.Tests
{
    [TestFixture(Category = TestCategory.UnitTests)]
    public class FTests
    {
        [Test]
        [Category(TestCategory.UnitTests)]
        [TestCase("c:\\temp", "c:\\temp\\")]
        [TestCase("\\\\temp\\temp", "\\\\temp\\temp\\")]
        public void AppendDirectorySeparatorCharTest(string input, string expected)
        {
            var actual = input.AppendDirectorySeparatorChar();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AppendDirectorySeparatorCharTest_NullInput_Exception()
        {
            Assert.Throws<ArgumentException>(() =>
                {
                    var actual = F.AppendDirectorySeparatorChar(null);
                }
            );
        }

        [Test]
        public void AppendDirectorySeparatorCharTest_EmptyInput_Exception()
        {
            Assert.Throws<ArgumentException>(() =>
                {
                    var actual = "   ".AppendDirectorySeparatorChar();
                }
            );
        }

        [Test]
        public void GetFreeDiskSpaceTest()
        {
            F.GetFreeDiskSpaceInGigaBytes("c:\\").Match<decimal>(size =>
            {
                Assert.IsTrue(size > 0M);
                Assert.IsTrue(size < 10000M);
                return size;
            },exception =>
            {
                Assert.Fail(exception.ToString());
                return 0;
            });
        }

        [Test()]
        [TestCase(@"test.txt","somename1", @"test.somename1.txt")]
        public void AppendToFileNameTest(string fileName, string name, string expected)
        {
            var actual = F.AppendNameToFileName(fileName, name);
            Assert.AreEqual(expected,actual);
        }
    }
}