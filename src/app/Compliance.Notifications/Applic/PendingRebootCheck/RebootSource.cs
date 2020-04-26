using System;
using System.Collections.Immutable;
using Compliance.Notifications.Applic.Common;
using LanguageExt;

namespace Compliance.Notifications.Applic.PendingRebootCheck
{
    public class RebootSource: Record<RebootSource>
    {
        private RebootSource(string value, string description)
        {
            Value = value;
            Description = description;
        }
        public string Value { get; set; }
        public string Description { get; set; }
        public const string CbsName = "Cbs";
        public const string WuauName = "Wuau";
        public const string PendingFileRenameOperationsName = "PendingFileRenameOperations";
        public const string SccmClientName = "SccmClient";
        public const string JoinDomainName = "JoinDomain";
        public const string ComputerNameRenameName = "ComputerNameRename";
        public const string RunOnceName = "RunOnce";

        public static RebootSource Cbs => new RebootSource(CbsName, "Component Based Servicing");
        public static RebootSource Wuau => new RebootSource(WuauName, "Windows Update");
        public static RebootSource PendingFileRenameOperations => new RebootSource(PendingFileRenameOperationsName, "Pending File Rename Operations");
        public static RebootSource SccmClient => new RebootSource(SccmClientName, "SCCM Client");
        public static RebootSource JoinDomain => new RebootSource(JoinDomainName, "Join Domain");
        public static RebootSource ComputerNameRename => new RebootSource(ComputerNameRenameName, "Computer Rename");
        public static RebootSource RunOnce => new RebootSource(RunOnceName, "Run Once");

        public static ImmutableList<RebootSource> AllSources => ImmutableList<RebootSource>.Empty.AddRange(new[] { Cbs, Wuau, PendingFileRenameOperations, SccmClient, JoinDomain, ComputerNameRename, RunOnce });

        public static RebootSource StringToRebootSource(string value)
        {
            switch (value)
            {
                case CbsName: return RebootSource.Cbs;
                case WuauName: return RebootSource.Wuau;
                case PendingFileRenameOperationsName: return RebootSource.PendingFileRenameOperations;
                case SccmClientName: return RebootSource.SccmClient;
                case JoinDomainName: return RebootSource.JoinDomain;
                case ComputerNameRenameName: return RebootSource.ComputerNameRename;
                case RunOnceName: return RebootSource.RunOnce;
                default:
                    throw new ArgumentException($"Invalid reboot source: {value}");
            }
        }

        public override string ToString()
        {
            return this.Description;
        }
    }

    public static class RebootSourceExtensions
    {
        public static bool IsDisabled(this RebootSource rebootSource)
        {
            var category = typeof(CheckPendingRebootCommand).GetPolicyCategory();
            var disabledValueName = rebootSource.GetDisabledValueName();
            var isDisabled = F.GetBooleanPolicyValue(Context.Machine, category, disabledValueName, false);
            if(isDisabled) Logging.DefaultLogger.Debug($"Reboot source '{rebootSource}' is disabled");
            return isDisabled;
        }

        public static string GetDisabledValueName(this RebootSource rebootSource)
        {
            if (rebootSource == null) throw new ArgumentNullException(nameof(rebootSource));
            switch (rebootSource.Value)
            {
                case RebootSource.CbsName: return "DisableRebootSourceComponentBasedServicing";
                case RebootSource.WuauName: return "DisableRebootSourceWindowsUpdate";
                case RebootSource.PendingFileRenameOperationsName: return "DisableRebootSourcePendingFileRenameOperations";
                case RebootSource.SccmClientName: return "DisableRebootSourceSccmClient";
                case RebootSource.JoinDomainName: return "DisableRebootSourceJoinDomain";
                case RebootSource.ComputerNameRenameName: return "DisableRebootSourceComputerNameRename";
                case RebootSource.RunOnceName: return "DisableRebootSourceRunOnce";
                default:
                    throw new ArgumentException($"Invalid reboot source: {rebootSource.Value}");
            }
        }
    }
}