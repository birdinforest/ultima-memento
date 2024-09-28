@echo off
REM
REM -- RunUO compile script for .NET 4.0 --
REM
REM The full .NET framework needs to be installed for this script.
REM i.e. not the "Client Profile", as it is missing several required DLLs.
REM
set targetfile=World.exe

if exist "%targetfile%" (
	echo Deleting binary...
	del "%targetfile%" 1>NUL 2>NUL
	
	if exist "%targetfile%" (
		echo Failed!
		echo.
		echo Is "%targetfile%" in use?
		echo.
		goto end
	) else (
		echo Success.
		echo.
	)
)

echo Recompiling...
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc /optimize /unsafe /t:exe /out:..\..\World.exe /win32icon:..\System\icon.ico /d:NEWTIMERS /d:NEWPARENT /recurse:..\System\*.cs /main:Server.Core

:end
pause
