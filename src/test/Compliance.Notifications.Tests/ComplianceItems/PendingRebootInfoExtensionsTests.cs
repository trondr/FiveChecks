using System.Collections.Generic;
using System.Threading.Tasks;
using Compliance.Notifications.Applic;
using Compliance.Notifications.Applic.Common;
using Compliance.Notifications.Common;
using Compliance.Notifications.Common.Tests;
using LanguageExt;
using NUnit.Framework;

namespace Compliance.Notifications.Tests.ComplianceItems
{
    [TestFixture()]
    public class PendingRebootInfoExtensionsTests
    {
        public void AssertAreEqual(PendingRebootInfo expected, PendingRebootInfo actual)
        {
            Assert.AreEqual(expected.RebootIsPending, actual.RebootIsPending, "RebootIsPending");
            Assert.AreEqual(expected.Source.Count, actual.Source.Count,"Source Count");
            for (var i = 0; i < expected.Source.Count; i++)
            {
                Assert.AreEqual(expected.Source[i], actual.Source[i],"Item is not equal: " + i);
            }
        }


        [Test]
        [Category(TestCategory.UnitTests)]
        public void UpdateTest_All_Are_True()
        {
            var r1 = new PendingRebootInfo {RebootIsPending = true, Source = new List<RebootSource> {RebootSource.Cbs}};
            var r2 = new PendingRebootInfo { RebootIsPending = true, Source = new List<RebootSource> { RebootSource.Wuau } };
            var r3 = new PendingRebootInfo { RebootIsPending = true, Source = new List<RebootSource> { RebootSource.PendingFileRename } };
            var expected  = new PendingRebootInfo {RebootIsPending = true,Source=new List<RebootSource> {RebootSource.Cbs, RebootSource.Wuau, RebootSource.PendingFileRename } };
            var actual = r1.Update(r2).Update(r3);
            AssertAreEqual(expected, actual);
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void UpdateTest_OneIsTrue_OneIsFalse_OneIsTrue()
        {
            var r1 = new PendingRebootInfo { RebootIsPending = true, Source = new List<RebootSource> { RebootSource.Cbs } };
            var r2 = new PendingRebootInfo { RebootIsPending = false, Source = new List<RebootSource> { RebootSource.Wuau } };
            var r3 = new PendingRebootInfo { RebootIsPending = true, Source = new List<RebootSource> { RebootSource.PendingFileRename } };
            var expected = new PendingRebootInfo { RebootIsPending = true, Source = new List<RebootSource> { RebootSource.Cbs, RebootSource.PendingFileRename } };
            var actual = r1.Update(r2).Update(r3);
            AssertAreEqual(expected, actual);
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void UpdateTest_OneIsFalse_OneIsFalse()
        {
            var r1 = new PendingRebootInfo { RebootIsPending = false, Source = new List<RebootSource> { RebootSource.Cbs } };
            var r2 = new PendingRebootInfo { RebootIsPending = false, Source = new List<RebootSource> { RebootSource.Wuau } };
            var r3 = new PendingRebootInfo { RebootIsPending = false, Source = new List<RebootSource> { RebootSource.PendingFileRename } };
            var expected = new PendingRebootInfo { RebootIsPending = false, Source = new List<RebootSource> { } };
            var actual = r1.Update(r2).Update(r3);
            AssertAreEqual(expected, actual);
        }

        [Test()]
        [Category(TestCategory.UnitTests)]
        public async Task SaveLoadSystemComplianceItemResult_PendingRebootInfo_Test_Success()
        {
            var expected = new PendingRebootInfo { RebootIsPending = true, Source = new List<RebootSource>() { RebootSource.Cbs, RebootSource.Wuau } };
            var savedResult = await F.SaveSystemComplianceItemResult<PendingRebootInfo>(expected).ConfigureAwait(false);
            var actual = await F.LoadSystemComplianceItemResult<PendingRebootInfo>().ConfigureAwait(false);
            actual.Match(act =>
            {
                AssertAreEqual(expected, act);
                return Unit.Default;
            }, exception =>
            {
                Assert.Fail(exception.ToExceptionMessage());
                return Unit.Default;
            });

        }
    }
}