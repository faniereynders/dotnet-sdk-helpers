@echo off

if [%1]==[help] goto help
if [%1]==[] goto help
if [%1]==[list] goto sdk_list
if [%1]==[latest] goto sdk_latest
if [%1]==[releases] goto sdk_releases
if [%1]==[get] goto sdk_download

for /f %%f in ('dir /b "%programfiles%\dotnet\sdk"') do (
    if %1==%%f goto switch
) 
echo The %1 version of .Net Core SDK was not found 
echo Please, run "dotnet sdk list" to make sure you have it installed in "%programfiles%\dotnet\sdk" 
goto end

:switch
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
if exist ..\global.json (
    set /p choice= There's a global.json in your parent directory. Do you want to delete it? (N/y) 
    if /I "%choice%"=="y" (
        del ..\global.json
    ) else (
        SETLOCAL ENABLEDELAYEDEXPANSION
        set dotnetVersion=
        for /f "delims=" %%a in ('dotnet --version') do set dotnetVersion=%%a
        echo .NET Core SDK current version: !dotnetVersion!
        goto end
    )
) 
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

goto end

:sdk_releases
echo Releases available for the .NET Core SDK are:
curl "https://raw.githubusercontent.com/dotnet/core/master/release-notes/releases.json" -H "Accept: application/json" -s | jq "map({date: .date,sdk: .\"version-sdk\"}) | unique_by(.sdk) | .[] | \"\(.date)\t\(.sdk)\" " -r
echo.

goto end

:sdk_download
SETLOCAL
SET version=%2
if [%version%]==[] SET version=latest
if "%version%"=="latest" (
    curl "https://raw.githubusercontent.com/dotnet/core/master/release-notes/releases.json" -H "Accept: application/json" -s | jq "map({date: .date,sdk: .\"version-sdk\"}) | unique_by(.sdk) | .[-1] | .sdk " -r > version.dat
    set /p version=<version.dat
)

SET platform=win-x64
if NOT [%3]==[] SET platform=%3
SET platform_id=sdk-%platform%

curl "https://raw.githubusercontent.com/dotnet/core/master/release-notes/releases.json" -H "Accept: application/json" -s | jq "map({sdk: .\"version-sdk\",url: (.\"blob-sdk\" + (.\"%platform_id%\" | rtrimstr(\".zip\")) + \".exe\"  )}) | unique_by(.sdk)  | .[] | select(.sdk==\"%version%\") | .url " -r > download.dat

SET /p version=<version.dat
SET /p url=<download.dat

echo Downloading .NET Core SDK version %version% for platform %platform%...

echo %url%

download %url% .\installs\%version%.exe

echo Done.
ENDLOCAL
goto end

:end
