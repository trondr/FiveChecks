using NUnit.Framework;
using System.Threading.Tasks;
using Compliance.Notifications.Common.Tests;

namespace Compliance.Notifications.Commands.Tests
{
    [TestFixture()]
    public class CommandDefinitionsTests
    {
        [Test()]
        [Category(TestCategory.UnitTests)]
        public async Task MeasureUserComplianceItemsTest()
        {
            var actualResult = await CommandDefinitions.MeasureUserComplianceItems();
            var actual = actualResult.Match(i => i, exception =>
            {
                Assert.Fail(exception.Message);
                return 1;
            });
        }

        [Test()]
        [Category(TestCategory.UnitTests)]
        public async Task MeasureSystemComplianceItemsTest()
        {
            var actualResult = await CommandDefinitions.MeasureSystemComplianceItems();
            var actual = actualResult.Match(i => i, exception =>
            {
                Assert.Fail(exception.Message);
                return 1;
            });
        }
    }
}