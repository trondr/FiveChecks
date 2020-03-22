using NUnit.Framework;
using Compliance.Notifications.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compliance.Notifications.Commands.Tests
{
    [TestFixture()]
    public class CommandDefinitionsTests
    {
        [Test()]
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