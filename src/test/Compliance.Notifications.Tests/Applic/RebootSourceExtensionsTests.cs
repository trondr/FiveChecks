using System.Linq;
using Compliance.Notifications.Applic.PendingRebootCheck;
using NUnit.Framework;

namespace Compliance.Notifications.Tests.Applic
{
    [TestFixture()]
    public class RebootSourceExtensionsTests
    {
        [Test]
        [Category(TestCategory.UnitTests)]
        public void GetDisabledValueNameTest()
        {
            var actual = RebootSource.AllSources.Select(source => source.GetDisabledValueName());
            Assert.AreEqual(7,actual.Count());
        }
    }
}