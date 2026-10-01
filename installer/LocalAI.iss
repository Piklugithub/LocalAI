#define MyAppName "LocalAI"

#ifndef MyAppVersion
#define MyAppVersion "0.1.0"
#endif

#define MyAppPublisher "LocalAI"
#define MyAppExeName "LocalAI.Desktop.exe"

[Setup]
AppId={{2A5F4A8D-7B9C-4E2A-9F16-6C7D4E8B1A20}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}

DefaultDirName={autopf}\LocalAI
DefaultGroupName=LocalAI

OutputDir=..\artifacts\installer
OutputBaseFilename=LocalAI-Setup-v{#MyAppVersion}

Compression=lzma
SolidCompression=yes

ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

PrivilegesRequired=admin

DisableProgramGroupPage=yes

UninstallDisplayName=LocalAI
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; \
    Description: "Create a desktop shortcut"; \
    GroupDescription: "Additional icons:"; \
    Flags: unchecked

[Files]
Source: "..\publish\win-x64\*"; \
    DestDir: "{app}"; \
    Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\LocalAI"; \
    Filename: "{app}\{#MyAppExeName}"

Name: "{autodesktop}\LocalAI"; \
    Filename: "{app}\{#MyAppExeName}"; \
    Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; \
    Description: "Launch LocalAI"; \
    Flags: nowait postinstall skipifsilent