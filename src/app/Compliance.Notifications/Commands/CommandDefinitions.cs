using System.Threading.Tasks;
using Compliance.Notifications.Commands.CheckDiskSpace;
using LanguageExt.Common;
using NCmdLiner.Attributes;

namespace Compliance.Notifications.Commands
{

    public static class CommandDefinitions
    {
        [Command(Summary = "Show example notification.",Description = "Show example notification.")]
        // ReSharper disable once UnusedMember.Global
        public static async Task<Result<int>> ShowNotification(
            [RequiredCommandParameter(Description = "Arguments",AlternativeName = "a", ExampleValue = new[]{"SomeArg1","SomeArg2"})]
            string[] args)
        {
            return await CheckDiskSpaceCommand.CheckDiskSpace(args).ConfigureAwait(false);
        }
    }
}
