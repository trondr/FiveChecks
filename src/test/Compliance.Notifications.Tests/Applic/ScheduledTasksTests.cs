using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using Compliance.Notifications.Tests.Common;
using NUnit.Framework;

namespace Compliance.Notifications.Tests.Applic
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