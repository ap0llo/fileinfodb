@ECHO OFF
SETLOCAL

for /f "usebackq tokens=*" %%i in (`"%programfiles(x86)%\Microsoft Visual Studio\Installer\vswhere" -latest -products * -requires Microsoft.Component.MSBuild -property installationPath`) do (
  SET InstallDir=%%i
)
SET MSBuildPath="%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe"

IF NOT EXIST %MSBuildPath% (  
  SET MSBuildPath="%InstallDir%\MSBuild\Current\Bin\MSBuild.exe"
)

IF NOT EXIST %MSBuildPath% (
  ECHO No MSBuild installation found. Is Visual Studio 2015 or 2019 installed?
  EXIT /B 1
) 

ECHO MSBuild found at %MSBuildPath%

if /I "%APPVEYOR%" == "True" ( 
    SET LOGGER=/loggit rger:C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll
) else (
    SET LOGGER=
)

%MSBuildPath% /clp:verbosity=minimal "%LOGGER%" %*

