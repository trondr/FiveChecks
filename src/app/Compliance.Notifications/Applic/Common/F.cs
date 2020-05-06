using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Management.Automation;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Compliance.Notifications.Applic.PasswordExpiryCheck;
using Compliance.Notifications.Resources;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Win32;
using Newtonsoft.Json;
using Directory = Pri.LongPath.Directory;
using Path = Pri.LongPath.Path;
using Task = System.Threading.Tasks.Task;

namespace Compliance.Notifications.Applic.Common
{
    /// <summary>
    /// Common functions
    /// </summary>
    public static class F
    {
        public static readonly Random Rnd = new Random();

        public static string GetGreeting(Some<NotificationProfile> userProfile)
        {
            return $"{F.GetGreeting(DateTime.Now)} {userProfile.Value.GivenName}";
        }
        
        public const double Kb = 1024;
        public const double Mb = 1024L*1024L;
        public const double Gb = 1024L*1024L*1024L;
        public const double Tb = 1024L * 1024L * 1024L * 1024L;
        
        public static string BytesToReadableString(this long bytes)
        {
            if (bytes < Kb)
                return $"{bytes} B";
            if(bytes < Mb)
                return $"{(bytes/ Kb).FormatDouble(false)} KB";
            if (bytes < Gb)
                return $"{(bytes / Mb).FormatDouble(true)} MB";
            if (bytes < Tb)
                return $"{(bytes / Gb).FormatDouble(true)} GB";
            return $"{(bytes / Tb).FormatDouble(true)} TB";
        }

        public static string FormatDouble(this double d, bool useTwoDecimalPlaces)
        {
            return 
                useTwoDecimalPlaces ? 
                    string.Format(CultureInfo.InvariantCulture,"{0:0.0#}", d):
                    string.Format(CultureInfo.InvariantCulture, "{0:0}", d);
        }
        
        public static Try<Unit> DeleteFile(Some<string> fileName) => () =>
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
            return Unit.Default;
        };


        public static Result<IEnumerable<T>> ToResult<T>(this IEnumerable<Result<T>> results)
        {
            var e = new Exception(string.Empty);
            var resultArray = results.ToArray();
            var exceptions =
                resultArray
                    .Where(result => !result.IsSuccess)
                    .Select(result => result.Match(arg => e, exception => exception))
                    .ToArray();
            var values =
                resultArray
                    .Where(result => result.IsSuccess)
                    .Select(result => result.Match(v => v, exception => throw exception))
                    .ToArray();

            return exceptions.Length == 0
                ? new Result<IEnumerable<T>>(values)
                : new Result<IEnumerable<T>>(new AggregateException(exceptions));
        }

        //Source: https://github.com/WindowsNotifications/desktop-toasts
        public static async Task<Option<Uri>> DownloadImageToDisk(Some<Uri> httpImage)
        {
            // Toasts can live for up to 3 days, so we cache images for up to 3 days.
            // Note that this is a very simple cache that doesn't account for space usage, so
            // this could easily consume a lot of space within the span of 3 days.

            try
            {
                if (DesktopNotificationManagerCompat.CanUseHttpImages)
                {
                    return httpImage.Value;
                }

                var directory = Directory.CreateDirectory(Path.GetTempPath() + "github.com.trondr.Compliance.Notifications");
                foreach (var d in directory.EnumerateDirectories())
                {
                    if (d.CreationTimeUtc.Date < DateTime.UtcNow.Date.AddDays(-3))
                    {
                        d.Delete(true);
                    }
                }


                var dayDirectory = directory.CreateSubdirectory($"{DateTime.UtcNow.Day}");
                string imagePath = dayDirectory.FullName + "\\" + (uint)httpImage.Value.GetHashCode() + ".jpg";

                if (File.Exists(imagePath))
                {
                    return new Uri("file://" + imagePath);
                }

                using (var c = new HttpClient())
                {
                    using (var stream = await c.GetStreamAsync(httpImage.Value).ConfigureAwait(false))
                    {
                        using (var fileStream = File.OpenWrite(imagePath))
                        {
                            stream.CopyTo(fileStream);
                        }
                    }
                }
                return new Uri("file://" + imagePath);
            }
            catch (HttpRequestException) { return Option<Uri>.None; }
        }

