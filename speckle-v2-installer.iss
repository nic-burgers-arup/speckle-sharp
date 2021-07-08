;defining variables
#define AppName      "Spec-v2"
#define Autocad2021Version  GetVersionNumbersString("ConnectorAutocadCivil\ConnectorAutocad2021\bin\Release\SpeckleConnectorAutocad.dll")
#define Autocad2022Version  GetVersionNumbersString("ConnectorAutocadCivil\ConnectorAutocad2022\bin\Release\SpeckleConnectorAutocad.dll")
#define Civil2021Version  GetVersionNumbersString("ConnectorAutocadCivil\ConnectorCivil2021\bin\Release\SpeckleConnectorCivil.dll")
#define Civil2022Version  GetVersionNumbersString("ConnectorAutocadCivil\ConnectorCivil2022\bin\Release\SpeckleConnectorCivil.dll")

#define DynamoVersion  GetVersionNumbersString("ConnectorDynamo\ConnectorDynamo\bin\Release\SpeckleConnectorDynamo.dll")
#define DynamoExtensionVersion  GetVersionNumbersString("ConnectorDynamo\ConnectorDynamoExtension\bin\Release\SpeckleConnectorDynamoExtension.dll")
#define DynamoFunctionsVersion  GetVersionNumbersString("ConnectorDynamo\ConnectorDynamoFunctions\bin\Release\SpeckleConnectorDynamoFunctions.dll")

#define GrasshopperVersion  GetVersionNumbersString("ConnectorGrasshopper\ConnectorGrasshopper\bin\SpeckleConnectorGrasshopper.dll")

#define Revit2019Version  GetVersionNumbersString("ConnectorRevit\ConnectorRevit2019\bin\Release\SpeckleConnectorRevit.dll")
#define Revit2020Version  GetVersionNumbersString("ConnectorRevit\ConnectorRevit2020\bin\Release\SpeckleConnectorRevit.dll")
#define Revit2021Version  GetVersionNumbersString("ConnectorRevit\ConnectorRevit2021\bin\Release\SpeckleConnectorRevit.dll")
#define Revit2022Version  GetVersionNumbersString("ConnectorRevit\ConnectorRevit2022\bin\Release\SpeckleConnectorRevit.dll")

#define RhinoVersion  GetFileVersion("ConnectorRhino\ConnectorRhino\bin\Release\SpeckleConnectorRhino.dll")

#define AppPublisher "Spec-cx"
#define AppURL       "https://docs.speckle.arup.com"
#define SpeckleFolder "{localappdata}\Speckle"
#define AppVersion "1.0.0.0"

