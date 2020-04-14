// File: ApplicationInfoHelper.cs
// Project Name: NCmdLiner
// Project Home: https://github.com/trondr/NCmdLiner/blob/master/README.md
// License: New BSD License (BSD) https://github.com/trondr/NCmdLiner/blob/master/License.md
// Credits: See the Credit folder in this project
// Copyright © <github.com/trondr> 2013 
// All rights reserved.

using System;
using System.IO;
using System.Reflection;
using NCmdLiner;

namespace Compliance.Notifications.Applic.Common
{
    public static class ApplicationInfo
    {
        private static Assembly AppAssembly
        {
            get
            {
                if (_appAssembly == null)
                {
                    _appAssembly = GetAssembly();
                }
                return _appAssembly;
            }
        }

        private static Assembly GetAssembly()
        {
            var assembly = Assembly.GetEntryAssembly() ?? typeof(ApplicationInfo).GetAssembly();
            return assembly;
        }

        private static Assembly _appAssembly;

        private static string _applicationName;
        /// <summary>
        /// Get exe file path
        /// </summary>
        public static string ApplicationName
        {
            get
            {
                if (string.IsNullOrEmpty(_applicationName))
                {
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(GetExeFilePath());
                    if (fileNameWithoutExtension != null)
                        _applicationName =
                            fileNameWithoutExtension.Replace(".Gui", "").Replace(".Console", "").Replace('.', ' ');
                }
                return _applicationName;
            }
        }

        private static string _applicationVersion;
        /// <summary>
        /// Get application version
        /// </summary>
        public static string ApplicationVersion
        {
            get
            {
                if (string.IsNullOrEmpty(_applicationVersion))
                {
                    if (AppAssembly.GetCustomAttributeEx(typeof(AssemblyInformationalVersionAttribute)) is AssemblyInformationalVersionAttribute informationalVersionAttribute)
                    {
                        _applicationVersion = informationalVersionAttribute.InformationalVersion;
                    }
                    if (string.IsNullOrEmpty(_applicationVersion))
                    {
                        _applicationVersion = AppAssembly.GetName().Version.ToString();
                    }
                }
                return _applicationVersion;
            }
        }

        private static string _exeFilePath;

        /// <summary>
        /// Get exe file path
        /// </summary>
        public static string GetExeFilePath()
        {
            if (string.IsNullOrEmpty(_exeFilePath))
            {
                _exeFilePath = AppAssembly.Location;
                if (!File.Exists(_exeFilePath))
                {
                    throw new FileNotFoundException("Could not find exe file path: " + _exeFilePath);
                }
            }
            return _exeFilePath;
        }

        // ReSharper disable once UnusedMember.Global
        public static string ApplicationCopyright
        {
            get
            {
                if (string.IsNullOrEmpty(_applicationCopyright))
                {
                    var attribute = AppAssembly.GetCustomAttributeEx(typeof(AssemblyCopyrightAttribute));
                    if (attribute != null)
                    {
                        var copyRightAttribute = (AssemblyCopyrightAttribute)attribute;
                        _applicationCopyright = copyRightAttribute.Copyright;
                    }
                    else
                    {
                        _applicationCopyright = "<Not Set>";
                    }
                }
                return _applicationCopyright;
            }

        }
        private static string _applicationCopyright;

        // ReSharper disable once UnusedMember.Global
        public static string ApplicationDescription
        {
            get
            {
                if (string.IsNullOrEmpty(_applicationDescription))
                {
                    var attribute = AppAssembly.GetCustomAttributeEx(typeof(AssemblyDescriptionAttribute));
                    if (attribute != null)
                    {
                        var descriptionAttribute = (AssemblyDescriptionAttribute)attribute;
                        _applicationDescription = descriptionAttribute.Description;
                    }
                    else
                    {
                        _applicationDescription = "<Not Set>";
                    }
                }
                return _applicationDescription;
            }
        }
        private static string _applicationDescription;

        public static Attribute GetCustomAttributeEx(this Assembly assembly, Type attributeType)
        {
            return Attribute.GetCustomAttribute(assembly, attributeType, false);
        }
    }
}