using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.Common;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Win32;

namespace Compliance.Notifications.Applic.DiskSpaceCheck
{
    public static class DiskCleanup
    {
        public static async Task<Result<Unit>> RunFullDiskCleanup()
        {
            return await F.AsyncPrepareTryFinally(
                () => { return SetCleanupManagerStateFlags(); },
                async () => await StartCleanMgrProcessAsync().ConfigureAwait(false),
                () => { return ResetCleanupManagerStateFlags();}
            ).ConfigureAwait(false);
        }

        private static async Task<Result<Unit>> StartCleanMgrProcessAsync()
        {
            return await Task.Run(() =>
            {
                return F.TryFunc(() => { return StartCleanMgrProcess(); });
            }).ConfigureAwait(false);
        }

        private static Result<Unit> StartCleanMgrProcess()
        {
            Logging.DefaultLogger.Info("Running CleanMrg.exe");
            var process = Process.Start(new ProcessStartInfo { FileName = "cleanmgr.exe", Arguments = "/SAGERUN:200 /d c: /Y", UseShellExecute = true });
            process?.WaitForExit();
            var exitCode = process?.ExitCode ?? 1;
            Logging.DefaultLogger.Info($"Finished running CleanMrg.exe. Exit code: {exitCode}");
            return exitCode == 0 ? new Result<Unit>(Unit.Default) : new Result<Unit>(new Exception($"CleanMgr.exe exited with exit code '{exitCode}'"));
        }

        /// <summary>
        /// Set the StateFlags0200=0x00000002 value for each defined volume cache registry key as preparation for running CleanMgr.exe
        /// </summary>
        /// <returns></returns>
        public static Result<Unit> SetCleanupManagerStateFlags()
        {
            Logging.DefaultLogger.Info("Preparing volume cache state flags for running CleanMrg.exe");
            return 
                VolumeCachesKeyPaths
                .Select(subKeyPath => RegistryOperations.SetRegistryValue(Registry.LocalMachine, subKeyPath, StateFlagsValueName, 2, RegistryValueKind.DWord))
                .ToArray()
                .ToResult()
                .Match(units =>
                {
                    Logging.DefaultLogger.Info("Successfully prepared volume cache state flags.");
                    return new Result<Unit>(Unit.Default);
                }, exception =>
                {
                    Logging.DefaultLogger.Error($"Failed preparing volume cache state flags. {exception.ToExceptionMessage()}");
                    return new Result<Unit>(new Exception($"Failed to execute function {nameof(SetCleanupManagerStateFlags)}", exception));
                });
        }

        /// <summary>
        /// Remove the StateFlags0200 value for each defined volume cache registry key
        /// </summary>
        /// <returns></returns>
        public static Result<Unit> ResetCleanupManagerStateFlags()
        {
            Logging.DefaultLogger.Info("Resetting volume cache state flags after running CleanMrg.exe");
            return 
                VolumeCachesKeyPaths
                    .Select(subKeyPath => RegistryOperations.DeleteRegistryValue(Registry.LocalMachine, subKeyPath, StateFlagsValueName))
                    .ToArray()
                    .ToResult()
                    .Match(units =>
                    {
                        Logging.DefaultLogger.Info("Successfully reset volume cache state flags.");
                        return new Result<Unit>(Unit.Default);
                    },exception =>
                    {
                        Logging.DefaultLogger.Error($"Failed resetting volume cache state flags. {exception.ToExceptionMessage()}");
                        return new Result<Unit>(new Exception($"Failed to execute function {nameof(ResetCleanupManagerStateFlags)}", exception));
                    });
        }
        private const string StateFlagsValueName = "StateFlags0200";
        private static readonly List<string> VolumeCachesKeyPaths = new List<string>
        {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\Active Setup Temp Folders",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\BranchCache",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\Content Indexer Cleaner",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\D3D Shader Cache",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\Delivery Optimization Files",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\Device Driver Packages",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\Downloaded Program Files",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\GameNewsFiles",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\GameStatisticsFiles",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\GameUpdateFiles",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\Internet Cache Files",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\Offline Pages Files",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\Old ChkDsk Files",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\Previous Installations",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\Recycle Bin",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\RetailDemo Offline Content",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\Service Pack Cleanup",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\Setup Log Files",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\System error memory dump files",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\System error minidump files",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\Temporary Files",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\Temporary Setup Files",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\Temporary Sync Files",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\Thumbnail Cache",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\Update Cleanup",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\Upgrade Discarded Files",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\User file versions",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\Windows Defender",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\Windows Error Reporting Files",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\Windows ESD installation files",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\Windows Upgrade Log Files",
            };
    }
}
