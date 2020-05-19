namespace FiveChecks.Applic.PendingRebootCheck
{
    public class PendingFileRenameOperationDto
    {
        public string Source { get; set; }
        public string Target { get; set; }
        public PendingFileRenameOperationAction Action { get; set; }
    }
}