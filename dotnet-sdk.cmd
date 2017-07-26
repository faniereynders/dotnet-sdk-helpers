@echo off

if [%1]==[help] goto help
if [%1]==[] goto help
if [%1]==[list] goto sdk_list
if [%1]==[latest] goto sdk_latest
echo Switching .NET Core SDK version to %1
(
echo {
echo   "sdk": {
echo     "version": "%1"
echo   }
echo }
) > global.json
goto end

:sdk_list
echo The installed .NET Core SDKs are:
dir /b "%programfiles%\dotnet\sdk" | find /i "."
goto end

:sdk_latest
if exist global.json del global.json
echo .NET Core SDK version switched to latest version.
dotnet --version

goto end

:help
echo .NET Core SDK Switcher
echo.
echo Usage: .net sdk [command]
echo Usage: .net sdk [version]
echo.
echo Commands:
echo   latest      Swtiches to the latest .NET Core SDK version
echo   list        Lists all installed .NET Core SDKs
echo   help        Display help
echo.
echo versions:
echo   An installed version number of a .NET Core SDK
echo.

:end
