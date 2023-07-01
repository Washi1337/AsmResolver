# Basic I/O

Every .NET image interaction is done through classes defined by the
`AsmResolver.DotNet` namespace:

``` csharp
using AsmResolver.DotNet;
```

## Creating a new .NET module

Creating a new image can be done by instantiating a `ModuleDefinition`
class:

``` csharp
var module = new ModuleDefinition("MyModule.exe");
```

The above will create a module that references mscorlib.dll 4.0.0.0
(.NET Framework 4.0). If another version of the Common Object Runtime
Library is desired, we can use one of the overloads of the constructor,
and use a custom `AssemblyReference`, or one of the pre-defined assembly
references in the `KnownCorLibs` class to target another version of the
library.

``` csharp
var module = new ModuleDefinition("MyModule.dll", KnownCorLibs.SystemRuntime_v4_2_2_0);
```

If you have a .NET runtime identifier as specified in the 
`TargetFrameworkAttribute` of an assembly, you can use the `DotNetRuntimeInfo` 
structure to get the corresponding default corlib:

``` csharp
var runtime = DotNetRuntimeInfo.Parse(".NETCoreApp,Version=v3.1");
var module = new ModuleDefinition("MyModule.dll", runtime.GetDefaultCorLib());
```

## Opening a .NET module

Opening a .NET module can be done through one of the `FromXXX` methods
from the `ModuleDefinition` class:

``` csharp
byte[] raw = ...
var module = ModuleDefinition.FromBytes(raw);
```

``` csharp
var module = ModuleDefinition.FromFile(@"C:\myfile.exe");
```

``` csharp
PEFile peFile = ...
var module = ModuleDefinition.FromFile(peFile);
```

``` csharp
BinaryStreamReader reader = ...
var module = ModuleDefinition.FromReader(reader);
```

``` csharp
IPEImage peImage = ...
var module = ModuleDefinition.FromImage(peImage);
```

If you want to read large files (+100MB), consider using memory mapped
I/O instead:

``` csharp
using var service = new MemoryMappedFileService();
var module = ModuleDefinition.FromFile(service.OpenFile(@"C:\myfile.exe"));
```

On Windows, if a module is loaded and mapped in memory (e.g. as a
dependency defined in Metadata or by the means of `System.Reflection`),
it is possible to load the module from memory by using `FromModule`, or
by transforming the module into a `HINSTANCE` and then providing it to
the `FromModuleBaseAddress` method:

``` csharp
Module module = ...;
var module = ModuleDefinition.FromModule(module);
```

``` csharp
Module module = ...;
IntPtr hInstance = Marshal.GetHINSTANCE(module);
var module = ModuleDefinition.FromModuleBaseAddress(hInstance);
```

## Writing a .NET module

Writing a .NET module can be done through one of the `Write` method
overloads.

``` csharp
module.Write(@"C:\myfile.patched.exe");
```

``` csharp
Stream stream = ...;
module.Write(stream);
```

For more advanced options to write .NET modules, see 
[Advanced PE Image Building](advanced-pe-image-building.md).

## Creating a new .NET assembly

AsmResolver also supports creating entire (multi-module) .NET assemblies
instead.

``` csharp
var assembly = new AssemblyDefinition("MyAssembly", new Version(1, 0, 0, 0));
```

## Opening a .NET assembly

Opening (multi-module) .NET assemblies can be done in a very similar
fashion as reading a single module:

``` csharp
byte[] raw = ...
var assembly = AssemblyDefinition.FromBytes(raw);
```

``` csharp
var assembly = AssemblyDefinition.FromFile(@"C:\myfile.exe");
```

``` csharp
IPEFile peFile = ...
var assembly = AssemblyDefinition.FromFile(peFile);
```

``` csharp
BinaryStreamReader reader = ...
var assembly = AssemblyDefinition.FromReader(reader);
```

``` csharp
IPEImage peImage = ...
var assembly = AssemblyDefinition.FromImage(peImage);
```

Similar to reading module definitions, if you want to read large files
(+100MB), consider using memory mapped I/O instead:

``` csharp
using var service = new MemoryMappedFileService();
var assembly = AssemblyDefinition.FromFile(service.OpenFile(@"C:\myfile.exe"));
```

## Writing a .NET assembly

Writing a .NET assembly can be done through one of the `Write` method
overloads.

``` csharp
assembly.Write(@"C:\myfile.patched.exe");
```

For more advanced options to write .NET assemblies, see 
[Advanced PE Image Building](advanced-pe-image-building.md).
