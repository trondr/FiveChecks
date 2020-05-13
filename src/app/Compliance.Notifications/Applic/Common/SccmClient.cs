using System;
using System.ComponentModel;
using System.Diagnostics;
using LanguageExt;
using LanguageExt.Common;

namespace Compliance.Notifications.Applic.Common
{
    public static class SccmClient
    {
        public static Result<Unit> TriggerSchedule(SccmAction sccmAction)
        {
            var sccmActionString = EnumUtility.StringValueOf(sccmAction);
            var startInfo = new ProcessStartInfo
            {
                FileName = "wmic.exe",
                Arguments = $"path sms_client CALL TriggerSchedule \"{sccmActionString}\" /NOINTERACTIVE",
                UseShellExecute = true
            };
            Logging.DefaultLogger.Info($"Sccm client trigging sccm action '{sccmActionString}'");
            var process = Process.Start(startInfo);
            process?.WaitForExit();
            var exitCode = process?.ExitCode;
            if(exitCode.HasValue && exitCode.Value == 0)
                return new Result<Unit>(Unit.Default);
            if(exitCode.HasValue && exitCode.Value != 0)
                return new Result<Unit>(new Exception("WMIC returned exit code: " + exitCode));
            return new Result<Unit>(new Exception("WMIC failded to run (process is null)"));
        }
    }

    public enum SccmAction
    {
        [Description("{00000000-0000-0000-0000-000000000108}")]
        SoftwareUpdatesAgentAssignmentEvaluationCycle,
        [Description("{00000000-0000-0000-0000-000000000113}")]
        ForceUpdateScan,
    }
}
