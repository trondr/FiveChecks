using System.Linq;
using FiveChecks.Applic.PendingRebootCheck;
using NUnit.Framework;

namespace FiveChecks.Tests.Applic
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