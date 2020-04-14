using System;
using System.Collections.Generic;
using Compliance.Notifications.Common;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.QueryStringDotNET;

namespace Compliance.Notifications.Module
{
    public static class ToastActions
    {
        public const string Restart = "restart";
        public const string DiskCleanup = "diskcleanup";
        public const string DiskAutoCleanup = "diskautocleanup";
        public const string ChangePassword = "changepassword";
        public const string Dismiss = "dismiss";

        public static Dictionary<string, Func<Result<Unit>>> Actions { get; } =
            new Dictionary<string, Func<Result<Unit>>>
            {
                {ToastActions.Restart, () => F.TryFunc<Unit>(() => F.OpenRestartDialog())},
                {ToastActions.DiskCleanup, () => F.TryFunc<Unit>(() => F.DiskCleanup())},
                {ToastActions.DiskAutoCleanup, () => F.TryFunc<Unit>(() => F.DiskAutoCleanup())},
                {ToastActions.ChangePassword, () => F.TryFunc<Unit>(() => F.ChangePassword())},
                {ToastActions.Dismiss, () => F.TryFunc<Unit>(() => F.DismissNotification())},
            };

        public static Option<Func<Result<Unit>>> ParseToastActionArguments(string arguments)
        {
            if (string.IsNullOrWhiteSpace(arguments))
            {
                Logging.DefaultLogger.Info("Toast was activated without arguments.");
                return Option<Func<Result<Unit>>>.None;
            }
            var args = QueryString.Parse(arguments);
            if (!args.Contains("action"))
            {
                Logging.DefaultLogger.Warn("Toast was activated without 'action' argument.");
                return Option<Func<Result<Unit>>>.None;
            }
            var action = args["action"];
            if (Actions.ContainsKey(action))
            {
                return Actions[action];
            }
            Logging.DefaultLogger.Info("Unknown action.");
            return Option<Func<Result<Unit>>>.None;
        }
    }
}