        public static async Task<string> DownloadImage(Some<Uri> httpImage)
        {
            var image = await DownloadImageToDisk(httpImage).ConfigureAwait(false);
            return image.Match(uri => uri.LocalPath, () => "");
        }

        private static Option<string> _cacheFolder = Option<string>.None;
        public static Option<string> CacheFolder
        {
            get
            {
                _cacheFolder = _cacheFolder.Match(cacheFolder => cacheFolder, () =>
                {
                    var assembly = Assembly.GetEntryAssembly();
                    var exeDirectory = assembly != null ? new FileInfo(assembly.Location).Directory : Option<DirectoryInfo>.None;
                    return exeDirectory.Match(info => Path.Combine(info.FullName, "HeroImages"),() => Option<string>.None);
                });
                return _cacheFolder;
            }
        }

        public static async Task<Option<string>> GetRandomImageFromCache(Some<string> cacheFolder)
        {
            var rand = new Random();
            return await Task.Run(() =>
            {
                if(!Directory.Exists(cacheFolder.Value)) return  Option<string>.None;
                var imageFiles = Directory.GetFiles(cacheFolder.Value, "*.jpg");
                var imageFile = imageFiles.ElementAt(rand.Next(imageFiles.Length)); ;
                return $"file://{imageFile}";
            }).ConfigureAwait(false);
        }
        
        public static string GetGreeting(DateTime now)
        {
            var hourOfDay = now.Hour;
            if (hourOfDay > 0 && hourOfDay < 12)
                return strings.GoodMorning;
            if (hourOfDay >= 12 && hourOfDay < 16)
                return strings.GoodAfterNoon;
            return strings.GoodEvening;
        }

        

        public static IEnumerable<T> ToSuccess<T>(this IEnumerable<Result<T>> results)
        {
            var resultArray = results.ToArray();
            //Log any faulted results
            resultArray.Where(result => result.IsFaulted).Select(result =>result.IfFail(exception =>
            {
                Logging.DefaultLogger.Warn($"Failed to produce {typeof(T).Name} result. Error: {exception.GetType().Name}: {exception.ToExceptionMessage()}" );
            })).ToArr();
            //Return the successful results
            return
                resultArray
                    .Where(result => result.IsSuccess)
                    .Select(result => result.Match(v => v, exception => throw exception))
                    .ToArray();
        }

        public static Collection<PSObject> RunPowerShell(Some<Func<PowerShell, Collection<PSObject>>> action)
        {
            using (var powerShell = PowerShell.Create(RunspaceMode.NewRunspace))
            {
                return action.Value(powerShell);
            }
        }

        public static string ObjectToString(this object data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var objectType = data.GetType();
            if (objectType == typeof(string))
            {
                return $"\"{data}\"";
            }
            if (objectType.IsPrimitive || !objectType.IsClass)
            {
                return data.ToString();
            }
            return JsonConvert.SerializeObject(data);
        }

        public static void CloseOtherProcessWithSameCommandLine(this Process process)
        {
            if (process == null) throw new ArgumentNullException(nameof(process));
            var processId = process.Id;
            var processCommandLine = process.GetCommandLineWithoutPathAndArguments();
            Logging.DefaultLogger.Info($"Closing other processes with command line {processCommandLine}");
            var processesToClose = 
                Process.GetProcessesByName("Compliance.Notifications")
                .Where(p0 => p0.Id != processId)
                .Where(p1 => p1.GetCommandLineWithoutPathAndArguments() == processCommandLine)
                .Select(p2 =>
                {
                    Logging.DefaultLogger.Info($"Closing {p2.ProcessName} ({p2.Id})");
                    p2.CloseMainWindow();
                    p2.Kill();
                    return p2;
                })
                .ToArray();
        }

        private static bool HasSameCommandLine(string getCommandLineWithoutPath, string processCommandLine)
        {
            return getCommandLineWithoutPath == processCommandLine;
        }

