# Compliance-Notifications

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
