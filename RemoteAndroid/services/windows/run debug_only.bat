@echo off
set /p appdir=<..\appdir.txt
cd /D %~dp0\..\..\%appdir%
%AppData%\npm\cordova prepare android && %AppData%\npm\cordova run android
pause