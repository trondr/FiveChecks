using System;
using System.Linq;
using Windows.Devices.Sensors;
using Compliance.Notifications.Applic.PendingRebootCheck;
using LanguageExt;
using NUnit.Framework;

namespace Compliance.Notifications.Tests.Applic.PendingRebootCheck
{
    [TestFixture]
    public class PendingFileRenameOperationExtensionsTests
    {
        [Test]
        public void ToDtoTest_Some_Some()
        {
            var actual = new PendingFileRenameOperation("somes","somet").ToDto();
            Assert.AreEqual("somes",actual.Source,"Source");
            Assert.AreEqual("somet", actual.Target, "Target");
        }

        [Test]
        public void ToDtoTest_Some_None()
        {
            var actual = new PendingFileRenameOperation("somes", Option<string>.None).ToDto();
            Assert.AreEqual("somes", actual.Source, "Source");
            Assert.AreEqual(string.Empty,actual.Target, "Target");
        }
        
        [Test]
        [Category(TestCategory.UnitTests)]
        public void ToPendingFileRenameOperationsArrayTest_InvalidArray_OddNumberOfEntries()
        {
            var testArray = new string[]
            {
                @"\??\C: \Users\eta410\Downloads\PendMoves\test2.exe",
                @"\??\C: \Users\eta410\Downloads\PendMoves\test2.exe",
                @"\??\C: \Users\eta410\Downloads\PendMoves\test3.exe",
                @"\??\C: \Users\eta410\Downloads\PendMoves\test4.exe",
                @"",
                @"\??\C: \Users\eta410\Downloads\PendMoves\test5.exe",
                @"\??\C: \Users\eta410\Downloads\PendMoves\test6.exe",
            };
            Assert.Throws<ArgumentException>(() =>
            {
                var actual = testArray.ToPendingFileRenameOperations().ToArray();
            });
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void ToPendingFileRenameOperationsArrayTest_ValidNonEmptyArray()
        {
            var testArray = new string[]
            {
                @"\??\C:\Users\eta410\Downloads\PendMoves\test.exe",
                @"\??\C: \Users\eta410\Downloads\PendMoves\test2.exe",
                @"\??\C: \Users\eta410\Downloads\PendMoves\test2.exe",
                @"\??\C: \Users\eta410\Downloads\PendMoves\test3.exe",
                @"\??\C: \Users\eta410\Downloads\PendMoves\test4.exe",
                @"",
                @"\??\C: \Users\eta410\Downloads\PendMoves\test5.exe",
                @"\??\C: \Users\eta410\Downloads\PendMoves\test6.exe",
            };
            var actual = testArray.ToPendingFileRenameOperations().ToArray();
            Assert.AreEqual(4, actual.Length,"Length");
            Assert.AreEqual(1,actual.Count(o => o.Action == PendingFileRenameOperationAction.Delete),"Count of delete actions");
            Assert.AreEqual(3, actual.Count(o => o.Action == PendingFileRenameOperationAction.Rename), "Count of rename actions");
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void ToPendingFileRenameOperationsArrayTest_InValidNonEmptyArray_SourceIsEmpty()
        {
            var testArray = new string[]
            {
                @"\??\C:\Users\eta410\Downloads\PendMoves\test.exe",
                @"\??\C: \Users\eta410\Downloads\PendMoves\test2.exe",
                @"\??\C: \Users\eta410\Downloads\PendMoves\test2.exe",
                @"\??\C: \Users\eta410\Downloads\PendMoves\test3.exe",
                @"",
                @"",
                @"\??\C: \Users\eta410\Downloads\PendMoves\test5.exe",
                @"\??\C: \Users\eta410\Downloads\PendMoves\test6.exe",
            };
            Assert.Throws<ValueIsNullException>(() =>
            {
                var actual = testArray.ToPendingFileRenameOperations().ToArray();
            });
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void ToPendingFileRenameOperationsArrayTest_ValidEmptyArray()
        {
            var testArray = new string[]{};
            var actual = testArray.ToPendingFileRenameOperations().ToArray();
            Assert.AreEqual(0, actual.Length);
        }

        [Test()]
        [Category(TestCategory.ManualTests)]
        public void GetPendingFileRenameOperationsTest()
        {
            var actual = PendingFileRenameOperationExtensions.GetPendingFileRenameOperations();
            Assert.AreEqual(4, actual.Length, "Length");
        }
    }
}