[Setup]
AppId={{BA3A01AA-F70D-4747-AA0E-E93F38C793C8}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
DefaultDirName={#SpeckleFolder}
DisableDirPage=yes
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
DisableWelcomePage=no
OutputDir="."
OutputBaseFilename=Speckle-cx-{#AppVersion}
SetupIconFile=DesktopUI\DesktopUI\Resources\s2block.ico
Compression=lzma
SolidCompression=yes
ChangesAssociations=yes
PrivilegesRequired=lowest
VersionInfoVersion={#AppVersion}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Components]
Name: dynamo; Description: Speckle for Dynamo 2.1+ - v{#DynamoVersion}; Types: full
Name: dynamoext; Description: Speckle for Dynamo Extensions - v{#DynamoExtensionVersion}; Types: full
Name: dynamofunc; Description: Speckle for Dynamo Functions - v{#DynamoFunctionsVersion}; Types: full
Name: rhino; Description: Speckle for Rhino 6, Rhino 7 - v{#RhinoVersion};  Types: full
Name: gh; Description: Speckle for Grasshopper - v{#GrasshopperVersion};  Types: full
Name: revit19; Description: Speckle for Revit 2019 ALPHA - v{#Revit2019Version};  Types: full
Name: revit20; Description: Speckle for Revit 2020 ALPHA - v{#Revit2020Version};  Types: full
Name: revit21; Description: Speckle for Revit 2021 ALPHA - v{#Revit2021Version};  Types: full
Name: revit22; Description: Speckle for Revit 2022 ALPHA - v{#Revit2021Version};  Types: full
Name: autocad21; Description: Speckle for AutoCAD 2021 ALPHA - v{#Autocad2021Version};  Types: full
Name: autocad22; Description: Speckle for AutoCAD 2022 ALPHA - v{#Autocad2022Version};  Types: full
Name: civil21; Description: Speckle for AutoCADCivil 2021 ALPHA - v{#Civil2021Version};  Types: full
Name: civil22; Description: Speckle for AutoCADCivil 2022 ALPHA - v{#Civil2022Version};  Types: full


[Types]
Name: "full"; Description: "Full installation"
Name: "custom"; Description: "Custom installation"; Flags: iscustom

[Dirs]
Name: "{app}"; Permissions: everyone-full

[Files]
;rhino                                                                                                                                    
Source: "ConnectorRhino\ConnectorRhino\bin\Release\*"; DestDir: "{userappdata}\McNeel\Rhinoceros\6.0\Plug-ins\SpeckleRhino2 (8dd5f30b-a13d-4a24-abdc-3e05c8c87143)\"; Flags: ignoreversion recursesubdirs; Components: rhino
Source: "ConnectorRhino\ConnectorRhino\bin\Release\*"; DestDir: "{userappdata}\McNeel\Rhinoceros\7.0\Plug-ins\SpeckleRhino2 (8dd5f30b-a13d-4a24-abdc-3e05c8c87143)\"; Flags: ignoreversion recursesubdirs; Components: rhino

;gh
Source: "ConnectorGrasshopper\ConnectorGrasshopper\bin\*"; DestDir: "{userappdata}\Grasshopper\Libraries\SpeckleGrasshopper2\"; Flags: ignoreversion recursesubdirs; Components: gh

;revit19
Source: "ConnectorRevit\ConnectorRevit2019\bin\Release\*"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2019\"; Flags: ignoreversion recursesubdirs; Components: revit19

;revit20
Source: "ConnectorRevit\ConnectorRevit2020\bin\Release\*"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2020\"; Flags: ignoreversion recursesubdirs; Components: revit20

;revit21
Source: "ConnectorRevit\ConnectorRevit2021\bin\Release\*"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2021\"; Flags: ignoreversion recursesubdirs; Components: revit21

;revit22
Source: "ConnectorRevit\ConnectorRevit2022\bin\Release\*"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2022\"; Flags: ignoreversion recursesubdirs; Components: revit22

;autocad21
Source: "ConnectorAutocadCivil\ConnectorAutocad2021\bin\Release\*"; DestDir: "{userappdata}\Autodesk\ApplicationPlugins\Speckle2AutoCAD2021\"; Flags: ignoreversion recursesubdirs; Components: autocad21

;autocad22
Source: "ConnectorAutocadCivil\ConnectorAutocad2022\bin\Release\*"; DestDir: "{userappdata}\Autodesk\ApplicationPlugins\Speckle2AutoCAD2022\"; Flags: ignoreversion recursesubdirs; Components: autocad21

;civil21
Source: "ConnectorAutocadCivil\ConnectorCivil2021\bin\Release\*"; DestDir: "{userappdata}\Autodesk\ApplicationPlugins\Speckle2Civil3D2021\"; Flags: ignoreversion recursesubdirs; Components: civil21

;civil22
Source: "ConnectorAutocadCivil\ConnectorCivil2022\bin\Release\*"; DestDir: "{userappdata}\Autodesk\ApplicationPlugins\Speckle2Civil3D2022\"; Flags: ignoreversion recursesubdirs; Components: civil22


[InstallDelete]
Type: filesandordirs; Name: "{userappdata}\McNeel\Rhinoceros\6.0\Plug-ins\SpeckleRhino2 (8dd5f30b-a13d-4a24-abdc-3e05c8c87143)\*"
Type: filesandordirs; Name: "{userappdata}\McNeel\Rhinoceros\7.0\Plug-ins\SpeckleRhino2 (8dd5f30b-a13d-4a24-abdc-3e05c8c87143)\*"
Type: filesandordirs; Name: "{userappdata}\Grasshopper\Libraries\SpeckleGrasshopper2\*"
Type: filesandordirs; Name: "{userappdata}\Autodesk\Revit\Addins\2019\*"
Type: filesandordirs; Name: "{userappdata}\Autodesk\Revit\Addins\2020\*"
Type: filesandordirs; Name: "{userappdata}\Autodesk\Revit\Addins\2021\*"
Type: filesandordirs; Name: "{userappdata}\Autodesk\Revit\Addins\2022\*"
Type: filesandordirs; Name: "{userappdata}\Autodesk\ApplicationPlugins\Speckle2AutoCAD2021\*"
Type: filesandordirs; Name: "{userappdata}\Autodesk\ApplicationPlugins\Speckle2AutoCAD2022\*"
Type: filesandordirs; Name: "{userappdata}\Autodesk\ApplicationPlugins\Speckle2Civil3D2021\*"
Type: filesandordirs; Name: "{userappdata}\Autodesk\ApplicationPlugins\Speckle2Civil3D2022\*"


[Registry]
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; ValueType: string; ValueName: "Name"; ValueData: "Speckle";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; ValueType: string; ValueName: "RegPath"; ValueData: "\\HKEY_CURRENT_USER\Software\McNeel\Rhinoceros\6.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; ValueType: dword; ValueName: "DirectoryInstall"; ValueData: "0";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; ValueType: dword; ValueName: "IsDotNETPlugIn"; ValueData: "1";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; ValueType: dword; ValueName: "LoadMode"; ValueData: "2";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; ValueType: dword; ValueName: "Type"; ValueData: "16";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143\CommandList"; ValueType: string; ValueName: "SpecklePanel"; ValueData: "2;SpecklePanel";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143\PlugIn"; ValueType: string; ValueName: "FileName"; ValueData: "{userappdata}\McNeel\Rhinoceros\6.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143\SpeckleWinR6.rhp";  
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; ValueType: string; ValueName: "Name"; ValueData: "Speckle";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; ValueType: string; ValueName: "RegPath"; ValueData: "\\HKEY_CURRENT_USER\Software\McNeel\Rhinoceros\7.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; ValueType: dword; ValueName: "DirectoryInstall"; ValueData: "0";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; ValueType: dword; ValueName: "IsDotNETPlugIn"; ValueData: "1";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; ValueType: dword; ValueName: "LoadMode"; ValueData: "2";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; ValueType: dword; ValueName: "Type"; ValueData: "16";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143\CommandList"; ValueType: string; ValueName: "SpecklePanel"; ValueData: "2;SpecklePanel";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143\PlugIn"; ValueType: string; ValueName: "FileName"; ValueData: "{userappdata}\McNeel\Rhinoceros\7.0\Plug-ins\SpeckleRhino2 (8dd5f30b-a13d-4a24-abdc-3e05c8c87143)\SpeckleWinR6.rhp";  

;checks if minimun requirements are met
[Code]
function IsDotNetDetected(version: string; service: cardinal): boolean;
// Indicates whether the specified version and service pack of the .NET Framework is installed.
//
// version -- Specify one of these strings for the required .NET Framework version:
//    'v1.1.4322'     .NET Framework 1.1
//    'v2.0.50727'    .NET Framework 2.0
//    'v3.0'          .NET Framework 3.0
//    'v3.5'          .NET Framework 3.5
//    'v4\Client'     .NET Framework 4.0 Client Profile
//    'v4\Full'       .NET Framework 4.0 Full Installation
//    'v4.7'          .NET Framework 4.5
//
// service -- Specify any non-negative integer for the required service pack level:
//    0               No service packs required
//    1, 2, etc.      Service pack 1, 2, etc. required
var
    key: string;
    install, release, serviceCount: cardinal;
    check47, success: boolean;
begin
    // .NET 4.5 installs as update to .NET 4.0 Full
    if version = 'v4.7' then begin
        version := 'v4\Full';
        check47 := true;
    end else
        check47 := false;

    // installation key group for all .NET versions
    key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\' + version;

    // .NET 3.0 uses value InstallSuccess in subkey Setup
    if Pos('v3.0', version) = 1 then begin
        success := RegQueryDWordValue(HKLM, key + '\Setup', 'InstallSuccess', install);
    end else begin
        success := RegQueryDWordValue(HKLM, key, 'Install', install);
    end;

    // .NET 4.0/4.5 uses value Servicing instead of SP
    if Pos('v4', version) = 1 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Servicing', serviceCount);
    end else begin
        success := success and RegQueryDWordValue(HKLM, key, 'SP', serviceCount);
    end;

    // .NET 4.5 uses additional value Release
    if check47 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Release', release);
        success := success and (release >= 378389);
    end;

    result := success and (install = 1) and (serviceCount >= service);
end;
