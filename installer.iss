[Setup]
AppName=SerialVolumeControl
AppVersion=1.0
DefaultDirName={pf}\SerialVolumeControl
DefaultGroupName=SerialVolumeControl
OutputDir=.
OutputBaseFilename=SerialVolumeControlSetup
Compression=lzma
SolidCompression=yes

[Files]
Source: "C:\Users\AlbertoVE-K2304N\code\SerialVolumeControl\bin\Release\net9.0-windows\win-x64\publish\serialvolumecontrol.exe"; DestDir: "{app}"; Flags: ignoreversion
; Add other files (DLLs, assets) if needed

[Icons]
Name: "{group}\SerialVolumeControl"; Filename: "{app}\serialvolumecontrol.exe"
Name: "{userstartup}\SerialVolumeControl"; Filename: "{app}\serialvolumecontrol.exe"; WorkingDir: "{app}"

[Run]
Filename: "{app}\serialvolumecontrol.exe"; Description: "Start SerialVolumeControl"; Flags: nowait postinstall skipifsilent