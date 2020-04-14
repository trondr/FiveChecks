using System.Collections.Generic;
using Compliance.Notifications.Common;
using LanguageExt;
using Microsoft.QueryStringDotNET;

namespace Compliance.Notifications.Module
{
    public static class ToastGroups
    {
        public const string CheckDiskSpace = "CheckDiskSpace";
        public const string CheckPendingReboot = "CheckPendingReboot";
        public const string CheckPasswordExpiry = "CheckPasswordExpiryReboot";
        public const string CheckSystemUptime = "CheckSystemUptime";


        public static List<string> Groups { get; } = new List<string>
        {
            ToastGroups.CheckDiskSpace,
            ToastGroups.CheckPendingReboot,
            ToastGroups.CheckPasswordExpiry,
            ToastGroups.CheckSystemUptime
        };

        public static Option<string> ParseToastGroupArguments(string arguments)
        {
            if (string.IsNullOrWhiteSpace(arguments))
            {
                Logging.DefaultLogger.Info("Toast was activated without arguments.");
                return Option<string>.None;
            }
            var args = QueryString.Parse(arguments);
            if (!args.Contains("group"))
            {
                Logging.DefaultLogger.Warn("Toast was activated without 'group' argument.");
                return Option<string>.None;
            }
            return args["group"];
        }
    }
}