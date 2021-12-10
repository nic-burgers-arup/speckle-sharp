# SpeckleGSA
SpeckleGSA is a plugin for [GSA 10.1](https://www.oasys-software.com/products/structural/gsa/) that connects it to the [Speckle](https://speckle.systems) ecosystem.

## Contents

- [SpeckleGSA](#specklegsa)
  - [Contents](#contents)
  - [Installation](#installation)
  - [Usage](#usage)
  - [Bugs and Feature Requests](#bugs-and-feature-requests)
  - [Contributing to SpeckleGSA](#contributing-to-specklegsa)
  - [About Speckle](#about-speckle)
  - [Notes](#notes)

## Installation

New releases of SpeckleGSA can be found [here](https://github.com/arup-group/speckle-sharp/releases).

The SpeckleGSA connector and the accompanying Objects Kit (the default objects kit) are bundled in a single installer.

![image](https://user-images.githubusercontent.com/69314485/145598135-11410379-c682-4d22-b991-67f831896ef9.png)

## Usage

Once SpeckleGSA is installed, run the program directly. It will take care of opening GSA - you cannot access SpeckleGSA if you run GSA directly.

SpeckleGSA implements key components of a Speckle client in it's tab interface:
- Server:
    - Allows users to login to a SpeckleServer
- GSA:
    - Create or open a GSA file
- Sender:
    - Sends model to a SpeckleServer
- Receiver:
    - Receive stream(s) from a SpeckleServer
- Settings

## Bugs and Feature Requests

SpeckleGSA is still currently under development which can cause many quick changes to occur. If there are any major bugs, please submit a new [issue](https://github.com/arup-group/speckle-sharp/issues).

## Contributing to SpeckleGSA

Checkout the [Contribution Guidelines](https://github.com/arup-group/speckle-sharp/blob/master/ConnectorGSA/CONTRIBUTING.md) for guidance on compiling SpeckleGSA and how to share your changes back to the community!

## About Speckle

Speckle reimagines the design process from the Internet up: an open source (Apache-2.0) initiative for developing an extensible Design & AEC data communication protocol and platform. Contributions are welcome - we can't build this alone!

## Notes

SpeckleGSA is developed and maintained by [Nic Burgers](https://github.com/nic-burgers-arup), [Gerard Taig](https://github.com/Gerard-Taig) and the Speckle@Arup team.
