using Compliance.Notifications.Applic.PasswordExpiryCheck;
using NUnit.Framework;

namespace Compliance.Notifications.Tests.Applic
{
    [TestFixture()]
    public class CheckPasswordExpiryCommandTests
    {
        [Test]
        [Category(TestCategory.ManualTests)]
        public void IsDisabledTest()
        {
            var actual = CheckPasswordExpiryCommand.IsDisabled(false);
            Assert.AreEqual(true, actual, @"Value is not set: [HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Policies\github.trondr\Compliance.Notifications\PasswordExpiryCheck]Disabled=1");
        }
    }
}