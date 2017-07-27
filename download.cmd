@echo off
echo Downloading release %2...
powershell -Command "(New-Object Net.WebClient).DownloadFile('%1', '%2.exe')"
start %2