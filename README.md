# Compliance-Notifications

Measure compliance items (such as free disk space, pending reboot, system uptime, missing updates) and write result to file system.

Show toast notification to the user when measurements are not within compliance range.

## Notifications

The following notifications are currently supported.

|Check|Description|Notification|
|---|---|---|
|Pending Reboot| Notify the user if the machine has a pending reboot. Four reboot sources are checked for pending reboot. Any of the reboot sources can be excluded from the check through configuration. This notification can all together be disabled. ![GitHub Logo](/doc/images/PendingReboot_Config.png) The pending file rename operations source can be fined tuned with three different filters to exclude specific operations. ![GitHub Logo](/doc/images/PendingReboot_Config2.png) | ![GitHub Logo](/doc/images/PendingReboot.png) |
|Missing MS Updates| Notify the user if there are MS updates waiting to be installed by the SCCM client and the configured wait hours have elapsed. Default wait hours are 48 hours. This notification can be disabled. ![GitHub Logo](/doc/images/MissingMsUpdates_Config.png) | ![GitHub Logo](/doc/images/MissingMsUpdates.png) |
|System Uptime| Notify the user if the computer uptime exceeds the maximum configured uptime hours. This notification can be disabled. ![GitHub Logo](/doc/images/Uptime_Config.png) | ![GitHub Logo](/doc/images/Uptime.png) |
|Disk space| Notify the user if free disk space is below the configured required free disk space value. This notification can be disabled. ![GitHub Logo](/doc/images/DiskSpaceIsLow_Config.png) | ![GitHub Logo](/doc/images/DiskSpaceIsLow.png) |
|Desktop Data| Notify the user if files other than shortcuts are found on the Desktop. Encourage user to store documents and data in My Documents or OneDrive. This notification can be disabled. ![GitHub Logo](/doc/images/DesktopData_Config.png) | ![GitHub Logo](/doc/images/DesktopData.png) |
|Password Expiry| Notify the user when password is soon to expire. The password expiry warning days are configurable. This notification can be disabled. ![GitHub Logo](/doc/images/PasswordExpiry_Config.png) | ![GitHub Logo](/doc/images/PasswordExpiry.png) |

## Localization

Resource files for the following languages have been added to the Compliance.Notification project and more languages can be added. Translation help is in any case needed.

|Language|Culture Code|Status|
|---|---|---|
|English|en-US|&#x2705;|
|Norwegian|no-NB|&#x2705;|
|Swedish|sv-SE|&#x274C;|
|Finnish|fi-FI|&#x274C;|

Resource files are translated using Visual Studio Extension ResX Resource Manager (https://github.com/dotnet/ResXResourceManager/releases)

# Developement

## Code of conduct.

Do your best to be able to answer yes to the following questions when contributing code.

* Can this method or function easily be tested? (https://markheath.net/post/testable-code-with-pure-functions)
* Is illegal states unrepresentable? (https://enterprisecraftsmanship.com/posts/c-and-f-approaches-to-illegal-state/)

Any bugs have harsher living conditions and are more easily squashed when these two conditions are met.

## Minimum Build Environment

* Run from an admin command line:

```batch		
@"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -InputFormat None -ExecutionPolicy Bypass -Command "iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))" && SET "PATH=%PATH%;%ALLUSERSPROFILE%\chocolatey\bin"
choco feature enable -n allowGlobalConfirmation
choco install fake
choco upgrade fake	
choco install windows-sdk-10-version-1903-all	
choco install visualstudio2019buildtools
choco install netfx-4.8-devpack
choco install wixtoolset	
choco install git
choco feature disable -n allowGlobalConfirmation
```

## Clone And Build

* Run from a standard command prompt:

```batch
mkdir c:\dev\github.trondr
cd c:\dev\github.trondr
git clone https://github.com/trondr/Compliance.Notifications.git ./Compliance.Notifications
cd c:\dev\github.trondr\Compliance.Notifications
Build.cmd
```

## Developement Environment

1. Run from an admin command line:

```batch
@"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -InputFormat None -ExecutionPolicy Bypass -Command "iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))" && SET "PATH=%PATH%;%ALLUSERSPROFILE%\chocolatey\bin"
choco feature enable -n allowGlobalConfirmation
choco install fake
choco upgrade fake	
choco install windows-sdk-10-version-1903-all	
choco install netfx-4.8-devpack
choco install notepadplusplus
choco install git
choco install vscode	
choco install visualstudio2019enterprise
REM choco install visualstudio2019professional
choco install visualstudio2019buildtools
choco install wixtoolset
choco feature disable -n allowGlobalConfirmation
```
2. Start Visual Studio Installer (from start menu)
  * Select Indiviual Components  
  * Install: MSBuild  
3. Download and install Wix Toolset Visual Studio 2019 Extension
  * https://marketplace.visualstudio.com/items?itemName=WixToolset.WixToolsetVisualStudio2019Extension
