;defining variables
#define AppName      "Speckle@Arup v2 Bentley Connectors"
#define MicroStationVersion  GetFileVersion("..\ConnectorBentley\ConnectorMicroStation\bin\Release\SpeckleConnectorMicroStation.dll")
#define OpenRoadsVersion  GetFileVersion("..\ConnectorBentley\ConnectorOpenRoads\bin\Release\SpeckleConnectorOpenRoads.dll")
#define OpenRailVersion  GetFileVersion("..\ConnectorBentley\ConnectorOpenRail\bin\Release\SpeckleConnectorOpenRail.dll")
#define OpenBuildingsVersion  GetFileVersion("..\ConnectorBentley\ConnectorOpenBuildings\bin\Release\SpeckleConnectorOpenBuildings.dll")
#define OpenBridgeVersion  GetFileVersion("..\ConnectorBentley\ConnectorOpenBridge\bin\Release\SpeckleConnectorOpenBridge.dll")
#define AppPublisher "Speckle@Arup"
#define AppURL       "https://speckle.arup.com"
#define SpeckleFolder "{localappdata}\Speckle"

[Setup]
AppId="1c19cd70-461d-4958-bec6-7270bb4fcdbd"
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
OutputBaseFilename=Speckle@ArupBentleyConnectors-v{#AppVersion}
SetupIconFile=..\Installer\ConnectionManager\SpeckleConnectionManagerUI\Assets\favicon.ico
Compression=lzma
SolidCompression=yes
ChangesAssociations=yes
PrivilegesRequired=lowest
VersionInfoVersion={#AppVersion}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Components]
Name: microstation; Description: Speckle for MicroStation CONNECT Edition Update 14 - v{#MicroStationVersion};  Types: full
Name: openroads; Description: Speckle for OpenRoads Designer CONNECT Edition 2020 R3 - v{#OpenRoadsVersion};  Types: full
Name: openrail; Description: Speckle for OpenRail Designer CONNECT Edition 2020 R3 - v{#OpenRailVersion};  Types: full
Name: openbuildings; Description: Speckle for OpenBuildings Designer CONNECT Edition Update 6 - v{#OpenBuildingsVersion};  Types: full
Name: openbridge; Description: Speckle for OpenBridge Modeller CE - 2021 Release 1 Update 10 - v{#OpenBridgeVersion};  Types: full
Name: kits; Description: Speckle Kits (for MicroStation, OpenRoads, OpenRail, OpenBridge and OpenBuildings) - v{#AppVersion};  Types: full custom; Flags: fixed

[Types]
Name: "full"; Description: "Full installation"
Name: "custom"; Description: "Custom installation"; Flags: iscustom

[Dirs]
Name: "{app}"; Permissions: everyone-full

[Files]
;microstation
Source: "..\ConnectorBentley\ConnectorMicroStation\bin\Release\*"; DestDir: "{userappdata}\Bentley\MicroStation\Addins\Speckle2MicroStation\"; Flags: ignoreversion recursesubdirs; Components: microstation
Source: "..\Objects\Converters\ConverterBentley\ConverterMicroStation\bin\Release\netstandard2.0\Objects.Converter.MicroStation.dll"; DestDir: "{userappdata}\Speckle\Kits\Objects"; Flags: ignoreversion recursesubdirs; Components: microstation
Source: "..\ConnectorBentley\ConnectorMicroStation\bin\Release\Speckle2MicroStation.cfg"; DestDir: "{commonappdata}\Bentley\Microstation CONNECT Edition\Configuration\Organization"; Flags: ignoreversion recursesubdirs; Components: microstation

;openroads
Source: "..\ConnectorBentley\ConnectorOpenRoads\bin\Release\*"; DestDir: "{userappdata}\Bentley\OpenRoadsDesigner\Addins\Speckle2OpenRoads\"; Flags: ignoreversion recursesubdirs; Components: openroads
Source: "..\Objects\Converters\ConverterBentley\ConverterOpenRoads\bin\Release\netstandard2.0\Objects.Converter.OpenRoads.dll"; DestDir: "{userappdata}\Speckle\Kits\Objects"; Flags: ignoreversion recursesubdirs; Components: openroads
Source: "..\ConnectorBentley\ConnectorOpenRoads\bin\Release\Speckle2OpenRoads.cfg"; DestDir: "{commonappdata}\Bentley\OpenRoads Designer CE\Configuration\Organization"; Flags: ignoreversion recursesubdirs; Components: openroads

;openrail
Source: "..\ConnectorBentley\ConnectorOpenRail\bin\Release\*"; DestDir: "{userappdata}\Bentley\OpenRailDesigner\Addins\Speckle2OpenRail\"; Flags: ignoreversion recursesubdirs; Components: openrail
Source: "..\Objects\Converters\ConverterBentley\ConverterOpenRail\bin\Release\netstandard2.0\Objects.Converter.OpenRail.dll"; DestDir: "{userappdata}\Speckle\Kits\Objects"; Flags: ignoreversion recursesubdirs; Components: openrail
Source: "..\ConnectorBentley\ConnectorOpenRail\bin\Release\Speckle2OpenRail.cfg"; DestDir: "{commonappdata}\Bentley\OpenRail Designer CE\Configuration\Organization"; Flags: ignoreversion recursesubdirs; Components: openrail

;openbuildings
Source: "..\ConnectorBentley\ConnectorOpenBuildings\bin\Release\*"; DestDir: "{userappdata}\Bentley\OpenBuildingsDesigner\Addins\Speckle2OpenBuildings\"; Flags: ignoreversion recursesubdirs; Components: openbuildings
Source: "..\Objects\Converters\ConverterBentley\ConverterOpenBuildings\bin\Release\netstandard2.0\Objects.Converter.OpenBuildings.dll"; DestDir: "{userappdata}\Speckle\Kits\Objects"; Flags: ignoreversion recursesubdirs; Components: openbuildings
Source: "..\ConnectorBentley\ConnectorOpenBuildings\bin\Release\Speckle2OpenBuildings.cfg"; DestDir: "{commonappdata}\Bentley\OpenBuildings CONNECT Edition\Configuration\Organization"; Flags: ignoreversion recursesubdirs; Components: openbuildings

;openbridge
Source: "..\ConnectorBentley\ConnectorOpenBridge\bin\Release\*"; DestDir: "{userappdata}\Bentley\OpenBridgeModeler\Addins\Speckle2OpenBridge\"; Flags: ignoreversion recursesubdirs; Components: openbridge
Source: "..\Objects\Converters\ConverterBentley\ConverterOpenBridge\bin\Release\netstandard2.0\Objects.Converter.OpenBridge.dll"; DestDir: "{userappdata}\Speckle\Kits\Objects"; Flags: ignoreversion recursesubdirs; Components: openbridge
Source: "..\ConnectorBentley\ConnectorOpenBridge\bin\Release\Speckle2OpenBridge.cfg"; DestDir: "{commonappdata}\Bentley\OpenBridge Modeler CE 10.10\Configuration\Organization"; Flags: ignoreversion recursesubdirs; Components: openbridge

;kits
Source: "..\Objects\Objects\bin\Release\netstandard2.0\Objects.dll"; DestDir: "{userappdata}\Speckle\Kits\Objects"; Flags: ignoreversion recursesubdirs; Components: kits

[InstallDelete]
Type: filesandordirs; Name: "{userappdata}\Bentley\MicroStation\Addins\Speckle2MicroStation\*"
Type: filesandordirs; Name: "{userappdata}\Bentley\OpenRoadsDesigner\Addins\Speckle2OpenRoads\*"
Type: filesandordirs; Name: "{userappdata}\Bentley\OpenRailDesigner\Addins\Speckle2OpenRail\*"
Type: filesandordirs; Name: "{userappdata}\Bentley\OpenBuildingsDesigner\Addins\Speckle2OpenBuildings\*"
Type: filesandordirs; Name: "{userappdata}\Bentley\OpenBridgeModeler\Addins\Speckle2OpenBridge\*"
Type: files; Name: "{userappdata}\Speckle\Kits\Objects\Objects.Converter.MicroStation.dll"
Type: files; Name: "{userappdata}\Speckle\Kits\Objects\Objects.Converter.OpenRoads.dll"
Type: files; Name: "{userappdata}\Speckle\Kits\Objects\Objects.Converter.OpenRail.dll"
Type: files; Name: "{userappdata}\Speckle\Kits\Objects\Objects.Converter.OpenBuildings.dll"
Type: files; Name: "{userappdata}\Speckle\Kits\Objects\Objects.Converter.OpenBridge.dll"

[Icons]
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"