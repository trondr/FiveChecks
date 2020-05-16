using System;
using System.Linq;
using Compliance.Notifications.Applic.PendingRebootCheck;
using LanguageExt;
using NUnit.Framework;

namespace Compliance.Notifications.Tests.Applic.PendingRebootCheck
{
    [TestFixture]
    public class PendingFileRenameOperationExtensionsTests
    {
        [Test]
        [Category(TestCategory.UnitTests)]
        public void ToDtoTest_Some_Some()
        {
            var actual = new PendingFileRenameOperation("somes","somet").ToDto();
            Assert.AreEqual("somes",actual.Source,"Source");
            Assert.AreEqual("somet", actual.Target, "Target");
        }

        [Test]
        [Category(TestCategory.UnitTests)]
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
                @"\??\C: \temp\test2.exe",
                @"\??\C: \temp\test2.exe",
                @"\??\C: \temp\test3.exe",
                @"\??\C: \temp\test4.exe",
                @"",
                @"\??\C: \temp\test5.exe",
                @"\??\C: \temp\test6.exe",
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
                @"\??\C:\temp\test.exe",
                @"\??\C: \temp\test2.exe",
                @"\??\C: \temp\test2.exe",
                @"\??\C: \temp\test3.exe",
                @"\??\C: \temp\test4.exe",
                @"",
                @"\??\C: \temp\test5.exe",
                @"\??\C: \temp\test6.exe",
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
                @"\??\C:\temp\test.exe",
                @"\??\C: \temp\test2.exe",
                @"\??\C: \temp\test2.exe",
                @"\??\C: \temp\test3.exe",
                @"",
                @"",
                @"\??\C: \temp\test5.exe",
                @"\??\C: \temp\test6.exe",
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


