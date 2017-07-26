@echo off
if [%1]==[help] goto help
REM pushd %~dp0
"%programfiles%\dotnet\dotnet.exe" %*
REM popd
goto end

:help
echo .NET Core CLI SDK Helper
echo.
echo Usage: .net [command]
echo Usage: .net [dotnet args]
echo.
echo Helper commands:
echo   sdk      Switches to a specific .NET Core SDK version
echo   help     Display help
echo.
echo dotnet args:
echo   The standard dotnet commands for installed sdk
echo.
:end
