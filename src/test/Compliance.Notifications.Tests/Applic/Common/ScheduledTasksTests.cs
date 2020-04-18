using NUnit.Framework;
using System.Threading.Tasks;
using Compliance.Notifications.Tests.Common;

namespace Compliance.Notifications.Applic.Common.Tests
{
    [TestFixture]
    public class ScheduledTasksTests
    {
        [Test]
        [Category(TestCategory.ManualTests)]
        public async Task RunScheduledTaskTest()
        {
            var actual = await ScheduledTasks.RunScheduledTask(ScheduledTasks.ComplianceUserMeasurements,true);
            var r= actual.Match(
                unit =>
                {
                    Assert.IsTrue(true, "Success was not expected.");
                    return "Success";
                },
                exception =>
                {
                    Assert.IsFalse(true, "Failed with: " + exception.ToExceptionMessage());
                    return "Fail";
                }
                );
        }
    }
}