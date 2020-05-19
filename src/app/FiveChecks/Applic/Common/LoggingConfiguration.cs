using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using LanguageExt;

namespace FiveChecks.Applic.Common
{
    public static class LoggingConfiguration
    {
        private const string SectionName = "FiveChecks";

        public static Try<string> GetLogDirectoryPath() => () =>
        {
            var section = (NameValueCollection)ConfigurationManager.GetSection(SectionName);
            if (section == null)
            {
                throw new ConfigurationErrorsException("Missing section in application configuration file: " + SectionName);
            }
            var logDirectoryPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(section["LogDirectoryPath"]));
            return Directory.CreateDirectory(logDirectoryPath).FullName;
        };

        public static Try<string> GetLogFileName() => () =>
        {
            var section = (NameValueCollection)ConfigurationManager.GetSection(SectionName);
            if (section == null)
            {
                throw new ConfigurationErrorsException("Missing section in application configuration file: " + SectionName);
            }
            var logFileName = Environment.ExpandEnvironmentVariables(section["LogFileName"]);
            return logFileName;
        };
    }
}
