@echo off
powershell -Command "(New-Object Net.WebClient).DownloadFile('%1', '%2')"
start %2