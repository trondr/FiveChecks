using Compliance.Notifications.Applic.DiskSpaceCheck;
using Compliance.Notifications.Applic.PendingRebootCheck;
using NUnit.Framework;

namespace Compliance.Notifications.Tests.Applic
{
    [TestFixture()]
    public class CheckDiskSpaceCommandTests
    {
        [Test]
        [Category(TestCategory.ManualTests)]
        public void IsDisabledTest()
        {
            var actual = CheckDiskSpaceCommand.IsNotificationDisabled(false);
            Assert.AreEqual(true,actual, @"Value is not set: [HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Policies\github.trondr\Compliance.Notifications\DiskSpaceCheck]Disabled=1");
        }
    }

    [TestFixture()]
    public class CheckPendingRebootCommandTests
    {
        [Test]
        [Category(TestCategory.ManualTests)]
        public void IsDisabledTest()
        {
            var actual = CheckPendingRebootCommand.IsNotificationDisabled(false);
            Assert.AreEqual(true, actual, @"Value is not set: [HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Policies\github.trondr\Compliance.Notifications\PendingRebootCheck]Disabled=1");
        }
    }
}