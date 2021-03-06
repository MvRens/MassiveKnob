#define AppName "Massive Knob"
#define AppVersion GetEnv("BUILD_VERSION")
#define AppPublisher "Mark van Renswoude"
#define AppURL "https://github.com/MvRens/MassiveKnob"
#define AppExeName "MassiveKnob.exe"
#define BasePath ".."

#if AppVersion == ""
  #define AppVersion "IDE build"
#endif


[Setup]
AppId={{6D668D73-54E5-4FE1-8028-24FE993627B8}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
DefaultDirName={pf}\{#AppName}
DisableProgramGroupPage=yes
OutputDir={#BasePath}\Release
OutputBaseFilename=MassiveKnobSetup-{#AppVersion}
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64

[Types]
Name: "full"; Description: "Full installation"
Name: "custom"; Description: "Custom installation"; Flags: iscustom

[Components]
Name: main; Description: "Massive Knob application"; Types: full custom; Flags: fixed
Name: essentialplugins; Description: "Essential plugins"; Types: full custom
Name: essentialplugins\serialdevice; Description: "Serial device"; Types: full custom
Name: essentialplugins\coreaudio; Description: "Windows Core Audio actions"; Types: full custom
Name: optionalplugins; Description: "Optional plugins"; Types: full custom
Name: optionalplugins\emulatordevice; Description: "Emulator device"; Types: full custom
Name: optionalplugins\voicemeeter; Description: "VoiceMeeter actions"; Types: full custom

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Main application
Source: {#BasePath}\MassiveKnob\bin\Release\{#AppExeName}; DestDir: "{app}"; Flags: ignoreversion; Components: main
Source: {#BasePath}\MassiveKnob\bin\Release\{#AppExeName}.config; DestDir: "{app}"; Flags: ignoreversion; Components: main
Source: {#BasePath}\MassiveKnob\bin\Release\*.dll; DestDir: "{app}"; Flags: ignoreversion; Components: main

; Serial device plugin
Source: {#BasePath}\MassiveKnob.Plugin.SerialDevice\bin\Release\MassiveKnobPlugin.json; DestDir: "{app}\Plugins\SerialDevice"; Flags: ignoreversion; Components: essentialplugins\serialdevice
Source: {#BasePath}\MassiveKnob.Plugin.SerialDevice\bin\Release\*.dll; DestDir: "{app}\Plugins\SerialDevice"; Flags: ignoreversion; Components: essentialplugins\serialdevice

; Core Audio plugin
Source: {#BasePath}\MassiveKnob.Plugin.CoreAudio\bin\Release\MassiveKnobPlugin.json; DestDir: "{app}\Plugins\CoreAudio"; Flags: ignoreversion; Components: essentialplugins\coreaudio
Source: {#BasePath}\MassiveKnob.Plugin.CoreAudio\bin\Release\*.dll; DestDir: "{app}\Plugins\CoreAudio"; Flags: ignoreversion; Components: essentialplugins\coreaudio

; Emulator device plugin
Source: {#BasePath}\MassiveKnob.Plugin.EmulatorDevice\bin\Release\MassiveKnobPlugin.json; DestDir: "{app}\Plugins\EmulatorDevice"; Flags: ignoreversion; Components: optionalplugins\emulatordevice
Source: {#BasePath}\MassiveKnob.Plugin.EmulatorDevice\bin\Release\*.dll; DestDir: "{app}\Plugins\EmulatorDevice"; Flags: ignoreversion; Components: optionalplugins\emulatordevice

; VoiceMeeter plugin
Source: {#BasePath}\MassiveKnob.Plugin.VoiceMeeter\bin\Release\MassiveKnobPlugin.json; DestDir: "{app}\Plugins\VoiceMeeter"; Flags: ignoreversion; Components: optionalplugins\voicemeeter
Source: {#BasePath}\MassiveKnob.Plugin.VoiceMeeter\bin\Release\*.dll; DestDir: "{app}\Plugins\VoiceMeeter"; Flags: ignoreversion; Components: optionalplugins\voicemeeter

[Dirs]
Name: "{localappdata}\MassiveKnob"
Name: "{localappdata}\MassiveKnob\Logs"
Name: "{localappdata}\MassiveKnob\Plugins"

[Icons]
Name: "{commonprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{commondesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
;Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// .NET version detection credit goes to:
// https://www.kynosarges.org/DotNetVersion.html

function IsDotNetDetected(version: string; service: cardinal): boolean;
// Indicates whether the specified version and service pack of the .NET Framework is installed.
//
// version -- Specify one of these strings for the required .NET Framework version:
//    'v1.1'          .NET Framework 1.1
//    'v2.0'          .NET Framework 2.0
//    'v3.0'          .NET Framework 3.0
//    'v3.5'          .NET Framework 3.5
//    'v4\Client'     .NET Framework 4.0 Client Profile
//    'v4\Full'       .NET Framework 4.0 Full Installation
//    'v4.5'          .NET Framework 4.5
//    'v4.5.1'        .NET Framework 4.5.1
//    'v4.5.2'        .NET Framework 4.5.2
//    'v4.6'          .NET Framework 4.6
//    'v4.6.1'        .NET Framework 4.6.1
//    'v4.6.2'        .NET Framework 4.6.2
//    'v4.7'          .NET Framework 4.7
//    'v4.7.1'        .NET Framework 4.7.1
//    'v4.7.2'        .NET Framework 4.7.2
//    'v4.8'          .NET Framework 4.8
//
// service -- Specify any non-negative integer for the required service pack level:
//    0               No service packs required
//    1, 2, etc.      Service pack 1, 2, etc. required
var
    key, versionKey: string;
    install, release, serviceCount, versionRelease: cardinal;
    success: boolean;
begin
    versionKey := version;
    versionRelease := 0;

    // .NET 1.1 and 2.0 embed release number in version key
    if version = 'v1.1' then begin
        versionKey := 'v1.1.4322';
    end else if version = 'v2.0' then begin
        versionKey := 'v2.0.50727';
    end

    // .NET 4.5 and newer install as update to .NET 4.0 Full
    else if Pos('v4.', version) = 1 then begin
        versionKey := 'v4\Full';
        case version of
          'v4.5':   versionRelease := 378389;
          'v4.5.1': versionRelease := 378675; // 378758 on Windows 8 and older
          'v4.5.2': versionRelease := 379893;
          'v4.6':   versionRelease := 393295; // 393297 on Windows 8.1 and older
          'v4.6.1': versionRelease := 394254; // 394271 before Win10 November Update
          'v4.6.2': versionRelease := 394802; // 394806 before Win10 Anniversary Update
          'v4.7':   versionRelease := 460798; // 460805 before Win10 Creators Update
          'v4.7.1': versionRelease := 461308; // 461310 before Win10 Fall Creators Update
          'v4.7.2': versionRelease := 461808; // 461814 before Win10 April 2018 Update
          'v4.8':   versionRelease := 528040; // 528049 before Win10 May 2019 Update
        end;
    end;

    // installation key group for all .NET versions
    key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\' + versionKey;

    // .NET 3.0 uses value InstallSuccess in subkey Setup
    if Pos('v3.0', version) = 1 then begin
        success := RegQueryDWordValue(HKLM, key + '\Setup', 'InstallSuccess', install);
    end else begin
        success := RegQueryDWordValue(HKLM, key, 'Install', install);
    end;

    // .NET 4.0 and newer use value Servicing instead of SP
    if Pos('v4', version) = 1 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Servicing', serviceCount);
    end else begin
        success := success and RegQueryDWordValue(HKLM, key, 'SP', serviceCount);
    end;

    // .NET 4.5 and newer use additional value Release
    if versionRelease > 0 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Release', release);
        success := success and (release >= versionRelease);
    end;

    result := success and (install = 1) and (serviceCount >= service);
end;


function InitializeSetup(): Boolean;
var
  response: Integer;
  errorCode: Integer;

begin
  if not IsDotNetDetected('v4.7.2', 0) then 
  begin
    response := SuppressibleMsgBox('Massive Knob requires Microsoft .NET Framework 4.7.2, which does not appear to be installed.' +
      'Do you want to open the .NET download page?'#13#10#13#10 +
      'Click No to continue, although Massive Knob may not run properly afterwards, or Cancel to abort the setup.', 
      mbInformation, 
      MB_YESNOCANCEL,
      IDNO);

    case response of
      IDYES:
        begin
          ShellExecAsOriginalUser('open', 'https://dotnet.microsoft.com/download/dotnet-framework/net472', '', '', 
            SW_SHOWNORMAL, ewNoWait, errorCode);
          Result := False;
        end;

      IDNO:
        Result := True;

      IDCANCEL:
        Result := False;
    end;
  end else
    Result := True;
end;