        public static string GetCommandLine(this Process process)
        {
            if (process == null) throw new ArgumentNullException(nameof(process));
            using (var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
            using (var objects = searcher.Get())
            {
                return objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString();
            }
        }

        public static string GetCommandLineWithoutPathAndArguments(this Process process)
        {
            if (process == null) throw new ArgumentNullException(nameof(process));
            return StripPathAndArgumentsFromCommandLine(process.ProcessName, process.GetCommandLine());
        }

        public static string StripPathAndArgumentsFromCommandLine(string processName, string commandLine)
        {
            if (string.IsNullOrWhiteSpace(commandLine)) return string.Empty;
            var match = System.Text.RegularExpressions.Regex.Match(commandLine, "^.+(" + processName + "\\.exe).*?\\s+" + "(.+?)\\s.+$");
            return $"{match.Groups[1].Value} {match.Groups[2].Value}";
        }

        public static Unit OpenRestartDialog()
        {
            var temporaryRegistryValue = TemporaryRegistryValue.NewTemporaryRegistryValue(Registry.CurrentUser,
                @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_PowerButtonAction",
                RegistryValueKind.DWord, 4);
            return temporaryRegistryValue.Match(value =>
            {
                using (value)
                {
                    var shutdownVsb = new StringBuilder();
                    shutdownVsb.AppendLine("dim oShell");
                    shutdownVsb.AppendLine("set oShell = CreateObject(\"Shell.Application\")");
                    shutdownVsb.AppendLine("oShell.ShutdownWindows");
                    shutdownVsb.AppendLine("set oShell = nothing");
                    F.RunVbScript(shutdownVsb.ToString());
                    Task.Delay(1000).Wait();//Wait a little before removing temporary registry value
                }
                return Unit.Default;
            }, exception =>
            {
                Logging.DefaultLogger.Error($"Failed to change Start_PowerButtonAction. {exception.ToExceptionMessage()}");
                return Unit.Default;
            });
        }

        public static void RunVbScript(string vbScript)
        {
            using (var vbScriptFile = new TemporaryFile(".vbs"))
            {
                using (var sw = new StreamWriter(vbScriptFile.File.FullName))
                {
                    sw.Write(vbScript);
                }
                Process.Start(new ProcessStartInfo{FileName = "wscript.exe", Arguments = $"\"{vbScriptFile.File.FullName}\"", UseShellExecute = true})?.WaitForExit();
            }
        }
        
        public static Result<T> TryFunc<T>(Func<Result<T>> func)
        {
            Try<T> TryFun() => () => func();
            return TryFun().Try();
        }

        public static async Task<Result<T>> AsyncTryFunc<T>(Func<Task<Result<T>>> func)
        {
            TryAsync<T> TryFun() => async () => await func().ConfigureAwait(false);
            return await TryFun().Try().ConfigureAwait(false);
        }

        public static Result<T> TryFinally<T>(Func<Result<T>> tryFunc, System.Action finallyAction)
        {
            try
            {
                return TryFunc(tryFunc);
            }
            finally
            {
                finallyAction();
            }
        }

        public static async Task<Result<T>> AsyncTryFinally<T>(Func<Task<Result<T>>> tryFunc, System.Action finallyAction)
        {
            try
            {
                return await AsyncTryFunc(tryFunc).ConfigureAwait(false);
            }
            finally
            {
                finallyAction();
            }
        }

        public static async Task<Result<T>> AsyncPrepareTryFinally<T>(Func<Result<T>> prepareFunc, Func<Task<Result<T>>> tryFunc, Func<Result<T>> finallyAction)
        {
            if (prepareFunc == null) throw new ArgumentNullException(nameof(prepareFunc));
            try
            {
                Exception preparationException = null;
                var runTry = prepareFunc().Match(arg => true, exception =>
                {
                    preparationException = exception;
                    return false;
                });
                if (runTry)
                {
                    return await AsyncTryFunc(tryFunc).ConfigureAwait(false);
                }
                else
                {
                    return await Task.FromResult(new Result<T>(preparationException)).ConfigureAwait(false);
                }
            }
            finally
            {
                finallyAction();
            }
        }
        
        public static Result<Unit> DiskCleanup()
        {
            Process.Start(new ProcessStartInfo { FileName = "ms-settings:storagesense", UseShellExecute = true });
            return Unit.Default;
        }

        public static Result<Unit> DiskAutoCleanup()
        {
            throw new NotImplementedException();
        }
        
        public static Result<Unit> ChangePassword()
        {
            return F.TryFunc(() =>
            {
                PasswordExpire.ShowWindowsSecurityDialog(PasswordExpire.GetIsRemoteSession());
                return new Result<Unit>(Unit.Default);
            });
        }

        public static Result<Unit> DismissNotification()
        {
            Logging.DefaultLogger.Info("User dismissed the notification.");
            return new Result<Unit>(Unit.Default);
        }

        public static Result<Unit> CreateMyDocumentsShortcut()
        {
            var explorerExe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            
            //Create my OneDrive folder shortcut.
            var oneDriveShortcutPath = Path.Combine(desktopPath, $"{strings.OneDriveFolderName}.lnk");
            Option<string> oneDriveFolder = Environment.GetEnvironmentVariable("OneDrive");
            oneDriveFolder
                .IfSome(path =>
                {
                    F.CreateShortcut(oneDriveShortcutPath, $"\"{explorerExe}\"", $"/root,\"{path}\"", strings.OneDriveFolderDescription, true)
                        .IfSome(s => Process.Start(new ProcessStartInfo() { FileName = s }));
                });

            //Create my documents shortcut
            var myDocumentsShortcutPath = Path.Combine(desktopPath, $"{strings.MyDocumentsFolderName}.lnk");
            var myDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            F.CreateShortcut(myDocumentsShortcutPath, $"\"{explorerExe}\"", $"/root,\"{myDocumentsFolder}\"", strings.MyDocumentsFolderDescription, true)
                .IfSome(s => Process.Start(new ProcessStartInfo() { FileName = s }));

            //Create my desktop shortcut
            var myDesktopShortcutPath = Path.Combine(desktopPath, $"{strings.MyDesktopFolderName}.lnk");
            var myDesktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            F.CreateShortcut(myDesktopShortcutPath, $"\"{explorerExe}\"", $"/root,\"{myDesktopFolder}\"", strings.MyDesktopFolderDescription, true)
                .IfSome(s => Process.Start(new ProcessStartInfo() { FileName = s }));

            return new Result<Unit>(Unit.Default);
        }

        public static Option<string> CreateShortcut(Some<string> shortcutPath, Some<string> path,
            Option<string> arguments, Some<string> description, bool force)
        {
            if(File.Exists(shortcutPath.Value) && !force)
                return new Option<string>(shortcutPath);
            if (File.Exists(shortcutPath.Value))
                File.Delete(shortcutPath.Value);

            // ReSharper disable once SuspiciousTypeConversion.Global
            var link = new ShellLink() as IShellLink;
            // setup shortcut information
            link?.SetDescription(description.Value);
            link?.SetPath(path.Value);
            arguments.IfSome(a => link?.SetArguments(a));
            // save it
            // ReSharper disable once SuspiciousTypeConversion.Global
            var file = link as IPersistFile;
            file?.Save(shortcutPath, false);
            return !File.Exists(shortcutPath.Value) ? Option<string>.None : new Option<string>(shortcutPath);
        }

        
        
        public static async Task DownloadImages(IEnumerable<int> range)
        {
            var remaining = range.Select(async i =>
                {
                    var imageUri = new Uri($"https://picsum.photos/364/202?image={i}");
                    var downloaded = await F.DownloadImageToDisk(imageUri).ConfigureAwait(false);
                    var optionalImageIndex = downloaded.Match(uri =>
                    {
                        Logging.DefaultLogger.Info($"{i:0000}:{uri.LocalPath}");
                        return Option<int>.None;
                    }, () =>
                    {
                        Logging.DefaultLogger.Info($"{i:0000}: None");
                        return new Some<int>(i);
                    });
                    return optionalImageIndex;
                }).Select(task => task.Result)
                .Where(ints => ints.IsSome)
                .Select(ints => ints.Match(i => i,() => -1)).ToArray();
            if(remaining.Length > 0)
                await DownloadImages(remaining).ConfigureAwait(false);
        }
        
        public static bool IsOnline()
        {
            Try<bool> TryOnline() => () =>
            {
                using (var context = new PrincipalContext(ContextType.Domain))
                {
                    return true;
                }
            };
            return TryOnline().Try().Match(b => b, exception =>
            {
                Logging.DefaultLogger.Info($"Cannot contact active directory domain. {exception.ToExceptionMessage()}");
                return false;
            });
        }
    }
    
    public enum ComplianceAction
    {
        Notification,
        Measurement
    }
}
