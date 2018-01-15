@ECHO ON
@REM Creates template Messages file and updates existing translation catalogs.
@REM When invoked with argument, it will also copy Locale folder to target directory specified by argument.

PUSHD %~dp0

setlocal enableextensions enabledelayedexpansion

if not exist "%~dp0vswhere.exe" GOTO DEFAULTBUILDLOCALE

REM VSWhere from https://github.com/Microsoft/vswhere use to locate highest installed VS Installation
for /f "usebackq tokens=1* delims=: " %%i in (`vswhere.exe -latest -requires Microsoft.VisualStudio.Workload.NativeDesktop`) do (
if /i "%%i"=="installationPath" set VSDir=%%j
)
ECHO "Visual Studio 2017 Detected as installed at "%VSDir%
IF "%VSDir%" == "" GOTO DEFAULTBUILDLOCALE

IF [%1] == [] cmd /C "%VSDir%\Common7\Tools\vsdevcmd.bat && cd ""%~dp0"" && msbuild release.xml /target:BuildLocale"
IF NOT [%1] == [] cmd /C "%VSDir%\Common7\Tools\vsdevcmd.bat && cd ""%~dp0"" && msbuild release.xml /target:BuildAndCopyLocale /property:LocaleTargetDir=%1"

GOTO END
:DEFAULTBUILDLOCALE

ECHO "Using Default Build Locale"

IF [%1] == [] cmd /C "vsdevcmd.bat && cd ""%~dp0"" && msbuild release.xml /target:BuildLocale"
IF NOT [%1] == [] cmd /C "vsdevcmd.bat && cd ""%~dp0"" && msbuild release.xml /target:BuildAndCopyLocale /property:LocaleTargetDir=%1"
:END

POPD