using System;
using System.Collections.Generic;
using System.Linq;
using Compliance.Notifications.Applic.Common;
using LanguageExt;
using Microsoft.Win32;

namespace Compliance.Notifications.Applic.PendingRebootCheck
{
    public class PendingFileRenameOperationDto
    {
        public string Source { get; set; }
        public string Target { get; set; }
        public PendingFileRenameOperationAction Action { get; set; }
    }

    public enum PendingFileRenameOperationAction
    {
        Delete,
        Rename
    }
    
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

    public static class PendingFileRenameOperationExtensions
    {
        public static PendingFileRenameOperationDto ToDto(this PendingFileRenameOperation pendingFileRenameOperation)
        {
            if (pendingFileRenameOperation == null) throw new ArgumentNullException(nameof(pendingFileRenameOperation));
            return new PendingFileRenameOperationDto {Source = pendingFileRenameOperation.Source.Value,Target = pendingFileRenameOperation.Target.Match(s => s,() => string.Empty),Action = pendingFileRenameOperation.Action};
        }

        public static PendingFileRenameOperation FromDto(this PendingFileRenameOperationDto pendingFileRenameOperationDto)
        {
            if (pendingFileRenameOperationDto == null) throw new ArgumentNullException(nameof(pendingFileRenameOperationDto));
            return new PendingFileRenameOperation(pendingFileRenameOperationDto.Source,pendingFileRenameOperationDto.Target);
        }

        public static IEnumerable<PendingFileRenameOperation> ToPendingFileRenameOperations(this string[] pendingFileRenameOperationsStringArray)
        {
            if (pendingFileRenameOperationsStringArray == null) throw new ArgumentNullException(nameof(pendingFileRenameOperationsStringArray));
            if (pendingFileRenameOperationsStringArray.Length % 2 != 0)throw new ArgumentException("Invalid pending file rename operations string array. Length of the array must be an even number.");

            for (var i = 0; i < pendingFileRenameOperationsStringArray.Length-1; i = i+2)
            {
                var source = pendingFileRenameOperationsStringArray[i];
                var target = pendingFileRenameOperationsStringArray[i+1];
                yield return new PendingFileRenameOperation(source, target);
            }
        }

        public static PendingFileRenameOperation[] GetPendingFileRenameOperations()
        {
            var registryKeyPath = @"SYSTEM\CurrentControlSet\Control\Session Manager";
            var registryValueName = "PendingFileRenameOperations";
            var pendingFileRenameOperationsStringArray = RegistryOperations.GetMultiStringRegistryValue(Registry.LocalMachine, registryKeyPath, registryValueName);
            return pendingFileRenameOperationsStringArray.ToPendingFileRenameOperations().ToArray();
        }
    }

    public static class PendingFileRenameOperations
    {

    }

}
