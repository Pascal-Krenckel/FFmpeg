nuget pack FFmpegDotNet.bin.winx64

@echo off
set "psCommand=powershell -Command "$pword = read-host 'Enter Password' -AsSecureString ; ^
    $BSTR=[System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($pword); ^
        [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)""
for /f "usebackq delims=" %%p in (`%psCommand%`) do set password=%%p
for /f %%i in ('dir /b /od /a-d FFmpegDotNet.bin.winx64.*') do set LAST=%%i
dotnet nuget push %LAST% --api-key %password% --source https://api.nuget.org/v3/index.json --timeout 3000
PAUSE