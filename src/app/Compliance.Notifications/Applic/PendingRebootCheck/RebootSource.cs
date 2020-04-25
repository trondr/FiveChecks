using System;
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
        public static RebootSource Cbs => new RebootSource("Cbs", "Component Based Servicing");
        public static RebootSource Wuau => new RebootSource("Wuau", "Windows Update");
        public static RebootSource PendingFileRename => new RebootSource("PendingFileRename", "Pending File Rename Operations");
        public static RebootSource SccmClient => new RebootSource("SccmClient", "SCCM Client");
        public static RebootSource JoinDomain => new RebootSource("JoinDomain", "Join Domain");
        public static RebootSource ComputerNameRename => new RebootSource("ComputerNameRename", "Computer Rename");
        public static RebootSource RunOnce => new RebootSource("RunOnce", "Run Once");

        public static RebootSource StringToRebootSource(string value)
        {
            switch (value)
            {
                case "Cbs": return RebootSource.Cbs;
                case "Wuau": return RebootSource.Wuau;
                case "PendingFileRename": return RebootSource.PendingFileRename;
                case "SccmClient": return RebootSource.SccmClient;
                case "JoinDomain": return RebootSource.JoinDomain;
                case "ComputerNameRename": return RebootSource.ComputerNameRename;
                case "RunOne": return RebootSource.RunOnce;
                default:
                    throw new ArgumentException($"Invalid reboot source: {value}");
            }
        }

        public override string ToString()
        {
            return this.Description;
        }
    }
}