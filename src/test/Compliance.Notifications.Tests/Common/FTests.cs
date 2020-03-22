using Compliance.Notifications.Common;
using System;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
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
            }, exception =>
             {
                 Assert.Fail(exception.ToString());
                 return 0;
             });
        }

        [Test()]
        [TestCase(@"test.txt", "somename1", @"test.somename1.txt")]
        public void AppendToFileNameTest(string fileName, string name, string expected)
        {
            var actual = F.AppendNameToFileName(fileName, name);
            Assert.AreEqual(expected, actual);
        }

        public class TestData : Record<TestData>
        {
            public TestData(Some<string> name, Some<string> description)
            {
                Description = description;
                Name = name;
            }

            public string Name { get; }

            public string Description { get; }
        }

        [Test]
        public async Task SaveAndLoadComplianceItemResultTest()
        {
            var testData = new TestData("A Name","A description");
            Some<string> fileName = $@"c:\temp\{typeof(TestData).Name}.json";
            var result = await F.SaveComplianceItemResult<TestData>(testData,fileName);
            result.Match<Unit>(unit =>
            {
                Assert.IsTrue(true);
                return Unit.Default;
            }, exception =>
            {
                Assert.Fail();
                return Unit.Default;
            });
            var loadedTestData = await F.LoadComplianceItemResult<TestData>(fileName);
            var ignore = loadedTestData.Match<TestData>(
                data =>
                        {
                            Assert.AreEqual("A Name", data.Name);
                            Assert.AreEqual("A description", data.Description);
                            return data;
                        }, 
                exception =>
                        {
                            Assert.Fail(exception.Message);
                            throw exception;
                        });
        }
    }
}