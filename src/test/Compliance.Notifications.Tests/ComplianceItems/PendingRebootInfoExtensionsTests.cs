using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using Compliance.Notifications.Applic.PendingRebootCheck;
using Compliance.Notifications.Tests.Common;
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
            Assert.AreEqual(expected.Sources.Count, actual.Sources.Count,"Source Count");
            for (var i = 0; i < expected.Sources.Count; i++)
            {
                Assert.AreEqual(expected.Sources[i], actual.Sources[i],"Item is not equal: " + i);
            }
        }


        [Test]
        [Category(TestCategory.UnitTests)]
        public void UpdateTest_All_Are_True()
        {
            var r1 = new PendingRebootInfo {RebootIsPending = true, Sources = new List<RebootSource> {RebootSource.Cbs}};
            var r2 = new PendingRebootInfo { RebootIsPending = true, Sources = new List<RebootSource> { RebootSource.Wuau } };
            var r3 = new PendingRebootInfo { RebootIsPending = true, Sources = new List<RebootSource> { RebootSource.PendingFileRenameOperations } };
            var expected  = new PendingRebootInfo {RebootIsPending = true,Sources=new List<RebootSource> {RebootSource.Cbs, RebootSource.Wuau, RebootSource.PendingFileRenameOperations } };
            var actual = r1.Update(r2).Update(r3);
            AssertAreEqual(expected, actual);
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void UpdateTest_OneIsTrue_OneIsFalse_OneIsTrue()
        {
            var r1 = new PendingRebootInfo { RebootIsPending = true, Sources = new List<RebootSource> { RebootSource.Cbs } };
            var r2 = new PendingRebootInfo { RebootIsPending = false, Sources = new List<RebootSource> { RebootSource.Wuau } };
            var r3 = new PendingRebootInfo { RebootIsPending = true, Sources = new List<RebootSource> { RebootSource.PendingFileRenameOperations } };
            var expected = new PendingRebootInfo { RebootIsPending = true, Sources = new List<RebootSource> { RebootSource.Cbs, RebootSource.PendingFileRenameOperations } };
            var actual = r1.Update(r2).Update(r3);
            AssertAreEqual(expected, actual);
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void UpdateTest_OneIsFalse_OneIsFalse()
        {
            var r1 = new PendingRebootInfo { RebootIsPending = false, Sources = new List<RebootSource> { RebootSource.Cbs } };
            var r2 = new PendingRebootInfo { RebootIsPending = false, Sources = new List<RebootSource> { RebootSource.Wuau } };
            var r3 = new PendingRebootInfo { RebootIsPending = false, Sources = new List<RebootSource> { RebootSource.PendingFileRenameOperations } };
            var expected = new PendingRebootInfo { RebootIsPending = false, Sources = new List<RebootSource> { } };
            var actual = r1.Update(r2).Update(r3);
            AssertAreEqual(expected, actual);
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public async Task SaveLoadSystemComplianceItemResult_PendingRebootInfo_Test_Success()
        {
            var expected = new PendingRebootInfo { RebootIsPending = true, Sources = new List<RebootSource>() { RebootSource.Cbs, RebootSource.Wuau } };
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

        [Test]
        [Category(TestCategory.UnitTests)]
        public void RemoveSourceTest()
        {
            var r1 = new PendingRebootInfo { RebootIsPending = true, Sources = new List<RebootSource> { RebootSource.Cbs } };
            var r1bk = new PendingRebootInfo { RebootIsPending = true, Sources = new List<RebootSource> { RebootSource.Cbs } };
            var r2 = new PendingRebootInfo { RebootIsPending = true, Sources = new List<RebootSource> { RebootSource.Wuau } };
            var r2bk = new PendingRebootInfo { RebootIsPending = true, Sources = new List<RebootSource> { RebootSource.Wuau } };
            var r3 = new PendingRebootInfo { RebootIsPending = true, Sources = new List<RebootSource> { RebootSource.PendingFileRenameOperations } };
            var r3bk = new PendingRebootInfo { RebootIsPending = true, Sources = new List<RebootSource> { RebootSource.PendingFileRenameOperations } };
            var expected1 = new PendingRebootInfo { RebootIsPending = true, Sources = new List<RebootSource> { RebootSource.Wuau , RebootSource.PendingFileRenameOperations} };
            var actual1 = r1.Update(r2).Update(r3).RemoveSource(RebootSource.Cbs);
            AssertAreEqual(expected1, actual1);
            var expected2 = new PendingRebootInfo { RebootIsPending = true, Sources = new List<RebootSource> { RebootSource.PendingFileRenameOperations } };
            var actual2 = actual1.RemoveSource(RebootSource.Wuau);
            AssertAreEqual(expected2, actual2);
            var expected3 = new PendingRebootInfo { RebootIsPending = false, Sources = new List<RebootSource> { } };
            var actual3 = actual2.RemoveSource(RebootSource.PendingFileRenameOperations);
            AssertAreEqual(expected3, actual3);
            AssertAreEqual(r1, r1bk);//Verify that no mutation has occured.
            AssertAreEqual(r2, r2bk);
            AssertAreEqual(r3, r3bk);

        }
        
        [Test]
        [Category(TestCategory.UnitTests)]
        public void RemoveRebootSourcesTest1()
        {
            var r1 = new PendingRebootInfo { RebootIsPending = true, Sources = new List<RebootSource>(RebootSource.AllSources) };
            Assert.AreEqual(RebootSource.AllSources.Count,r1.Sources.Count);
            var expected = new PendingRebootInfo { RebootIsPending = false, Sources = new List<RebootSource>() };
            var actual = r1.RemoveRebootSources(RebootSource.AllSources);
            AssertAreEqual(expected, actual);
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void RemoveRebootSourcesTest2()
        {
            var r1 = new PendingRebootInfo { RebootIsPending = true, Sources = new List<RebootSource>(RebootSource.AllSources) };
            Assert.AreEqual(RebootSource.AllSources.Count, r1.Sources.Count);
            var expected = new PendingRebootInfo { RebootIsPending = true, Sources = new List<RebootSource> { RebootSource.Wuau, RebootSource.PendingFileRenameOperations, RebootSource.SccmClient, RebootSource.JoinDomain, RebootSource.RunOnce}};
            var actual = r1.RemoveRebootSources(RebootSource.AllSources.Where(source => source.Value.StartsWith("C")));
            AssertAreEqual(expected, actual);
        }
    }
}