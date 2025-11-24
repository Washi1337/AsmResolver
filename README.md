# AsmResolver

 [![Test and Publish](https://github.com/Washi1337/AsmResolver/actions/workflows/test-and-publish.yml/badge.svg)](https://github.com/Washi1337/AsmResolver/actions/workflows/test-and-publish.yml)
 [![Nuget feed](https://img.shields.io/nuget/v/AsmResolver)](https://www.nuget.org/packages/AsmResolver/)
 [![Nuget feed](https://img.shields.io/nuget/vpre/AsmResolver)](https://www.nuget.org/packages/AsmResolver/)
 [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
 [![Discord](https://img.shields.io/discord/961647807591243796.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2)](https://discord.gg/Y7DTBkbhJJ)

AsmResolver is a library for reading, modifying and reconstructing Portable Executable (PE) files. It supports PE images running natively on Windows, as well as images containing managed (.NET) metadata.

AsmResolver is released under the MIT license.


## Main Features

AsmResolver has a lot of features. Below is a non-exhaustive list of the highlights:

- [x] Create, read, modify, write and patch PE files.
  - [x] Full access to sections, data directories and their interpretations.
  - [x] Import Address Table (IAT) reconstruction and trampolining.
  - [x] Full control over the layout of the final PE file output.
- [x] Rich support for various Win32 resource types.
- [x] Rich support for .NET metadata with an intuitive API similar to `System.Reflection`.
  - [x] Managed, native and dynamic method body support.
  - [x] Easy metadata importing and cloning.
  - [x] Managed resource file serializers and deserializers.
  - [x] Support for AppHost / SingleFileHost bundles.
  - [x] Support for ReadyToRun (R2R) binaries.
- [x] Build to be robust against malformed/obfuscated binaries.
- [x] Rich read support for PDB and PortablePdb symbols.
  - [x] Fully managed cross-platform API (No DIA or similar required).
- [x] .NET 3.5 compatible.
- [x] Cross-platform (Windows and *nix, .NET standard 2.0 and Mono compatible).
- [x] Documented.
- [x] Unit tested.


## Documentation

- [Guides](https://docs.washi.dev/asmresolver)
- [API Reference](https://docs.washi.dev/asmresolver/api/core/AsmResolver.html)


## Support

- [Issue Tracker](https://github.com/Washi1337/AsmResolver/issues)
- [Discussion Board](https://github.com/washi1337/asmresolver/discussions)
- [Discord](https://discord.gg/Y7DTBkbhJJ)

## Binaries

Stable Builds:

- [NuGet Feed](https://www.nuget.org/packages/AsmResolver/)
- [GitHub Releases](https://github.com/Washi1337/AsmResolver/releases)

Nightly Builds:

- [Nightly Nuget Feed](https://nuget.washi.dev/) (https://nuget.washi.dev/v3/index.json)

| Branch | Build status |
|--------|--------|
| master | [![Test and Publish](https://github.com/Washi1337/AsmResolver/actions/workflows/test-and-publish.yml/badge.svg?branch=master)](https://github.com/Washi1337/AsmResolver/actions/workflows/test-and-publish.yml) |
| development | [![Test and Publish](https://github.com/Washi1337/AsmResolver/actions/workflows/test-and-publish.yml/badge.svg?branch=development)](https://github.com/Washi1337/AsmResolver/actions/workflows/test-and-publish.yml) |

## Compiling

The solution can be built using the .NET SDK or an IDE that works with it (e.g., Visual Studio and JetBrains Rider). The main packages target LTS versions of various .NET runtimes (.NET 3.5, .NET Standard 2.0, .NET Standard 2.1, .NET Core 3.1, .NET 6.0, .NET 8.0, .NET 9.0, .NET 10.0).

To build the project from the command line, use:
```bash
$ dotnet build
```

To run all tests, use:
```bash
$ dotnet test
```
For running the tests successfully, you will need to have additional versions of the .NET _runtime_ installed (including STS versions or versions declared EOL), as the unit tests verify reading binaries targeting various .NET runtimes.
To run the tests successfully on MacOS and Linux, `mono` and `wine` are expected to be installed as well.


## Contributing

- See [CONTRIBUTING.md](CONTRIBUTING.md).


## Acknowledgments

AsmResolver started as a hobby project but has grown into a community project with various contributors. Without these people, AsmResolver would not have been where it is today!

- Special thanks to all the people who contributed [directly with code commits](https://github.com/Washi1337/AsmResolver/graphs/contributors) or monetarily via [GitHub sponsors](https://github.com/sponsors/Washi1337).

- Special thanks to the people at [@MonoMod](https://github.com/MonoMod) for helping with .NET 3.5 compatibility.

- Another big thank you to all the people that suggested new features, provided feedback on the API design, have done extensive testing, and/or reported bugs on the [issue board](https://github.com/Washi1337/AsmResolver/issues), by e-mail, or through DMs.

If you feel you have been under-represented in these acknowledgments, feel free to reach out.
