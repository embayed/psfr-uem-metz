; Inno Setup Script for Intalio AutoCAD Protocol Handler
; Download Inno Setup from: https://jrsoftware.org/isdl.php

#define MyAppName "Intalio AutoCAD Protocol Handler"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Intalio"
#define MyAppExeName "PSFR_AutoCAD_ProtocolHandler.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
AppId={{8F4A5C3B-9D2E-4B1A-8C7F-6E9A2D5B4C8F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\Intalio\AutoCAD Protocol Handler
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=
OutputDir=Setup
OutputBaseFilename=Intalio_AutoCAD_ProtocolHandler_Setup
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
UninstallDisplayIcon={app}\{#MyAppExeName}
SetupIconFile=
DisableWelcomePage=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
; Main executable (will be built by publish.bat)
Source: "bin\Release\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "Configure AutoCAD Protocol Handler"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"

[Run]
; Register the protocol handlers after installation
Filename: "{app}\{#MyAppExeName}"; Parameters: "/register"; StatusMsg: "Registering protocol handlers..."; Flags: runhidden

[UninstallRun]
; Unregister the protocol handlers before uninstallation
Filename: "{app}\{#MyAppExeName}"; Parameters: "/unregister"; RunOnceId: "UnregisterProtocols"; Flags: runhidden

[Code]
function InitializeSetup(): Boolean;
begin
  Result := True;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Additional post-install tasks can go here
  end;
end;

function InitializeUninstall(): Boolean;
begin
  Result := True;
end;

[Messages]
WelcomeLabel2=This will install [name/ver] on your computer.%n%nThis application allows you to open AutoCAD files directly from your web browser using custom protocol handlers.%n%nIt is recommended that you close all other applications before continuing.
