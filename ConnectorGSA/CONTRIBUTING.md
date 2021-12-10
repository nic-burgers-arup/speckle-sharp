# Contributing

Thank you for sharing back to the SpeckleGSA project!

These guidelines are simple and should make it easy for you to share your improvements to SpeckleGSA with the Speckle and GSA communities!

All contributions are welcome: bug fixes, documentation, tutorials, new features!

Please set up a pull request to the `dev` branch for all contributions - this will let us discuss and review the changes before incorporating them into the main code base. If this is your first time contributing to SpeckleGSA, please fork the repo and set up a merge request from your repo.

Check out [http://makeapullrequest.com/]() or [https://www.firsttimersonly.com/]() if you have never contributed to an open source project before!

All contributions should be make SpeckleGSA better, so please make sure that your changes do not break SpeckleGSA, or remove existing functionality.

If you would like to make a significant change, please open an issue to discuss it first and be sure to tag [daviddekoning](https://github.com/daviddekoning), [nic-burgers-arup](https://github.com/nic-burgers-arup) and [jenessaman](https://github.com/jenessaman).

Please also take a look at the specklesystems [contribution guide](https://github.com/specklesystems/speckle-sharp/blob/main/.github/CONTRIBUTING.md)!

## Building SpeckleGSA

### Requirements

- Visual Studio 2019
- .NET Framework 4.7.1

### Dev Notes

The SpeckleGSA solution is currently made up of the following projects:
- ConnectorGSA: main project with receiver, sender and user interface
- ConverterGSA: ToSpeckle and ToNative conversion methods for GSA objects and Speckle structural objects
- Speckle.Proxy: a set of Gwa parsers and helper methods
- Speckle.GSA.API: methods and helpers for working with the GSA COM API 
- Core: the canonical SDK for Speckle v2
- Objects: the Speckle objects kit (containing the structural object classes)
- PolygonMesher: a utility for generating meshes from input vertices
- Test projects for the Connector and conversion methods

### Building Process

SpeckleGSA requires that Oasys GSA and Speckle (Speckle v2, not v1!) are installed. Install Speckle v2 by following specklesystems getting started guide [here](https://speckle.systems/getstarted/). 

*For Arup staff - install Speckle v2 using the Speckle@Arup Installer, which can be found in ArupApps or on GitHub [here](https://github.com/arup-group/speckle-sharp/releases).*

Then:
- Clone/fork the repo
- Restore all Nuget packages missing on the solution
- Set ConnectorGSA as start project and rebuild all

### Release process

To build a new version of the SpeckleGSA installer, you can run the Create Installer for GSA Connector [workflow](https://github.com/arup-group/speckle-sharp/actions/workflows/installer-creator-gsa.yml) in GitHub Actions. This will prepare a installer for SpeckleGSA and the Speckle Objects Kit and create a new pre-release with the newly created installer. 
