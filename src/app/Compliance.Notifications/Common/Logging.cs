using System;
using System.Diagnostics;
using System.IO;
using LanguageExt;
using LanguageExt.Common;
using log4net;

namespace Compliance.Notifications.Common
{
    public static class Logging
    {

        internal static ILog DefaultLogger => GetLogger("Default");

        /// <summary>
        /// Getting named logger
        /// </summary>
        /// <param name="name">Name of logger</param>
        /// <returns></returns>
        internal static ILog GetLogger(string name)
        {
            var logFileName = LoggingConfiguration.GetLogFileName().Match(f => f.AppendNameToFileName(name),exception => throw exception);
            var logFile = LoggingConfiguration.GetLogDirectoryPath().Match(d => Path.Combine(d, logFileName), exception => throw exception);
            log4net.GlobalContext.Properties["LogFile"] = logFile;
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile));
            return LogManager.GetLogger(typeof(Program).Namespace);
        }

        internal static void WriteErrorToEventLog(string message)
        {
            // ReSharper disable once RedundantNameQualifier
            System.Console.WriteLine(message);
            const string logName = "Application";
            using (var eventLog = new EventLog(logName))
            {
                eventLog.Source = logName;
                eventLog.WriteEntry(message, EventLogEntryType.Error, 10001, 1);
            }
        }

        internal static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ErrorHandler(e.ExceptionObject as Exception, 1);
        }

        internal static Try<int> TryErrorLogging(Exception ex) => () =>
        {
            Logging.DefaultLogger.Error(ex);
            return new Result<int>(1);
        };

        internal static int ErrorHandler(Exception ex, int exitCode)
        {
            Logging.WriteErrorToEventLog($"ERROR: {ex}");
            TryErrorLogging(ex).IfFail(e =>
            {
                Logging.WriteErrorToEventLog($"LOGGING ERROR: {e}");
                return exitCode;
            });
            return exitCode;
        }
    }
}