        [Test]
        [Category(TestCategory.UnitTests)]
        public void ToRegExPatternsTest_Empty_Patterns_Array()
        {
            var patterns = Array.Empty<string>();
            var actual = patterns.ToRegExPatterns().ToArray();
            Assert.AreEqual(0,actual.Length,"Number of regular expressions");
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void ToRegExPatternsTest_NonEmpty_Patterns_Array_With_EmptyPatterns()
        {
            var patterns = new string[]{"","",""};
            var actual = patterns.ToRegExPatterns().ToArray();
            Assert.AreEqual(3, actual.Length, "Number of regular expressions");
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void ToRegExPatternsTest_NonEmpty_Patterns_Array_With_NullPatterns()
        {
            var patterns = new string[] { null, null, null };
            var actual = patterns.ToRegExPatterns().ToArray();
            Assert.AreEqual(0, actual.Length, "Number of regular expressions");
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void ToRegExPatternsTest_NonEmpty_Patterns_Array_With_InvlidPatterns()
        {
            var patterns = new[] { "]", "[", "(" };
            var actual = patterns.ToRegExPatterns().ToArray();
            Assert.AreEqual(1, actual.Length, "Number of regular expressions");
        }

        public static string[] TestData = {
            @"??\C:\WINDOWS\system32\spool\DRIVERS\x64\3\New\MXDWDRV.DLL",
            @"??\C:\WINDOWS\system32\spool\DRIVERS\x64\3\MXDWDRV.DLL",
            @"??\C:\WINDOWS\system32\spool\DRIVERS\x64\3\New\PJLMON.DLL",
            @"??\C:\WINDOWS\system32\spool\DRIVERS\x64\3\PJLMON.DLL",
            @"??\C:\WINDOWS\system32\spool\DRIVERS\x64\3\New\PS5UI.DLL",
            @"??\C:\WINDOWS\system32\spool\DRIVERS\x64\3\PS5UI.DLL",
            @"??\C:\WINDOWS\system32\spool\DRIVERS\x64\3\New\PSCRIPT5.DLL",
            @"??\C:\WINDOWS\system32\spool\DRIVERS\x64\3\PSCRIPT5.DLL",
            @"??\C:\WINDOWS\system32\spool\DRIVERS\x64\3\New\UNIDRV.DLL",
            @"??\C:\WINDOWS\system32\spool\DRIVERS\x64\3\UNIDRV.DLL",
            @"??\C:\WINDOWS\system32\spool\DRIVERS\x64\3\New\UNIDRVUI.DLL",
            @"??\C:\WINDOWS\system32\spool\DRIVERS\x64\3\UNIDRVUI.DLL",
            @"??\C:\WINDOWS\system32\spool\DRIVERS\x64\3\New\UNIRES.DLL",
            @"??\C:\WINDOWS\system32\spool\DRIVERS\x64\3\UNIRES.DLL",
            @"??\C:\WINDOWS\system32\spool\V4Dirs\81C1C79C-D629-44C4-8E7E-22B0A24184D7\233ad48d.gpd",
            @"",
            @"??\C:\WINDOWS\system32\spool\PRTPROCS\x64\1_hpcpp225.dll",
            @"",
            @"??\C:\WINDOWS\system32\spool\DRIVERS\W32X86\3\New\mxdwdrv.dll",
            @"??\C:\WINDOWS\system32\spool\DRIVERS\W32X86\3\mxdwdrv.dll",
            @"??\C:\WINDOWS\system32\spool\DRIVERS\W32X86\3\New\PrintConfig.dll",
            @"??\C:\WINDOWS\system32\spool\DRIVERS\W32X86\3\PrintConfig.dll",
            @"??\C:\WINDOWS\system32\spool\DRIVERS\x64\3\New\tsprint.dll",
            @"??\C:\WINDOWS\system32\spool\DRIVERS\x64\3\tsprint.dll",
            @"??\C:\WINDOWS\system32\spool\DRIVERS\x64\3\New\PrintConfig.dll",
            @"??\C:\WINDOWS\system32\spool\DRIVERS\x64\3\PrintConfig.dll",
            @"??\C:\WINDOWS\system32\spool\drivers\x64\3\Old\1\PrintConfig.dll",
            @"",
            @"??\C:\WINDOWS\system32\spool\drivers\x64\3\Old\1\PrintConfig.dll",
            @"",
        };

        [Test]
        [Category(TestCategory.UnitTests)]
        public void ExcludeTest_Exclude_Patterns_Matching_44C4_()
        {
            var operations = TestData.ToPendingFileRenameOperations().ToArray();
            var excludeRenameTargets = false;
            var excludeDeleteTargets = false;
            var excludePatterns = new[] { "44C4-" }.ToRegExPatterns().ToArray();
            var actual = operations.Exclude(excludeRenameTargets, excludeDeleteTargets, excludePatterns).Select(operation => operation.ToDto()).ToArray();
            Assert.AreEqual(14, actual.Length, "Number of Pending File Rename Operations");
        }


        [Test]
        [Category(TestCategory.UnitTests)]
        public void ExcludeTest_Exclude_Patterns_Matching_3_MXD()
        {
            var operations = TestData.ToPendingFileRenameOperations().ToArray();
            var excludeRenameTargets = false;
            var excludeDeleteTargets = false;
            var excludePatterns = new[] { "3\\\\MXD" }.ToRegExPatterns().ToArray();
            var actual = operations.Exclude(excludeRenameTargets, excludeDeleteTargets, excludePatterns).Select(operation => operation.ToDto()).ToArray();
            Assert.AreEqual(13, actual.Length, "Number of Pending File Rename Operations");
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void ExcludeTest_Exclude_Patterns_Matching_tsprint()
        {
            var operations = TestData.ToPendingFileRenameOperations().ToArray();
            var excludeRenameTargets = false;
            var excludeDeleteTargets = false;
            var excludePatterns = new[] { "tsprint" }.ToRegExPatterns().ToArray();
            var actual = operations.Exclude(excludeRenameTargets, excludeDeleteTargets, excludePatterns).Select(operation => operation.ToDto()).ToArray();
            Assert.AreEqual(14, actual.Length, "Number of Pending File Rename Operations");
        }


        [Test]
        [Category(TestCategory.UnitTests)]
        public void ExcludeTest_Exclude_Patterns_Matching_All()
        {
            var operations = TestData.ToPendingFileRenameOperations().ToArray();
            var excludeRenameTargets = false;
            var excludeDeleteTargets = false;
            var excludePatterns = new[]{"spool"}.ToRegExPatterns().ToArray();
            var actual = operations.Exclude(excludeRenameTargets, excludeDeleteTargets, excludePatterns).Select(operation => operation.ToDto()).ToArray();
            Assert.AreEqual(0, actual.Length, "Number of Pending File Rename Operations");
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void ExcludeTest_Exclude_Patterns_Matching_All_IgnoreCase()
        {
            var operations = TestData.ToPendingFileRenameOperations().ToArray();
            var excludeRenameTargets = false;
            var excludeDeleteTargets = false;
            var excludePatterns = new[] { "Spool" }.ToRegExPatterns().ToArray();
            var actual = operations.Exclude(excludeRenameTargets, excludeDeleteTargets, excludePatterns).Select(operation => operation.ToDto()).ToArray();
            Assert.AreEqual(0, actual.Length, "Number of Pending File Rename Operations");
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void ExcludeTest_Do_Not_Exclude()
        {
            var operations = TestData.ToPendingFileRenameOperations().ToArray();
            var excludeRenameTargets = false;
            var excludeDeleteTargets = false;
            var excludePatterns = Array.Empty<string>().ToRegExPatterns().ToArray();
            var actual = operations.Exclude(excludeRenameTargets, excludeDeleteTargets, excludePatterns).Select(operation => operation.ToDto()).ToArray();
            Assert.AreEqual(15, actual.Length, "Number of Pending File Rename Operations");
        }
        
        [Test]
        [Category(TestCategory.UnitTests)]
        public void ExcludeTest_Exclude_Delete_And_Rename_Targets()
        {
            var operations = TestData.ToPendingFileRenameOperations().ToArray();
            var excludeRenameTargets = true;
            var excludeDeleteTargets = true;
            var excludePatterns = Array.Empty<string>().ToRegExPatterns().ToArray();
            var actual = operations.Exclude(excludeRenameTargets, excludeDeleteTargets, excludePatterns).Select(operation => operation.ToDto()).ToArray();
            Assert.AreEqual(0,actual.Length,"Number of Pending File Rename Operations");
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void ExcludeTest_Exclude_Delete_Targets()
        {
            var operations = TestData.ToPendingFileRenameOperations().ToArray();
            var excludeRenameTargets = false;
            var excludeDeleteTargets = true;
            var excludePatterns = Array.Empty<string>().ToRegExPatterns().ToArray();
            var actual = operations.Exclude(excludeRenameTargets, excludeDeleteTargets, excludePatterns).Select(operation => operation.ToDto()).ToArray();
            Assert.AreEqual(11, actual.Length, "Number of Pending File Rename Operations");
        }

        [Test]
        [Category(TestCategory.UnitTests)]
        public void ExcludeTest_Exclude_Rename_Targets()
        {
            var operations = TestData.ToPendingFileRenameOperations().ToArray();
            var excludeRenameTargets = true;
            var excludeDeleteTargets = false;
            var excludePatterns = Array.Empty<string>().ToRegExPatterns().ToArray();
            var actual = operations.Exclude(excludeRenameTargets, excludeDeleteTargets, excludePatterns).Select(operation => operation.ToDto()).ToArray();
            Assert.AreEqual(4, actual.Length, "Number of Pending File Rename Operations");
        }
        
    }
}