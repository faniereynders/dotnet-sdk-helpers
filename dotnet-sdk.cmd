@echo off

if [%1]==[help] goto help
if [%1]==[] goto help
if [%1]==[list] goto sdk_list
if [%1]==[latest] goto sdk_latest

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

REM Look for a global.json file in all the parent directories up to the root
set currentDir=%cd%
call :searchParentDirectories
cd %currentDir%

REM No global.json found in any parent directory, we don't need to do anything more
if "%version_from_global%"=="" goto end

REM In order to get the latest sdk installed, dotnet.exe --version needs to be executed in a context
REM where no global.json will be found. I might be assuming too much but I think it's safe to believe
REM no global.json will be found in %programfiles%\dotnet\
for /f %%f in ('start /d "%programfiles%\dotnet\" /b dotnet.exe --version') do (
    set latest_installed_version=%%f
)

REM The global.json already points to the latest version installed. 
if "%version_from_global%"=="%latest_installed_version%" goto end

echo You have a global.json in one of your parent directories pointing to the %version_from_global% sdk version
echo while the latest installed version is the %latest_installed_version% sdk
echo.

set /p choice= Do you want to switch to the latest installed sdk (%latest_installed_version%)? (Y/n)
REM Do nothing
if /I "%choice%"=="n" goto end
echo Switching .NET Core SDK version to %latest_installed_version% (as a local globa.json)
(
echo {
echo   "sdk": {
echo     "version": "%latest_installed_version%"
echo   }
echo }
) > global.json
dotnet --version

goto end

REM Function that goes up in the directory tree looking for a global.json
REM Stops on the first one found or when the root is reached
:searchParentDirectories
cd ..
if exist global.json ( 
    for /f %%f in ('dotnet.exe --version') do (
        set version_from_global=%%f
    )
    exit /b
)
REM We got to the root, stop recursion
if "%cd:~3,1%"=="" exit /b
call :searchParentDirectories
exit /b

goto end

:help
echo .NET Core SDK Switcher
echo.
echo Usage: .net sdk [command]
echo Usage: .net sdk [version]
echo.
echo Commands:
echo   latest      Switches to the latest .NET Core SDK version
echo   list        Lists all installed .NET Core SDKs
echo   help        Display help
echo.
echo versions:
echo   An installed version number of a .NET Core SDK
echo.

:end
