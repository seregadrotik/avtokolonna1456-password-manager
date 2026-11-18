; Inno Setup script
; Output: dist\Avtokolonna1456_Setup.exe

#define MyAppName "Автоколонна 1456 — Менеджер паролей"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Автоколонна 1456"
#define MyAppExeName "Avtokolonna1456.PasswordManager.exe"
#define MyAppId "Avtokolonna1456.PasswordManager"

[Setup]
AppId={{B7C2A1D4-8E5F-4A91-9C33-1F0E6D8A2B45}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\Avtokolonna1456\PasswordManager
DefaultGroupName=Автоколонна 1456
DisableProgramGroupPage=yes
OutputDir=..\dist
OutputBaseFilename=Avtokolonna1456_Setup
SetupIconFile=..\src\Avtokolonna1456.PasswordManager\Resources\app.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=lowest
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Tasks]
Name: "desktopicon"; Description: "Создать ярлык на рабочем столе"; GroupDescription: "Дополнительно:"

[Files]
Source: "..\src\Avtokolonna1456.PasswordManager\bin\x64\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Запустить приложение"; Flags: nowait postinstall skipifsilent

[Code]
function IsDotNet48Installed(): Boolean;
var
  Release: Cardinal;
begin
  Result := RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release)
    and (Release >= 528040);
end;

function InitializeSetup(): Boolean;
begin
  if not IsDotNet48Installed() then
  begin
    MsgBox('.NET Framework 4.8 не найден.'#13#10 +
           'Установите его с сайта Microsoft и повторите установку.',
           mbError, MB_OK);
    Result := False;
  end
  else
    Result := True;
end;
