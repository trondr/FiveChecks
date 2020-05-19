using NUnit.Framework;
using FiveChecks.Applic.Common;
using FiveChecks.Tests.Applic;

namespace FiveChecks.Tests
{
    [TestFixture()]
    public class EnumUtilityTests
    {
        [Test]
        [Category(TestCategory.UnitTests)]
        public void StringValueOfTest_ForceUpdateScan()
        {
            var actualOption = EnumUtility.StringValueOf(SccmAction.ForceUpdateScan);
            var expectedTriggerId = "{00000000-0000-0000-0000-000000000113}";
            actualOption.Match(actual =>
            {
                Assert.IsTrue(expectedTriggerId == actual, "Trigger id not equal");
            }, () =>
            {
                Assert.Fail("Did not expect None");
            });
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void StringValueOfTest_SoftwareUpdatesAgentAssignmentEvaluationCycle()
        {
            var actualOption = EnumUtility.StringValueOf(SccmAction.SoftwareUpdatesAgentAssignmentEvaluationCycle);
            var expectedTriggerId = "{00000000-0000-0000-0000-000000000108}";
            actualOption.Match(actual =>
            {
                Assert.IsTrue(expectedTriggerId == actual, "Trigger id not equal");
            }, () =>
            {
                Assert.Fail("Did not expect None");
            });
        }
    }
}