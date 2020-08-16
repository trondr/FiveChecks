@SET ExitCode=0
@REM ================================================================================================
@echo Start Date: %date% - Time: %time%
@REM ================================================================================================
@echo Computername: %COMPUTERNAME%
@echo Username: %USERNAME%
@echo Architecture: %PROCESSOR_ARCHITECTURE%
@VER
@REM  Set Codepage to 1252, to handle Norwegian Æ-Ø-Å characters
@chcp 1252
@REM  *** Set current UNC path as temporary Drive letter.
@pushd %~dp0

@REM ================================================================================================
@REM RunInNativeMode=True: Run script in 64 bit process on 64 bit OS
@REM RunInNativeMode=False: Run script in 32 bit process on 64 bit OS
@Set RunInNativeMode=True
@REM ================================================================================================

@REM *** Check if Terminal Server 
Set ServerType=0
For /F "tokens=3" %%a in ('reg query "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Terminal Server" /v TSAppCompat ^|find "0x1"') do Set ServerType=Terminal

@REM  *** Use %LOGDIR% for application installation logging in Install/Uninstall section!
@IF EXIST "%Public%\Logs" (Set LOGDIR=%Public%\Logs) ELSE @Set LOGDIR=%TEMP%
@echo LOGDIR set to: %LOGDIR%

@IF "%ServerType%"=="Terminal" "%WinSysDir%\change.exe" user /Install

@REM Removal of previous x86 versions on 64-bits OS.

@REM Check if script commands should be run in 64 bit or 32 bit
IF %RunInNativeMode%==True goto NativeModeTrue
IF %RunInNativeMode%==False goto NativeModeFalse
:NativeModeTrue
@Echo Setting NativeModeTrue
IF EXIST "%windir%\sysnative\cmd.exe" (Set WinSysDir=%windir%\sysnative) ELSE (Set WinSysDir=%windir%\System32)
@goto RunScript
:NativeModeFalse
@Echo Setting NativeModeFalse
IF EXIST "%windir%\SysWOW64\cmd.exe" (Set WinSysDir=%windir%\SysWOW64) ELSE (Set WinSysDir=%windir%\System32)
@goto RunScript

:RunScript
@Echo WinSysDir=%WinSysDir%
@Set CmdExe=%WinSysDir%\cmd.exe
@Set PowerShellExe=%WinSysDir%\WindowsPowershell\v1.0\PowerShell.exe
@Echo PowerShellExe=%PowerShellExe%
%CmdExe% /C "Set PROCESSOR_ARCHITECTURE"
REM "%PowerShellExe%" -ExecutionPolicy UnRestricted -Command "& { . \"%~dp0%~n0.ps1\" %1 %2 %3 %4 %5 %6 %7 %8 %9; exit $LASTEXITCODE }"
@REM ================================================================================================
"%WinSysDir%\msiexec.exe" /x "%~dp0%MsiFileName%" REBOOT=ReallySuppress /l*v "%LOGDIR%\%MsiFileName%-uninstall.log" /qn
@Set ExitCode=%errorlevel%
@REM ================================================================================================
@Echo ExitCode=%ExitCode%
@goto End

:ArgumentError
@echo ERROR: Command line argument(s) missing! Please check installation string
set resultcode=77

:End
@REM ================================================================================================
@echo *** End Date: %date% - Time: %time%
@REM ================================================================================================
@IF "%ServerType%"=="Terminal" "%WinSysDir%\change.exe" user /Execute
@popd
@Exit /B %ExitCode%