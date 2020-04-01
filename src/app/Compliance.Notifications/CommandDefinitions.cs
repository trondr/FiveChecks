using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Compliance.Notifications.Commands;
using Compliance.Notifications.Common;
using Compliance.Notifications.Model;
using LanguageExt.Common;
using NCmdLiner.Attributes;

namespace Compliance.Notifications
{
    public static class CommandDefinitions
    {
        [Command(Summary = "Install compliance notification utility.",
            Description = "Install compliance notification utility into the task scheduler.")]
        public static async Task<Result<int>> Install()
        {
            return await F.Install().ConfigureAwait(false);
        }


        [Command(Summary = "Uninstall compliance notification utility.",
            Description = "Uninstall compliance notification utility from the task scheduler.")]
        public static async Task<Result<int>> UnInstall()
        {
            return await F.UnInstall().ConfigureAwait(false);
        }


        [Command(Summary = "Check disk space compliance.", Description = "Check disk space. Disk space is compliant if: ((CurrentTotalFreeDiskSpace - requiredFreeDiskSpace) > 0. If 'subtractSccmCache' is set to true disk space will be compliant if: ((CurrentTotalFreeDiskSpace + CurrentSizeOfSccmCache) - requiredFreeDiskSpace) > 0")]
        // ReSharper disable once UnusedMember.Global
        public static async Task<Result<int>> CheckDiskSpace(
            [RequiredCommandParameter(Description = "Free disk space requirement in GB",AlternativeName = "fr", ExampleValue = 40)]
            decimal requiredFreeDiskSpace,
            [OptionalCommandParameter(Description = "Subtract current size of Sccm cache. When set to true, disk space is compliant if: ((CurrentTotalFreeDiskSpace + CurrentSizeOfSccmCache) - requiredFreeDiskSpace) > 0. This parameter is ignored on a client without Sccm Client.", AlternativeName = "ssc",ExampleValue = true,DefaultValue = false)]
            bool subtractSccmCache,
            [OptionalCommandParameter(Description = "Use a specific UI culture. F.example show user interface in Norwegian regardless of operating system display language.", AlternativeName = "uic",ExampleValue = "nb-NO",DefaultValue = "")]
            string userInterfaceCulture
            )
        {
            if (!string.IsNullOrEmpty(userInterfaceCulture))
            {
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(userInterfaceCulture);
            }
            Process.GetCurrentProcess().CloseOtherProcessWithSameCommandLine();
            var exitCodeResult = new Result<int>(1);
            App.RunApplicationOnStart(async (sender, args) =>
            {
                exitCodeResult = await CheckDiskSpaceCommand.CheckDiskSpace(requiredFreeDiskSpace, subtractSccmCache).ConfigureAwait(false);
            });
            return await Task.FromResult(exitCodeResult).ConfigureAwait(false);
        }

        [Command(Summary = "Check pending reboot compliance.", Description = "Check pending reboot compliance.")]
        public static async Task<Result<int>> CheckPendingReboot(
            [OptionalCommandParameter(Description = "Use a specific UI culture. F.example show user interface in Norwegian regardless of operating system display language.", AlternativeName = "uic",ExampleValue = "nb-NO",DefaultValue = "")]
            string userInterfaceCulture
            )
        {
            if (!string.IsNullOrEmpty(userInterfaceCulture))
            {
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(userInterfaceCulture);
            }
            Process.GetCurrentProcess().CloseOtherProcessWithSameCommandLine();
            var exitCodeResult = new Result<int>(1);
            App.RunApplicationOnStart(async (sender, args) =>
            {
                exitCodeResult = await CheckPendingRebootCommand.CheckPendingReboot().ConfigureAwait(false);
            });
            return await Task.FromResult(exitCodeResult).ConfigureAwait(false);
        }
        
        [Command(Summary = "Handle activated toasts.", Description = "Handle activated toasts.")]
        public static async Task<Result<int>> ToastActivated()
        {
            var eventArgs = AppInstance.GetActivatedEventArgs();
            Logging.DefaultLogger.Warn($"ToastActivated : Not implemented! Event args: {eventArgs.ObjectToString()}");
            await Task.Delay(1000).ConfigureAwait(false);
            return new Result<int>(0);
        }

        [Command(Summary = "Measure system compliance items.",Description = "Measure system compliance items (disk space,  pending reboot, system uptime, power up time, etc.) and write result to event log and to file system. System compliance measurements must be run in system context or with administrative privileges. Can be implemented as a scheduled task that the user has permission to execute.")]
        public static async Task<Result<int>> MeasureSystemComplianceItems()
        {
            var result = await SystemComplianceItems.Measurements.ExecuteComplianceMeasurements().ConfigureAwait(false);
            return result.Match(unit => new Result<int>(0), exception =>
            {
                Logging.DefaultLogger.Error($"Failed to measure system compliance items. {exception.ToExceptionMessage()}");
                return new Result<int>(1);
            });
        }

        [Command(Summary = "Measure user compliance items.", Description = "Measure user compliance items (data stored on desktop, etc.) and write result to event log and to file system. User compliance measurements must be run in user context.")]
        public static async Task<Result<int>> MeasureUserComplianceItems()
        {
            Logging.DefaultLogger.Warn("MeasureUserComplianceItems: NOT IMPLEMENTED");
            return await Task.FromResult(0).ConfigureAwait(false);
        }
    }
}
