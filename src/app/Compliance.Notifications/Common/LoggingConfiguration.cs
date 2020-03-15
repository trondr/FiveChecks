using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;

namespace Compliance.Notifications.Common
{
    public static class LoggingConfiguration
    {
        private static string _logDirectoryPath;
        private static string _logFileName;
        private static readonly string _sectionName = "Compliance.Notifications";

        public static string LogDirectoryPath
        {
            get
            {
                if (string.IsNullOrEmpty(_logDirectoryPath))
                {
                    var section = (NameValueCollection)ConfigurationManager.GetSection(_sectionName);
                    if (section == null)
                    {
                        throw new ConfigurationErrorsException("Missing section in application configuration file: " + _sectionName);
                    }
                    _logDirectoryPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(section["LogDirectoryPath"]));
                }
                return _logDirectoryPath;
            }
        }

        public static string LogFileName
        {
            get
            {
                if (string.IsNullOrEmpty(_logFileName))
                {
                    var section = (NameValueCollection)ConfigurationManager.GetSection(_sectionName);
                    if (section == null)
                    {
                        throw new ConfigurationErrorsException("Missing section in application configuration file: " + _sectionName);
                    }
                    _logFileName = Environment.ExpandEnvironmentVariables(section["LogFileName"]);
                }
                return _logFileName;
            }
        }
    }
}
