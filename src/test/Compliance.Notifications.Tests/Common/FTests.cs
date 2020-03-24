using Compliance.Notifications.Common;
using System;
using System.IO;
using System.Threading.Tasks;
using Compliance.Notifications.Commands.CheckDiskSpace;
using Compliance.Notifications.ComplianceItems.SystemDiskSpace;
using LanguageExt;
using NUnit.Framework;
using DirectoryInfo = Pri.LongPath.DirectoryInfo;
using FileInfo = Pri.LongPath.FileInfo;

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
            public TestData(Some<string> name, Some<string> description, UDecimal someNumber)
            {
                Description = description;
                SomeNumber = someNumber;
                Name = name;
            }

            public string Name { get; }

            public string Description { get; }

            public UDecimal SomeNumber { get; }
        }

        [Test]
        public async Task SaveAndLoadComplianceItemResultTest()
        {
            var testData = new TestData("A Name", "A description", 81.3452m);
            Some<string> fileName = $@"c:\temp\{typeof(TestData).Name}.json";
            var result = await F.SaveComplianceItemResult<TestData>(testData, fileName);
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
                            Assert.AreEqual(new UDecimal(81.3452m), data.SomeNumber);
                            return data;
                        },
                exception =>
                        {
                            Assert.Fail(exception.Message);
                            throw exception;
                        });
        }

        [Test()]
        public void TryGetFilesTest()
        {
            var actual = F.TryGetFiles(new DirectoryInfo(@"c:\temp\UserTemp\msdtadmin"), "*.*");
            var files = actual.Try().Match<FileInfo[]>(infos =>
                {
                    Assert.IsTrue(false, "Not expected.");
                    return infos;
                }, exception =>
                {
                    Assert.True(true);
                    return new FileInfo[] { };
                });
        }

        [Test()]
        public async Task GetFolderSizeTest()
        {
            var actual = await F.GetFolderSize(@"c:\temp");
            var actualSize = actual.Match<UDecimal>(size =>
            {
                Assert.IsTrue(true);
                return size;
            }, exception =>
            {
                Assert.False(true, "Not expected to fail");
                return 0M;
            });
            Assert.IsTrue(actualSize > 0);
        }

        [Test()]
        public async Task LoadSystemComplianceItemResultTest_Success()
        {
            var expected = new DiskSpaceInfo { SccmCacheSize = 12, TotalFreeDiskSpace = 123 };
            var savedResult = await F.SaveSystemComplianceItemResult<DiskSpaceInfo>(expected).ConfigureAwait(false);
            var actual = await F.LoadDiskSpaceResult().ConfigureAwait(false);
            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public async Task LoadSystemComplianceItemResultTest_FileNotExist()
        {
            var savedResult = F.ClearSystemComplianceItemResult<DiskSpaceInfo>();
            var actual = await F.LoadDiskSpaceResult().ConfigureAwait(false);
            Assert.AreEqual(DiskSpaceInfo.Default, actual);
        }

        [Test()]
        public void ToExceptionMessageTest_AggregateException()
        {
            var mainMessage = "Many exceptions throw.";
            var message1 = "File not found.";
            var message2 = "Invalid argument";
            var testException = new AggregateException(mainMessage, new Exception[]{new FileNotFoundException(message1),new ArgumentException(message2)});
            var nl = Environment.NewLine;
            var expected = $"{mainMessage}{nl}{message1}{nl}{message2}";
            var actual = testException.ToExceptionMessage();
            Assert.AreEqual(actual,expected);
        }
    }
}