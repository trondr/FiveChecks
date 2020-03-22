using System.Threading.Tasks;
using Compliance.Notifications.Commands.CheckDiskSpace;
using Compliance.Notifications.Common;
using LanguageExt.Common;
using NCmdLiner.Attributes;

namespace Compliance.Notifications.Commands
{
    public static class CommandDefinitions
    {
        [Command(Summary = "Check disk space compliance.", Description = "Check disk space. Disk space is compliant if: ((CurrentTotalFreeDiskSpace - requiredFreeDiskSpace) > 0. If 'subtractSccmCache' is set to true disk space will be compliant if: ((CurrentTotalFreeDiskSpace + CurrentSizeOfSccmCache) - requiredFreeDiskSpace) > 0")]
        // ReSharper disable once UnusedMember.Global
        public static async Task<Result<int>> CheckDiskSpace(
            [RequiredCommandParameter(Description = "Free disk space requirement in GB",AlternativeName = "fr", ExampleValue = 40)]
            decimal requiredFreeDiskSpace,
            [OptionalCommandParameter(Description = "Subtract current size of Sccm cache. When set to true, disk space is compliant if: ((CurrentTotalFreeDiskSpace + CurrentSizeOfSccmCache) - requiredFreeDiskSpace) > 0. This parameter is ignored on a client without Sccm Client.", AlternativeName = "ssc",ExampleValue = true,DefaultValue = false)]
            bool subtractSccmCache
            )
        {
            return await CheckDiskSpaceCommand.CheckDiskSpace(requiredFreeDiskSpace, subtractSccmCache).ConfigureAwait(false);
        }
        
        [Command(Summary = "Handle activated toasts.", Description = "Handle activated toasts.")]
        public static async Task<Result<int>> ToastActivated()
        {
            Logging.DefaultLogger.Warn("ToastActivated : Not implemented!");
            await Task.Delay(1000).ConfigureAwait(false);
            return new Result<int>(0);
        }
    }
}
