using System.Threading.Tasks;
using FiveChecks.Applic.Common;
using FiveChecks.Applic.DiskSpaceCheck;
using LanguageExt.Common;
using NUnit.Framework;

namespace FiveChecks.Tests.Applic.Commands
{
    [TestFixture()]
    public class CheckDiskSpaceCommandTests
    {
        public static class ShowDiskSpaceToastNotificationCallCount
        {
            public const int One = 1;
            public const int Zero = 0;
        }

        public static class RemoveDiskSpaceToastNotificationCallCount
        {
            public const int One = 1;
            public const int Zero = 0;
        }

        public static class LoadDiskSpaceCallCount
        {
            public const int One = 1;
            public const int Zero = 0;
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        [TestCase(40,true,30.0, 10.0, LoadDiskSpaceCallCount.One, ShowDiskSpaceToastNotificationCallCount.Zero, RemoveDiskSpaceToastNotificationCallCount.One, false,false, Description = "Less than required and subtract sccm cache, do not show notification")]
        [TestCase(40, true, 50.0, 10.0, LoadDiskSpaceCallCount.One, ShowDiskSpaceToastNotificationCallCount.Zero, RemoveDiskSpaceToastNotificationCallCount.One, false, false, Description = "More than required and subtract sccm cache, do not show notification")]
        [TestCase(40, false, 50.0, 10.0, LoadDiskSpaceCallCount.One, ShowDiskSpaceToastNotificationCallCount.Zero, RemoveDiskSpaceToastNotificationCallCount.One, false, false, Description = "More than required and subtract sccm cache, do not show notification")]
        [TestCase(40, false, 30.0, 10.0, LoadDiskSpaceCallCount.One, ShowDiskSpaceToastNotificationCallCount.One, RemoveDiskSpaceToastNotificationCallCount.Zero, true, false, Description = "Less than required, do not subtract sccm cache, show notification")]
        [TestCase(40, true, 20.0, 10.0, LoadDiskSpaceCallCount.One, ShowDiskSpaceToastNotificationCallCount.One, RemoveDiskSpaceToastNotificationCallCount.Zero,true, false, Description = "Less than required and subtract sccm cache, show notification")]
        public void CheckDiskSpaceTest(decimal requiredFreeDiskSpace, bool subtractSccmCache,
            decimal totalFreeDiskSpace, decimal sccmCacheSize, int expectedLoadDiskSpaceCallCount,
            int expectedShowDiskSpaceToastNotificationCount, int expectedRemoveDiskSpaceToastNotificationCallCount,
            bool isNonCompliant, bool isDisabled)
        {
            var actualLoadDiskSpaceCallCount = 0;
            var actualShowDiskSpaceToastNotificationCount = 0;
            var actualRemoveDiskSpaceToastNotificationCount = 0;
            var actual = 
                    CheckDiskSpaceCommand.CheckDiskSpacePure(
                        requiredFreeDiskSpace, 
                        subtractSccmCache,
                        async () =>
                        {
                            actualLoadDiskSpaceCallCount++;
                            await Task.CompletedTask;
                            return new DiskSpaceInfo {TotalFreeDiskSpace = totalFreeDiskSpace, SccmCacheSize = sccmCacheSize};
                        },
                        (info => isNonCompliant),
                        (reqFreeDiskSpace) => { 
                            actualShowDiskSpaceToastNotificationCount++;
                            return Task.FromResult(new Result<ToastNotificationVisibility>(ToastNotificationVisibility.Show));
                        }, () =>
                        {
                            actualRemoveDiskSpaceToastNotificationCount++;
                            return Task.FromResult(new Result<ToastNotificationVisibility>(ToastNotificationVisibility.Hide));
                        },isDisabled);

            Assert.AreEqual(expectedLoadDiskSpaceCallCount, actualLoadDiskSpaceCallCount, "LoadDiskSpaceResult");
            Assert.AreEqual(expectedShowDiskSpaceToastNotificationCount, actualShowDiskSpaceToastNotificationCount,"ShowDiskSpaceToastNotification");
            Assert.AreEqual(expectedRemoveDiskSpaceToastNotificationCallCount, actualRemoveDiskSpaceToastNotificationCount, "RemoveDiskSpaceToastNotification");
        }

        [Test]
        [Category(TestCategory.ManualTests)]
        public void IsDisabledTest()
        {
            var actual = Profile.IsNotificationDisabled(false, typeof(CheckDiskSpaceCommand));
            Assert.AreEqual(true, actual, @"Value is not set: [HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Policies\FiveChecks\FiveChecks\DiskSpaceCheck]Disabled=1");
        }
    }
}