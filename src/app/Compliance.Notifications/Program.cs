using System;
using System.Threading.Tasks;
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
            AppDomain.CurrentDomain.UnhandledException += Logging.CurrentDomainOnUnhandledException;
            IApplicationInfo applicationInfo = new NCmdLiner.ApplicationInfo()
            {
                Authors = "github.com/trondr",
                Copyright = "Copyright © github/trondr 2020",
                Description = "Show compliance notifications to the user."
            };
            var returnValue = NCmdLiner.CmdLinery.Run(typeof(CommandDefinitions), args, applicationInfo, new NotepadMessenger());
            return returnValue;
        };

        static async Task<int> Main(string[] args)
        {
            Logging.DefaultLogger.Info($"Start: {ApplicationInfo.ApplicationName}.{ApplicationInfo.ApplicationVersion}. Command line: {Environment.CommandLine}");
            var exitCode = await TryRun(args).Match(Succ: i => i, Fail: exception => Logging.ErrorHandler(exception, 1)).ConfigureAwait(false);
            Logging.DefaultLogger.Info($"Stop: {ApplicationInfo.ApplicationName}.{ApplicationInfo.ApplicationVersion}. Exit code: {exitCode}");
            await Task.Delay(5000).ConfigureAwait(false);
            return exitCode;
        }
    }
}
