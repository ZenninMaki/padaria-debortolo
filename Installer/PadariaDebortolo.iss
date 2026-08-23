#define MyAppName "Padaria Debortolo"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Padaria Debortolo"
#define MyAppExeName "infinite_coffee_app.exe"

[Setup]
AppId={{B7BD4DDC-6B04-4F3A-9C2D-8C8E6A5E4A10}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\Padaria Debortolo
DefaultGroupName={#MyAppName}
OutputDir=..\artifacts\installer
OutputBaseFilename=PadariaDebortolo-Setup
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin
UninstallDisplayIcon={app}\desktop\{#MyAppExeName}

[Files]
Source: "..\artifacts\server\*"; DestDir: "{app}\server"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\artifacts\desktop\*"; DestDir: "{app}\desktop"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\DatabaseScripts\*"; DestDir: "{app}\database"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\server\Start-PadariaDebortolo.cmd"; WorkingDir: "{app}\server"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\desktop\{#MyAppExeName}"; WorkingDir: "{app}\desktop"

[Run]
Filename: "{app}\server\Start-PadariaDebortolo.cmd"; Description: "Iniciar o sistema web agora"; Flags: postinstall nowait skipifsilent
