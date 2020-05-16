using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Compliance.Notifications.Applic.Common;
using LanguageExt;
using Microsoft.Win32;

namespace Compliance.Notifications.Applic.PendingRebootCheck
{
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

        public static Option<Regex> ToRegEx(this string pattern)
        {
            try
            {
                return new Regex(pattern,RegexOptions.IgnoreCase);
            }
            catch (ArgumentException e)
            {
                Logging.DefaultLogger.Warn($"Invalid exclude regex '{pattern}'. {e.ToExceptionMessage()}");
                return Option<Regex>.None;
            }
        }

        public static IEnumerable<Regex> ToRegExPatterns(this IEnumerable<string> patterns)
        {
            if (patterns == null) throw new ArgumentNullException(nameof(patterns));
            return patterns
                .Select(s => s.ToRegEx())
                .Where(option => option.IsSome)
                .Select(option => option.Match(regex => regex,() => throw new InvalidOperationException("Regex was None. Should never happen! Developer!!!")));
        }

        public static IEnumerable<PendingFileRenameOperation> Exclude(this IEnumerable<PendingFileRenameOperation> pendingFileRenameOperations, bool excludeRenameTargets, bool excludeDeleteTargets, Regex[] excludePatternsArray)
        {
            return pendingFileRenameOperations
                .Where(dto => !(excludeDeleteTargets && dto.Action == PendingFileRenameOperationAction.Delete))
                .Where(dto => !(excludeRenameTargets && dto.Action == PendingFileRenameOperationAction.Rename))
                .Where(dto => !(excludePatternsArray.Any(regex => regex.IsMatch(dto.Source)) || excludePatternsArray.Any(regex => dto.Target.Match(s => regex.IsMatch(s),() => false))));
        }
    }
}