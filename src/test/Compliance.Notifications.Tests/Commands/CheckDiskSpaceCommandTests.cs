using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using Compliance.Notifications.Applic.DiskSpaceCheck;
using Compliance.Notifications.Tests.Common;
using LanguageExt.Common;
using NUnit.Framework;

namespace Compliance.Notifications.Tests.Commands
{
    [TestFixture()]
    [Category(TestCategory.UnitTests)]
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
        [TestCase(40,true,30.0, 10.0, LoadDiskSpaceCallCount.One, ShowDiskSpaceToastNotificationCallCount.Zero, RemoveDiskSpaceToastNotificationCallCount.One, false, Description = "Less than required and subtract sccm cache, do not show notification")]
        [TestCase(40, true, 50.0, 10.0, LoadDiskSpaceCallCount.One, ShowDiskSpaceToastNotificationCallCount.Zero, RemoveDiskSpaceToastNotificationCallCount.One, false, Description = "More than required and subtract sccm cache, do not show notification")]
        [TestCase(40, false, 50.0, 10.0, LoadDiskSpaceCallCount.One, ShowDiskSpaceToastNotificationCallCount.Zero, RemoveDiskSpaceToastNotificationCallCount.One, false, Description = "More than required and subtract sccm cache, do not show notification")]
        [TestCase(40, false, 30.0, 10.0, LoadDiskSpaceCallCount.One, ShowDiskSpaceToastNotificationCallCount.One, RemoveDiskSpaceToastNotificationCallCount.Zero, true, Description = "Less than required, do not subtract sccm cache, show notification")]
        [TestCase(40, true, 20.0, 10.0, LoadDiskSpaceCallCount.One, ShowDiskSpaceToastNotificationCallCount.One, RemoveDiskSpaceToastNotificationCallCount.Zero,true, Description = "Less than required and subtract sccm cache, show notification")]
        public void CheckDiskSpaceTest(decimal requiredFreeDiskSpace, bool subtractSccmCache, decimal totalFreeDiskSpace, decimal sccmCacheSize, int expectedLoadDiskSpaceCallCount, int expectedShowDiskSpaceToastNotificationCount, int expectedRemoveDiskSpaceToastNotificationCallCount, bool isNonCompliant)
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
                        (reqFreeDiskSpace, s) => { 
                            actualShowDiskSpaceToastNotificationCount++;
                            return Task.FromResult(new Result<ToastNotificationVisibility>(ToastNotificationVisibility.Show));
                        }, () =>
                        {
                            actualRemoveDiskSpaceToastNotificationCount++;
                            return Task.FromResult(new Result<ToastNotificationVisibility>(ToastNotificationVisibility.Hide));
                        });

            Assert.AreEqual(expectedLoadDiskSpaceCallCount, actualLoadDiskSpaceCallCount, "LoadDiskSpaceResult");
            Assert.AreEqual(expectedShowDiskSpaceToastNotificationCount, actualShowDiskSpaceToastNotificationCount,"ShowDiskSpaceToastNotification");
            Assert.AreEqual(expectedRemoveDiskSpaceToastNotificationCallCount, actualRemoveDiskSpaceToastNotificationCount, "RemoveDiskSpaceToastNotification");
        }
    }
}