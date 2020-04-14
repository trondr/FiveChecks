using System;
using LanguageExt;

namespace Compliance.Notifications.Module
{
    public class RebootSource: Record<RebootSource>
    {
        private RebootSource(string value) { Value = value; }
        public string Value { get; set; }
        public static RebootSource Cbs => new RebootSource("Cbs");
        public static RebootSource Wuau => new RebootSource("Wuau");
        public static RebootSource PendingFileRename => new RebootSource("PendingFileRename");
        public static RebootSource SccmClient => new RebootSource("SccmClient");
        public static RebootSource JoinDomain => new RebootSource("JoinDomain");
        public static RebootSource ComputerNameRename => new RebootSource("ComputerNameRename");
        public static RebootSource RunOnce => new RebootSource("RunOnce");

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
    }
}