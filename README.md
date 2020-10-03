AsmResolver
===========

 [![Master branch build status](https://img.shields.io/appveyor/ci/Washi1337/AsmResolver/master.svg)](https://ci.appveyor.com/project/Washi1337/asmresolver/branch/master) [![Nuget feed](https://img.shields.io/nuget/v/AsmResolver.svg)](https://www.nuget.org/packages/AsmResolver/)  [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) [![Documentation Status](https://readthedocs.org/projects/asmresolver/badge/?version=latest)](https://asmresolver.readthedocs.io/en/latest/?badge=latest)

AsmResolver is a PE inspection library allowing .NET programmers to read, modify and write executable files. This includes .NET as well as native native images. The library exposes high-level representations of the PE, while still allowing the user to access low-level structures.


AsmResolver is released under the MIT license.

Binaries
-----------
Get the latest stable build from the [nuget feed](https://www.nuget.org/packages/AsmResolver/), or download the bleeding edge build from [appveyor](https://ci.appveyor.com/project/Washi1337/asmresolver/build/artifacts).

| Branch | Build status |
|--------|--------|
| master | [![Master branch build status](https://img.shields.io/appveyor/ci/Washi1337/AsmResolver/master.svg)](https://ci.appveyor.com/project/Washi1337/asmresolver/branch/master) |
| development | [![Development branch build status](https://img.shields.io/appveyor/ci/Washi1337/AsmResolver/development.svg)](https://ci.appveyor.com/project/Washi1337/asmresolver/branch/development)

Alternatively, you can build the project yourself using MSBuild or an IDE that works with MSBuild (such as Visual Studio and JetBrains Rider), assuming you have the .NET Core toolchain installed. If you want to compile the test libraries as well, make sure to also restore all nuget packages.

Documentation
-------------
Check out the [wiki](https://asmresolver.readthedocs.org/) for guides and information on how to use the library!

Contributing
------------
See [CONTRIBUTING.md](CONTRIBUTING.md).

Found a bug or have questions?
------------------------------
Please use the [issue tracker](https://github.com/Washi1337/AsmResolver/issues). Try to be as descriptive as possible.

