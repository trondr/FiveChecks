using System;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using Compliance.Notifications.Applic.PendingRebootCheck;
using Compliance.Notifications.Tests.Common;
using LanguageExt.Common;
using NUnit.Framework;
using Pri.LongPath;

namespace Compliance.Notifications.Tests.Applic
{
    [TestFixture]
    public class FTests
    {
        [Test]
        [Category(TestCategory.UnitTests)]
        public void TryFuncTest_Expect_Exception()
        {
            var actual = F.TryFunc(() =>
            {
                throw new Exception("Test");
#pragma warning disable 162
                return new Result<int>(0);
#pragma warning restore 162
            });
            actual.Match(Succ: i =>
                {
                    Assert.IsFalse(true, "Success not expected.");
                    return 0;
                },
                exception =>
                {
                    Console.WriteLine(exception.ToExceptionMessage());
                    Assert.IsTrue(true, "N/A");
                    return 1;
                });
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void TryFuncTest_Expect_Success()
        {
            var actual = F.TryFunc(() => new Result<int>(0));
            actual.Match(Succ: i =>
                {
                    Assert.IsTrue(true, "N/A.");
                    return 0;
                },
                exception =>
                {
                    Console.WriteLine(exception.ToExceptionMessage());
                    Assert.IsTrue(true, "Failure not expected");
                    return 1;
                });
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public async Task AsyncTryFuncTest_Expect_Exception()
        {
            var actual = await F.AsyncTryFunc(async () =>
            {
                throw new Exception("Test");
#pragma warning disable 162
                return await Task.FromResult(new Result<int>(0));
#pragma warning restore 162
            });
            actual.Match(Succ: i =>
                {
                    Assert.IsFalse(true, "Success not expected.");
                    return 0;
                },
                exception =>
                {
                    Console.WriteLine(exception.ToExceptionMessage());
                    Assert.IsTrue(true, "N/A");
                    return 1;
                });
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public async Task AsyncTryFuncTest_Expect_Success()
        {
            var actual = await F.AsyncTryFunc(async () =>
            {
                return await Task.FromResult(new Result<int>(0));
            });
            actual.Match(Succ: i =>
                {
                    Assert.IsTrue(true, "N/A");
                    return 0;
                },
                exception =>
                {
                    Console.WriteLine(exception.ToExceptionMessage());
                    Assert.IsFalse(true, "Failure not expected.");
                    return 1;
                });
        }

        [Test()]
        [TestCase(1024L, "1 KB")]
        [TestCase(1000000L, "977 KB")]
        [TestCase(1048576L, "1.0 MB")]
        [TestCase(1073741824L, "1.0 GB")]
        [TestCase(2073741824L, "1.93 GB")]
        [TestCase(2073741824000L, "1.89 TB")]
        [Category(TestCategory.UnitTests)]
        public void BytesToReadableStringTest(long bytes, string expected)
        {
            var actual = bytes.BytesToReadableString();
            Assert.AreEqual(expected, actual);
        }

        [Test()]
        [Category(TestCategory.UnitTests)]
        public void CreateShortcutTest()
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var shortCutPath = Path.Combine(desktopPath, "Miiine Dokumenter.lnk");
            var myDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var explorerExe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
            var actual = F.CreateShortcut(shortCutPath, $"\"{explorerExe}\"", $"/root,\"{myDocumentsFolder}\"", "Miiine dokumeeenter", true);

            var shortCutPath2 = Path.Combine(desktopPath, "Miiine Desktop.lnk");
            var myDesktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var actual2 = F.CreateShortcut(shortCutPath2, $"\"{explorerExe}\"", $"/root,\"{myDesktopFolder}\"", "Miiin Desktop", true);

        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void GetPolicyCategoryTest_SomeType()
        {
            var actual = typeof(CheckPendingRebootCommand).GetPolicyCategory();
            actual.IfNone(() => Assert.Fail("Did not expect None as result."));
            var expected = "PendingRebootCheck";
            actual.IfSome(s => Assert.AreEqual(expected, s));
        }


        [Test]
        [Category(TestCategory.ManualTests)]
        public async Task GetRandomImageFromCacheTest()
        {
            var actual = await F.GetRandomImageFromCache(@"E:\Dev\github.trondr\Compliance.Notifications\src\heroimages").ConfigureAwait(false);
            actual.Match(uri => { Assert.IsTrue(File.Exists(new Uri(uri).LocalPath),$"image file does not exist: {uri}"); return "";}, () => { Assert.Fail();return "";});
            actual.IfNone(() => Assert.Fail("Did not expect None."));
        }
        [Test]
        [Category(TestCategory.UnitTests)]
        [TestCase(2, 1, 1, "2 days")]
        [TestCase(1,1,1,"1 day")]
        [TestCase(0, 1, 1, "1 hour")]
        [TestCase(0, 10, 1, "10 hours")]
        [TestCase(0, 0, 1, "0 hours")]
        public void TimeSpanToReadableStringTest(int days, int hours, int minutes,string expected)
        {
            var timeSpan = new TimeSpan(days,hours,minutes,0);
            var actual = timeSpan.TimeSpanToString();
            Assert.AreEqual(expected,actual,"Timespan text was not expected.");
        }
    }
}
