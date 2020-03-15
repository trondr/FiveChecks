using System;
using System.Diagnostics;
using System.IO;
using log4net;

namespace Compliance.Notifications.Common
{
    public static class Logging
    {
        private static readonly ILog _defaultLogger;

        public static ILog DefaultLogger => _defaultLogger ?? GetLogger("Default");


        /// <summary>
        /// Getting named logger
        /// </summary>
        /// <param name="name">Name of logger</param>
        /// <returns></returns>
        public static ILog GetLogger(string name)
        {
            var logFileName = LoggingConfiguration.LogFileName.AppendToFileName(name);
            var logFile = Path.Combine(LoggingConfiguration.LogDirectoryPath, logFileName);
            log4net.GlobalContext.Properties["LogFile"] = logFile;
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile));
            return LogManager.GetLogger(typeof(Program).Namespace);
        }

        public static void WriteErrorToEventLog(string message)
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
    }
}
