using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt.Common;
using DirectoryInfo = Pri.LongPath.DirectoryInfo;

namespace Compliance.Notifications.Applic
{
    public static class DesktopData
    {
        public static Task<Result<DesktopDataInfo>> GetDesktopDataInfo()
        {
            //get desktop folder
            var desktopDirectory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
            //get desktop files
            var allFiles = desktopDirectory.GetFiles("*.*", SearchOption.AllDirectories);
            var allNonShortcutFiles = 
                allFiles
                .Where(info => !info.Name.EndsWith(".lnk",StringComparison.InvariantCulture))
                .Where(info => !info.Name.EndsWith("desktop.ini", StringComparison.InvariantCulture))
                .ToArray();
            var numberOfAllNonShortcutFiles = allNonShortcutFiles.Length;
            var sizeofAllNonShortcutFilesInBytes = allNonShortcutFiles.Sum(info => info.Length);
            return Task.FromResult(new Result<DesktopDataInfo>(new DesktopDataInfo {HasDesktopData = numberOfAllNonShortcutFiles > 0, NumberOfFiles = numberOfAllNonShortcutFiles, TotalSizeInBytes = sizeofAllNonShortcutFilesInBytes}));
        }
    }
}