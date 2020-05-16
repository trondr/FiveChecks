using LanguageExt;

namespace Compliance.Notifications.Applic.PendingRebootCheck
{
    public class PendingFileRenameOperation
    {
        public PendingFileRenameOperation(Some<string> source, Option<string>  target)
        {
            if (string.IsNullOrWhiteSpace(source.Value))
                throw new ValueIsNullException("Source must be non-zero length string.");
            Source = source;
            Target = target.Match(s => string.IsNullOrWhiteSpace(s) ? Option<string>.None : s,() => Option<string>.None);
        }

        public Some<string> Source { get; }
        public Option<string> Target { get; }

        public PendingFileRenameOperationAction Action => Target.Match(s => PendingFileRenameOperationAction.Rename,
            () => PendingFileRenameOperationAction.Delete);
    }
}
