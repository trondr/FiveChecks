using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Compliance.Notifications.Commands;
using Compliance.Notifications.Common;
using Compliance.Notifications.ComplianceItems;
using LanguageExt.Common;
using NCmdLiner.Attributes;

namespace Compliance.Notifications
{
    public static class CommandDefinitions
    {
        [Command(Summary = "Install compliance notification utility.",
            Description = "Install compliance notification utility into the task scheduler.")]
        public static async Task<Result<int>> Install()
        {
            return await F.Install().ConfigureAwait(false);
        }


        [Command(Summary = "Uninstall compliance notification utility.",
            Description = "Uninstall compliance notification utility from the task scheduler.")]
        public static async Task<Result<int>> UnInstall()
        {
            return await F.UnInstall().ConfigureAwait(false);
        }


        [Command(Summary = "Check disk space compliance.", Description = "Check disk space. Disk space is compliant if: ((CurrentTotalFreeDiskSpace - requiredFreeDiskSpace) > 0. If 'subtractSccmCache' is set to true disk space will be compliant if: ((CurrentTotalFreeDiskSpace + CurrentSizeOfSccmCache) - requiredFreeDiskSpace) > 0")]
        // ReSharper disable once UnusedMember.Global
        public static async Task<Result<int>> CheckDiskSpace(
            [RequiredCommandParameter(Description = "Free disk space requirement in GB",AlternativeName = "fr", ExampleValue = 40)]
            decimal requiredFreeDiskSpace,
            [OptionalCommandParameter(Description = "Subtract current size of Sccm cache. When set to true, disk space is compliant if: ((CurrentTotalFreeDiskSpace + CurrentSizeOfSccmCache) - requiredFreeDiskSpace) > 0. This parameter is ignored on a client without Sccm Client.", AlternativeName = "ssc",ExampleValue = true,DefaultValue = false)]
            bool subtractSccmCache,
            [OptionalCommandParameter(Description = "Use a specific UI culture. F.example show user interface in Norwegian regardless of operating system display language.", AlternativeName = "uic",ExampleValue = "nb-NO",DefaultValue = "")]
            string userInterfaceCulture
            )
        {
            if (!string.IsNullOrEmpty(userInterfaceCulture))
            {
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(userInterfaceCulture);
            }
            return await CheckDiskSpaceCommand.CheckDiskSpace(requiredFreeDiskSpace, subtractSccmCache).ConfigureAwait(false);
        }
        
        [Command(Summary = "Handle activated toasts.", Description = "Handle activated toasts.")]
        public static async Task<Result<int>> ToastActivated()
        {
            Logging.DefaultLogger.Warn("ToastActivated : Not implemented!");
            await Task.Delay(1000).ConfigureAwait(false);
            return new Result<int>(0);
        }

        [Command(Summary = "Measure system compliance items.",Description = "Measure system compliance items (disk space,  pending reboot, system uptime, power up time, etc.) and write result to event log and to file system. System compliance measurements must be run in system context or with administrative privileges. Can be implemented as a scheduled task that the user has permission to execute.")]
        public static async Task<Result<int>> MeasureSystemComplianceItems()
        {
            var result = await SystemComplianceItems.Measurements.ExecuteComplianceMeasurements().ConfigureAwait(false);
            return result.Match(unit => new Result<int>(0), exception =>
            {
                Logging.DefaultLogger.Error($"Failed to measure system compliance items. {exception.ToExceptionMessage()}");
                return new Result<int>(1);
            });
        }

        [Command(Summary = "Measure user compliance items.", Description = "Measure user compliance items (data stored on desktop, etc.) and write result to event log and to file system. User compliance measurements must be run in user context.")]
        public static async Task<Result<int>> MeasureUserComplianceItems()
        {
            Logging.DefaultLogger.Warn("MeasureUserComplianceItems: NOT IMPLEMENTED");
            var num1 = toInt1("10");
            Logging.DefaultLogger.Info($"{num1}");
            var num2 = toInt2("20");
            Logging.DefaultLogger.Info($"{num2}");

            var types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsClass).Where(type => typeof(StringToInt).IsAssignableFrom(type)).ToArray();
            var instances = System.Reflection.Assembly.GetExecutingAssembly().DefinedTypes.SelectMany(typeInfo => typeInfo.DeclaredMembers.Where(memberInfo => typeof(StringToInt).IsAssignableFrom(memberInfo.ReflectedType))).ToArray();
            var methods = System.Reflection.Assembly.GetExecutingAssembly().DefinedTypes.Where(info => { return info.DeclaredMembers.Any(memberInfo => memberInfo.Name == "toInt1"); }).ToArray();
            var methods2 = System.Reflection.Assembly.GetExecutingAssembly().DefinedTypes
                .SelectMany(info => info.DeclaredMembers.Where(memberInfo => (memberInfo.MemberType == MemberTypes.Field) && (memberInfo.GetUnderlyingType() == typeof(StringToInt))))
                .ToArray();
            var fields = methods2.Select(info => info as FieldInfo).Select(info =>
                {
                    var field = typeof(StringToInt).GetField(info.Name, BindingFlags.Public | BindingFlags.Static);
                    var fieldValue = field.GetValue(null);
                    var method = fieldValue.GetType().GetMethod("Invoke");
                    method.Invoke(field, parameters: Array.Empty<object>());
                    return field;
                } );

            return await Task.FromResult(0).ConfigureAwait(false);
        }

        public static Type GetUnderlyingType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default:
                    throw new ArgumentException
                    (
                        "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                    );
            }
        }

        public delegate int StringToInt(string number);

        private static StringToInt toInt1 = ToInt;
        private static StringToInt toInt2 = ToInt;

        private static int ToInt(string number)
        {
            Logging.DefaultLogger.Info("TEST: Convert number to int");
            return Convert.ToInt32(number, CultureInfo.InvariantCulture);

        }
    }
}
