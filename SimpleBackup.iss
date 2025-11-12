[Setup]
AppName=Simple Backup
AppVersion=1.0.0
AppPublisher=Simple Backup Project
AppPublisherURL=https://github.com/unhuman/Simple-Backup
AppSupportURL=https://github.com/unhuman/Simple-Backup/issues
AppUpdatesURL=https://github.com/unhuman/Simple-Backup/releases
DefaultDirName={autopf}\Simple Backup
DefaultGroupName=Simple Backup
AllowNoIcons=yes
OutputDir=.\installer
OutputBaseFilename=SimpleBackup-1.0.0-Setup
Compression=zip
SolidCompression=yes
UninstallDisplayIcon={app}\Simple Backup.exe
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Main executable and dependencies from the release build
Source: "bin\Release\net10.0-windows\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Simple Backup"; Filename: "{app}\Simple Backup.exe"
Name: "{group}\{cm:UninstallProgram,Simple Backup}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\Simple Backup"; Filename: "{app}\Simple Backup.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\Simple Backup.exe"; Description: "{cm:LaunchProgram,Simple Backup}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: dirifempty; Name: "{app}"
