@echo off
set CopyTo=.\Help
set OutputPath=C:\Windows\Temp\Help
set WorkingPath=C:\Windows\Temp\HelpWork

if exist "%OutputPath%\" rmdir /s/q %OutputPath%
if exist "%WorkingPath%\" rmdir /s/q %WorkingPath%

c:\Windows\Microsoft.NET\Framework\v3.5\msbuild s3pi.shfbproj "/p:OutputPath=%OutputPath:\=\\%" "/p:WorkingPath=%WorkingPath:\=\\%"
if ERRORLEVEL 1 goto FAIL

if exist "%CopyTo%" rmdir /s/q %CopyTo%
robocopy "%OutputPath%" "%CopyTo%" /s /xo /np /njh /njs /w:5 /log:.\robocopy.log
if ERRORLEVEL 2 goto FAIL
del .\robocopy.log
rmdir /s/q %OutputPath%

copy htaccess.txt "%CopyTo%\.htaccess"

goto PAUSE

:FAIL
echo %ERRORLEVEL%

:PAUSE
pause