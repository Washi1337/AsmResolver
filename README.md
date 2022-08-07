# AsmResolver

 [![Master branch build status](https://img.shields.io/appveyor/ci/Washi1337/AsmResolver/master.svg)](https://ci.appveyor.com/project/Washi1337/asmresolver/branch/master)
 [![Nuget feed](https://img.shields.io/nuget/v/AsmResolver.svg)](https://www.nuget.org/packages/AsmResolver/)
 [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
 [![Documentation Status](https://readthedocs.org/projects/asmresolver/badge/?version=latest)](https://asmresolver.readthedocs.io/en/latest/?badge=latest)
 [![Discord](https://img.shields.io/discord/961647807591243796.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2)](https://discord.gg/Y7DTBkbhJJ)

AsmResolver is a PE inspection library allowing .NET programmers to read, modify and write executable files. This includes .NET as well as native images. The library exposes high-level representations of the PE, while still allowing the user to access low-level structures.

AsmResolver is released under the MIT license.


## Features

AsmResolver has a lot of features. Below a non-exhaustive list:

- [x] Create, read and write PE files
  - [x] Inspect and update PE headers.
  - [x] Create, read and write sections.
- [x] Create, read and write various data directories
  - [x] Debug Directory (CodeView)
  - [x] .NET Directory
    - [x] CIL assembler and disassemblers
    - [x] Metadata Directory (tables, strings, user-strings, blobs, GUIDs)
    - [x] Resources Directory
    - [x] Strong Name Signing
    - [x] VTable Fixup Directory
  - [x] Exception Directory (AMD64)
  - [x] Export Directory
  - [x] Import Directory
  - [x] Base Relocation Directory
  - [x] TLS Directory
  - [x] Win32 Resources Directory
- [x] Fully mutable object model for .NET modules that is similar to System.Reflection
  - [x] Strong type-system with many useful factory methods for quickly constructing new metadata.
  - [x] Full metadata importing and cloning (useful for injecting metadata into another assembly)
  - [x] .NET Framework 2.0+, .NET Core and .NET 5+ binary file support.
  - [x] Infer memory layout of types statically.
  - [x] Create, read and write managed resource sets (`.resources` files)
  - [x] Create new method bodies containing native code.
  - [x] Highly configurable reader and writer options and custom error handling for both.
  - [x] Rich support for AppHost and SingleFileHost bundled files.


## Documentation

Check out the [wiki](https://asmresolver.readthedocs.org/) for guides and information on how to use the library.


## Binaries

Stable builds:

- [GitHub releases](https://github.com/Washi1337/AsmResolver/releases)
- [NuGet feed](https://www.nuget.org/packages/AsmResolver/)

Nightly builds:

- [AppVeyor](https://ci.appveyor.com/project/Washi1337/asmresolver/build/artifacts)

| Branch | Build status |
|--------|--------|
| master | [![Master branch build status](https://img.shields.io/appveyor/ci/Washi1337/AsmResolver/master.svg)](https://ci.appveyor.com/project/Washi1337/asmresolver/branch/master) |
| development | [![Development branch build status](https://img.shields.io/appveyor/ci/Washi1337/AsmResolver/development.svg)](https://ci.appveyor.com/project/Washi1337/asmresolver/branch/development)


## Compiling

The solution can be build using the .NET SDK or an IDE that works with the .NET SDK (such as Visual Studio and JetBrains Rider). The main packages target .NET Standard 2.0, and the xUnit test projects target .NET Core 3.1.

To build the project from the commandline, use:
```bash
$ dotnet restore
$ dotnet build
```

To run all tests, simply run:
```bash
$ dotnet test
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on general workflow and code style.


## Found a bug or have questions?

Please use the [issue tracker](https://github.com/Washi1337/AsmResolver/issues). Try to be as descriptive as possible.

You can also join the [Discord](https://discord.gg/Y7DTBkbhJJ) to engage more directly with the community.

## Acknowledgements

AsmResolver started out as a hobby project, but has grown into a community project with various contributors. Without these people, AsmResolver would not have been where it is today!

- Special thanks to all the people who contributed [directly with code commits](https://github.com/Washi1337/AsmResolver/graphs/contributors).

- Another big thank you to all the people that suggested new features, provided feedback on the API design, have done extensive testing, and/or reported bugs on the [issue board](https://github.com/Washi1337/AsmResolver/issues), by e-mail, or through DMs.

If you feel you have been under-represented in these acknowledgements, feel free to contact me.
