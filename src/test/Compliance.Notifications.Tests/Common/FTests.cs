using System;
using System.IO;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using Compliance.Notifications.Applic.DiskspaceCheck;
using Compliance.Notifications.Applic.SystemUptimeCheck;
using LanguageExt;
using NUnit.Framework;
using DirectoryInfo = Pri.LongPath.DirectoryInfo;
using FileInfo = Pri.LongPath.FileInfo;

namespace Compliance.Notifications.Tests.Common
{
    [TestFixture()]
    public class FTests
    {
        [Test]
        [Category(TestCategory.UnitTests)]
        [Category(TestCategory.UnitTests)]
        [TestCase("c:\\temp", "c:\\temp\\")]
        [TestCase("\\\\temp\\temp", "\\\\temp\\temp\\")]
        public void AppendDirectorySeparatorCharTest(string input, string expected)
        {
            var actual = input.AppendDirectorySeparatorChar();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void AppendDirectorySeparatorCharTest_NullInput_Exception()
        {
            Assert.Throws<ArgumentException>(() =>
                {
                    var actual = DiskSpace.AppendDirectorySeparatorChar(null);
                }
            );
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void AppendDirectorySeparatorCharTest_EmptyInput_Exception()
        {
            Assert.Throws<ArgumentException>(() =>
                {
                    var actual = "   ".AppendDirectorySeparatorChar();
                }
            );
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void GetFreeDiskSpaceTest()
        {
            DiskSpace.GetFreeDiskSpaceInGigaBytes("c:\\").Match<decimal>(size =>
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
        [Category(TestCategory.UnitTests)]
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
        [Category(TestCategory.UnitTests)]
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
        [Category(TestCategory.UnitTests)]
        public void TryGetFilesTest()
        {
            var actual = DiskSpace.TryGetFiles(new DirectoryInfo(@"c:\temp\UserTemp\msdtadmin"), "*.*");
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
        [Category(TestCategory.UnitTests)]
        public async Task GetFolderSizeTest()
        {
            var actual = await DiskSpace.GetFolderSize(@"c:\temp");
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
        [Category(TestCategory.UnitTests)]
        public async Task SaveLoadDiskSpaceResultTest_Success()
        {
            var expected = new DiskSpaceInfo { SccmCacheSize = 12, TotalFreeDiskSpace = 123 };
            var savedResult = await F.SaveSystemComplianceItemResult<DiskSpaceInfo>(expected).ConfigureAwait(false);
            var actual = await DiskSpace.LoadDiskSpaceResult().ConfigureAwait(false);
            Assert.AreEqual(expected, actual);
        }

        [Test()]
        [Category(TestCategory.UnitTests)]
        public async Task LoadDiskSpaceResultTest_FileNotExist()
        {
            var savedResult = F.ClearSystemComplianceItemResult<DiskSpaceInfo>();
            var actual = await DiskSpace.LoadDiskSpaceResult().ConfigureAwait(false);
            Assert.AreEqual(DiskSpaceInfo.Default, actual);
        }

        [Test()]
        [Category(TestCategory.UnitTests)]
        public void ToExceptionMessageTest_AggregateException()
        {
            var mainMessage = "Many exceptions throw.";
            var message1 = "File not found.";
            var message2 = "Invalid argument";
            var testException = new AggregateException(mainMessage, new Exception[] { new FileNotFoundException(message1), new ArgumentException(message2) });
            var nl = Environment.NewLine;
            var expected = $"AggregateException: {mainMessage}{nl}FileNotFoundException: {message1}{nl}ArgumentException: {message2}";
            var actual = testException.ToExceptionMessage();
            Assert.AreEqual(actual, expected);
        }

        [Test]
        [Category(TestCategory.ManualTests)]
        public async Task InstallTest()
        {
            var actual = await Setup.Install();
            var exitCode = actual.Match(i =>
            {
                Assert.IsTrue(true);
                return 0;
            }, exception =>
            {
                Assert.Fail(exception.ToExceptionMessage());
                return 1;
            });
        }

        [Test]
        [Category(TestCategory.ManualTests)]
        public async Task UnInstallTest()
        {
            var actual = await Setup.UnInstall();
            var exitCode = actual.Match(i =>
            {
                Assert.IsTrue(true);
                return 0;
            }, exception =>
            {
                Assert.Fail(exception.ToExceptionMessage());
                return 1;
            });
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        [TestCase(1, "Good morning")]
        [TestCase(12, "Good afternoon")]
        [TestCase(16, "Good evening")]
        [TestCase(21, "Good evening")]
        [TestCase(23, "Good evening")]
        public void GetGreetingTest(int hour, string expected)
        {
            var actual = F.GetGreeting(new DateTime(2020, 01, 13, hour, 31, 21));
            Assert.AreEqual(expected, actual);
        }

        [Test()]
        [Category(TestCategory.UnitTests)]
        public async Task GetGivenNameTest()
        {
            var actual = await F.GetGivenName();
            actual.Match(s =>
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(s));
                return "";
            }, () =>
             {
                 Assert.Fail("None not expected");
                 return "";
             });
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        [TestCase("C:\\Program Files (x86)\\github.com.trondr\\Compliance.Notifications\\1.0.20092.46\\Compliance.Notifications.exe CheckDiskSpace /requiredDiskSpace=\"100\"", "Compliance.Notifications", "Compliance.Notifications.exe CheckDiskSpace")]
        [TestCase("\"C:\\Program Files (x86)\\github.com.trondr\\Compliance.Notifications\\1.0.20092.46\\Compliance.Notifications.exe\" CheckDiskSpace /requiredDiskSpace=\"100\"", "Compliance.Notifications", "Compliance.Notifications.exe CheckDiskSpace")]
        public void StripPathFromCommandLineTest(string input, string processName, string expected)
        {
            var actual = F.StripPathAndArgumentsFromCommandLine(processName, input);
            Assert.AreEqual(expected, actual);
        }

        [Test()]
        [Category(TestCategory.ManualTests)]
        public void OpenRestartDialogTest()
        {
            F.OpenRestartDialog();
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        [TestCase("2020-01-13 16:55:27", "2020-01-13 10:55:27", "6 hours")]
        [TestCase("2020-01-13 16:55:27", "2020-01-12 16:55:27", "1 day")]
        [TestCase("2020-01-13 16:55:27", "2020-01-11 16:55:27", "2 days")]
        public void InPeriodFromNowPureTest(string dateTimeString, string nowDateTimeString, string expected)
        {
            var dateTime = DateTime.Parse(dateTimeString);
            var now = DateTime.Parse(nowDateTimeString);
            var actual = dateTime.InPeriodFromNowPure(() => now);
            Assert.AreEqual(expected, actual);
        }

        [Test()]
        [Category(TestCategory.UnitTests)]
        public void GetLastRestartTimeTest()
        {
            var actual = SystemUptime.GetLastRestartTime();
            Assert.IsTrue(actual < DateTime.Now,"Restart time is in the future, which does not make sense.");
        }
    }
}