using NUnit.Framework;
using System.Threading.Tasks;
using Compliance.Notifications.Common.Tests;
using LanguageExt.Common;

namespace Compliance.Notifications.Commands.CheckDiskSpace.Tests
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

        public static class LoadDiskSpaceCallCount
        {
            public const int One = 1;
            public const int Zero = 0;
        }

        [Test]
        [TestCase(40,true,30.0,10.0, LoadDiskSpaceCallCount.One, ShowDiskSpaceToastNotificationCallCount.Zero,Description = "Less than required and subtract sccm cache, do not show notification")]
        [TestCase(40, true, 50.0, 10.0, LoadDiskSpaceCallCount.One, ShowDiskSpaceToastNotificationCallCount.Zero, Description = "More than required and subtract sccm cache, do not show notification")]
        [TestCase(40, false, 50.0, 10.0, LoadDiskSpaceCallCount.One, ShowDiskSpaceToastNotificationCallCount.Zero, Description = "More than required and subtract sccm cache, do not show notification")]
        [TestCase(40, false, 30.0, 10.0, LoadDiskSpaceCallCount.One, ShowDiskSpaceToastNotificationCallCount.One,Description = "Less than required, do not subtract sccm cache, show notification")]
        [TestCase(40, true, 20.0, 10.0, LoadDiskSpaceCallCount.One, ShowDiskSpaceToastNotificationCallCount.One, Description = "Less than required and subtract sccm cache, show notification")]
        public void CheckDiskSpaceTest(decimal requiredFreeDiskSpace, bool subtractSccmCache, decimal totalFreeDiskSpace, decimal sccmCacheSize, int expectedLoadDiskSpaceCallCount, int expectedShowDiskSpaceToastNotificationCount)
        {
            var actualLoadDiskSpaceCallCount = 0;
            var actualShowDiskSpaceToastNotificationCount = 0;
            var actual = 
                    CheckDiskSpaceCommand.CheckDiskSpaceF(
                        requiredFreeDiskSpace, 
                        subtractSccmCache,
                        () =>
                        {
                            actualLoadDiskSpaceCallCount++;
                            return new DiskSpaceInfo {TotalFreeDiskSpace = totalFreeDiskSpace, SccmCacheSize = sccmCacheSize};
                        },
                        (reqFreeDiskSpace, s) => { 
                            actualShowDiskSpaceToastNotificationCount++;
                            return new Task<Result<int>>(() => 0);
                        });

            Assert.AreEqual(expectedLoadDiskSpaceCallCount, actualLoadDiskSpaceCallCount, "LoadDiskSpaceResult");
            Assert.AreEqual(expectedShowDiskSpaceToastNotificationCount, actualShowDiskSpaceToastNotificationCount,"ShowDiskSpaceToastNotification");
        }
    }
}