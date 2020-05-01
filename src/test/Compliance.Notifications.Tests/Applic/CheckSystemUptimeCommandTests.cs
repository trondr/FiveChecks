using Compliance.Notifications.Applic.SystemUptimeCheck;
using NUnit.Framework;

namespace Compliance.Notifications.Tests.Applic
{
    [TestFixture()]
    public class CheckSystemUptimeCommandTests
    {
        [Test]
        [Category(TestCategory.ManualTests)]
        public void IsDisabledTest()
        {
            var actual = CheckSystemUptimeCommand.IsNotificationDisabled(false);
            Assert.AreEqual(true, actual, @"Value is not set: [HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Policies\github.trondr\Compliance.Notifications\SystemUptimeCheck]Disabled=1");
        }
    }
}