using Compliance.Notifications.Applic.DesktopDataCheck;
using NUnit.Framework;

namespace Compliance.Notifications.Tests.Applic
{
    [TestFixture()]
    public class CheckDesktopDataCommandTests
    {
        [Test]
        [Category(TestCategory.ManualTests)]
        public void IsDisabledTest()
        {
            var actual = CheckDesktopDataCommand.IsDisabled(false);
            Assert.AreEqual(true, actual, @"Value is not set: [HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Policies\github.trondr\Compliance.Notifications\DesktopDataCheck]Disabled=1");
        }
    }
}