using System;
using System.Threading.Tasks;
using Compliance.Notifications.Commands;
using Compliance.Notifications.Common;
using LanguageExt;
using NCmdLiner;
using ApplicationInfo = Compliance.Notifications.Common.ApplicationInfo;

namespace Compliance.Notifications
{
    class Program
    {
        private static TryAsync<int> TryRun(string[] args) => () =>
        {
            IApplicationInfo applicationInfo = new NCmdLiner.ApplicationInfo()
            {
                Authors = "github.com/trondr",
                Copyright = "Copyright © github/trondr 2020",
                Description = "Show compliance notifications to the user."
            };
            var returnValue = NCmdLiner.CmdLinery.Run(typeof(CommandDefinitions), args, applicationInfo, new NotepadMessenger());
            return returnValue;
        };

        private static int ErrorHandler(Exception ex, int exitCode)
        {
            Logging.WriteErrorToEventLog($"ERROR: {ex}");
            return exitCode;
        }

        static async Task<int> Main(string[] args)
        {
            Logging.DefaultLogger.Info($"Start: {ApplicationInfo.ApplicationName}.{ApplicationInfo.ApplicationVersion}. Command line: {Environment.CommandLine}");
            var exitCode = await TryRun(args).Match(Succ: i => i, Fail: exception => ErrorHandler(exception,1)).ConfigureAwait(false);
            Logging.DefaultLogger.Info($"Stop: {ApplicationInfo.ApplicationName}.{ApplicationInfo.ApplicationVersion}. Exit code: {exitCode}");
            return exitCode;
        }
    }
}
