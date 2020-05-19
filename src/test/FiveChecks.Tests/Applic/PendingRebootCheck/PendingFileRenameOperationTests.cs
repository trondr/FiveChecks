using FiveChecks.Applic.PendingRebootCheck;
using LanguageExt;
using NUnit.Framework;

namespace FiveChecks.Tests.Applic.PendingRebootCheck
{
    [TestFixture]
    public class PendingFileRenameOperationTests
    {
        [Test]
        [Category(TestCategory.UnitTests)]
        public void PendingFileRenameOperationTest_SourceIsNull_ValueNullException()
        {
            Assert.Throws<ValueIsNullException>(() =>
            {
                var actual = new PendingFileRenameOperation(null, null);
            });
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void PendingFileRenameOperationTest_SourceIsSome_TargetIsNull()
        {
            var source = "C:\\Temp\\test123.tmp";
            string target = null;
            var actual = new PendingFileRenameOperation(source, target);
            Assert.AreEqual(source, actual.Source.Value);
            Assert.AreEqual(Option<string>.None, actual.Target);
            Assert.AreEqual(PendingFileRenameOperationAction.Delete, actual.Action);
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void PendingFileRenameOperationTest_SourceIsSome_TargetIsEmpty()
        {
            var source = "C:\\Temp\\test123.tmp";
            string target = string.Empty;
            var actual = new PendingFileRenameOperation(source, target);
            Assert.AreEqual(source, actual.Source.Value);
            Assert.AreEqual(Option<string>.None, actual.Target);
            Assert.AreEqual(PendingFileRenameOperationAction.Delete, actual.Action);
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void PendingFileRenameOperationTest_SourceIsSome_TargetIsNonNull()
        {
            var source = "C:\\Temp\\test123.tmp";
            string target = "C:\\Temp\\test1234.tmp";
            var actual = new PendingFileRenameOperation(source, target);
            Assert.AreEqual(source, actual.Source.Value);
            Assert.AreEqual(target, actual.Target.Match(s => s,() =>
            {
                Assert.Fail("Did not expect None");
                return string.Empty;
            }));
            Assert.AreEqual(PendingFileRenameOperationAction.Rename, actual.Action);
        }
    }
}