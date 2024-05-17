# AsmResolver

 [![Master branch build status](https://img.shields.io/appveyor/ci/Washi1337/AsmResolver/master.svg)](https://ci.appveyor.com/project/Washi1337/asmresolver/branch/master)
 [![Nuget feed](https://img.shields.io/nuget/v/AsmResolver.svg)](https://www.nuget.org/packages/AsmResolver/)
 [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
 [![Discord](https://img.shields.io/discord/961647807591243796.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2)](https://discord.gg/Y7DTBkbhJJ)

AsmResolver is a Portable Executable (PE) inspection library that is able to read, modify and write executable files. This includes .NET modules as well as native images. The library exposes high-level representations of the PE, while still allowing the user to access low-level structures.

AsmResolver is released under the MIT license.


## Main Features

AsmResolver has a lot of features. Below is a non-exhaustive list of the highlights:

- [x] Create, read, modify, write and patch PE files.
  - [x] Full access to sections, data directories and their interpretations.
- [x] Rich support for .NET modules with an intuitive API similar to `System.Reflection`.
  - [x] Managed, native and dynamic method body support.
  - [x] Easy metadata importing and cloning.
  - [x] Managed resource file serializers and deserializers.
  - [x] Support for AppHost / SingleFileHost bundles.
  - [x] Support for ReadyToRun binaries.
- [x] Read PDB symbols.
  - [x] Fully managed cross-platform API (No DIA or similar required).
- [x] .NET 3.5 compatible.
- [x] Documented.
- [x] Unit tested.


## Documentation

- [Guides](https://docs.washi.dev/asmresolver)
- [API Reference](https://docs.washi.dev/asmresolver/api/core/AsmResolver.html)


## Binaries

Stable Builds:

- [NuGet Feed](https://www.nuget.org/packages/AsmResolver/)
- [GitHub Releases](https://github.com/Washi1337/AsmResolver/releases)

Nightly Builds:

- [AppVeyor](https://ci.appveyor.com/project/Washi1337/asmresolver/build/artifacts)

| Branch | Build status |
|--------|--------|
| master | [![Master branch build status](https://img.shields.io/appveyor/ci/Washi1337/AsmResolver/master.svg)](https://ci.appveyor.com/project/Washi1337/asmresolver/branch/master) |
| development | [![Development branch build status](https://img.shields.io/appveyor/ci/Washi1337/AsmResolver/development.svg)](https://ci.appveyor.com/project/Washi1337/asmresolver/branch/development)


## Compiling

The solution can be built using the .NET SDK or an IDE that works with it (e.g., Visual Studio and JetBrains Rider). The main packages target LTS versions of various .NET runtimes (.NET 3.5, .NET Standard 2.0, .NET Standard 2.1, .NET Core 3.1 and .NET 6.0).

To build the project from the command line, use:
```bash
$ dotnet build
```

To run all tests, use:
```bash
$ dotnet test
```
For running the tests successfully, you will need to have various versions of .NET installed (ranging from .NET Framework to .NET Core 3.1 and .NET 5+), as the unit tests verify reading binaries targeting various .NET runtimes.


## Contributing

- See [CONTRIBUTING.md](CONTRIBUTING.md).


## Support

- [Issue Tracker](https://github.com/Washi1337/AsmResolver/issues)
- [Discussion Board](github.com/washi1337/asmresolver/discussions)
- [Discord](https://discord.gg/Y7DTBkbhJJ)


## Acknowledgments

AsmResolver started as a hobby project but has grown into a community project with various contributors. Without these people, AsmResolver would not have been where it is today!

- Special thanks to all the people who contributed [directly with code commits](https://github.com/Washi1337/AsmResolver/graphs/contributors).

- Another big thank you to all the people that suggested new features, provided feedback on the API design, have done extensive testing, and/or reported bugs on the [issue board](https://github.com/Washi1337/AsmResolver/issues), by e-mail, or through DMs.

If you feel you have been under-represented in these acknowledgments, feel free to reach out.
