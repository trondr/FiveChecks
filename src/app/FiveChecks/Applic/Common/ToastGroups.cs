using System.Collections.Generic;
using LanguageExt;
using Microsoft.QueryStringDotNET;

namespace FiveChecks.Applic.Common
{
    public static class ToastGroups
    {
        public const string CheckDiskSpace = "CheckDiskSpace";
        public const string CheckPendingReboot = "CheckPendingReboot";
        public const string CheckPasswordExpiry = "CheckPasswordExpiry";
        public const string CheckSystemUptime = "CheckSystemUptime";
        public const string CheckDesktopData = "CheckDesktopData";
        public const string CheckMissingMsUpdates = "CheckMissingMsUpdates";

        public static List<string> Groups { get; } = new List<string>
        {
            ToastGroups.CheckDiskSpace,
            ToastGroups.CheckPendingReboot,
            ToastGroups.CheckPasswordExpiry,
            ToastGroups.CheckSystemUptime,
            ToastGroups.CheckDesktopData,
            ToastGroups.CheckMissingMsUpdates
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