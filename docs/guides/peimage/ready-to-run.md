# .NET ReadyToRun Directory

ReadyToRun (RTR or R2R) is a technology where managed method bodies written
in the Common Intermediate Language (CIL) are compiled ahead of time (AoT).
This can speed up startup times of a .NET application, as the program is not
required to JIT compile the pre-compiled method bodies at runtime anymore.

Low level ReadyToRun metadata can be found in the .NET data directory, under
the managed native header entry. AsmResolver provides rich support for various
ReadyToRun metadata structures found in this header.

To test whether a PE image has ReadyToRun metadata, query the
`ManagedNativeHeader` property and test if it is of type `ReadyToRunDirectory`:

```csharp
using AsmResolver.PE.DotNet.ReadyToRun;

IPEImage image = ...

var header = image.DotNetDirectory.ManagedNativeHeader;
if (header is ReadyToRunDirectory directory)
{
    // Application has ReadyToRun metadata.
}
```

In the following, we will assume `directory` is the root `ReadyToRunDirectory`
instance obtained in a similar manner.


## Sections

The ReadyToRun data directory consists of various sections, which can be accessed
through the `Sections` property.

```csharp
ReadyToRunDirectory directory = ...

for (int i = 0; i < directory.Sections.Count; i++)
    Console.WriteLine($"{i}: {directory.Sections[i].Type}");
```

`Sections` is mutable. New sections can be added and existing ones can be removed:
```csharp
directory.Sections.Add(new CustomReadyToRunSection(...));
directory.Sections.RemoveAt(0);
```

Individual sections can be obtained by the `GetSection` method:

```csharp
var section = directory.GetSection(ReadyToRunSectionType.CompilerIdentifier);
```

```csharp
var section = directory.GetSection<CompilerIdentifierSection>();
```

`GetSection` throws if the section is not present in the directory.
Alternatively, it is possible to use the non-throwing `TryGetSection`
that returns a `bool` instead:

```csharp
if (directory.TryGetSection(ReadyToRunSectionType.CompilerIdentifier, out var section))
{
    // Section exists.
}
```

```csharp
if (directory.TryGetSection(out CompilerIdentifierSection? section))
{
    // Section exists.
}
```

The raw binary contents of many sections can be read using `CreateReader`,
which returns a `BinaryStreamReader`:

```csharp
if (section.CanRead)
{
    var reader = section.CreateReader();
    // parse data here...
}
```

AsmResolver provides parsing for various sections out of the box. All
supported section formats have a designated class that interpret and expose
the data stored in these sections. Any other unsupported section is represented
using `CustomReadyToRunSection` exposing the raw data as an `ISegment`.


## Runtime Functions

References to the precompiled native code of managed methods is stored in the
`RuntimeFunctions` section, represented by the `RuntimeFunctionsSection` class.

```csharp
var section = directory.GetSection<RuntimeFunctionsSection>();

foreach (var function in section.GetFunctions())
    Console.WriteLine($"Rva: {function.Begin.Rva:X8}");
```

To start reading the native code of this function, use `CreateReader` on the
exposed `ISegmentReference`s:

```csharp
if (function.Begin.CanRead)
{
    var reader = function.Begin.CreateReader();
    // ...
}
```

The remaining format of each function follows the `RUNTIME_FUNCTION` structure
as found in the Exceptions Data Directory of a PE file, and thus is
platform-specific.

Below an example can be found for inspecting fields such as unwind info in this
structure for AMD64 PEs:

```csharp
var section = directory.GetSection<RuntimeFunctionsSection>();

foreach (var function in section.GetFunctions().OfType<X64RuntimeFunction>())
{
    Console.WriteLine($"Rva: {function.Begin.Rva:X8}");

    var unwindInfo = function.UnwindInfo;
    Console.WriteLine($"Size of Prolog: {function.UnwindInfo.SizeOfProlog}");
}
```

Refer to the documentation of the [Exceptions Directory](exceptions.md) for
ways to casting and interpreting each supported specific formats.


## Method Entry Points

Method entry points describe the starting runtime function and references
fixups in the `ImportSections` section that need to be called when calling a
pre-compiled managed method. In AsmResolver, entry points are exposed by the
`MethodEntryPointsSection` class.

```csharp
var section = GetSection<MethodEntryPointsSection>();
```

The method entry points section is ordered in such a way that the `i`-th entry
point maps to the `i`-th method in the method table. Note that not every method
is required to have an entry point specified. Some entries in this list
may therefore be `null`.

Below an example that iterates all entry points and their fixups:

```csharp
for (int i = 0; i < section.EntryPoints.Count; i++)
{
    var token = new MetadataToken(TableIndex.Method, (uint) (i + 1));
    var entryPoint = section.EntryPoints[i];

    if (entryPoint is null)
    {
        Console.WriteLine($"Method {token} is not mapped to a runtime function.");
    }
    else
    {
        Console.WriteLine($"Method {token} is mapped to runtime function {entryPoint.RuntimeFunctionIndex}.");
        foreach (var fixup in entryPoint.Fixups)
            Console.WriteLine($"- Import {fixup.ImportIndex}, Slot {fixup.SlotIndex}");
    }
}
```

## Import Sections

Method entry points reference fixups defined in the import section. The
imports can be extracted using the `ImportSectionsSection` class.

```csharp
var section = directory.GetSection<ImportSectionsSection>();
```

This section contains various sub-sections of type `ImportSection`.
Every section contains a set of `Slots`, a list of references that will
receive addresses to strings, IL code or other stub dispatches.

```csharp
foreach (var import in section.Sections)
{
    Console.WriteLine($"Import Type: {import.Type}");
    for (int i = 0; i < import.Slots.Count; i++)
        Console.WriteLine($" {i}: {import.Slots[i].Rva}");
}
```

Some sections have a parallel list of signature binary blobs, which
describe the general shape and parameters of the associated slot.

```csharp
foreach (var import in section.Sections)
{
    Console.WriteLine($"Import Type: {import.Type}");

    for (int i = 0; i < import.Slots.Count; i++)
    {
        Console.WriteLine($" {i}: {import.Slots[i].Rva}");

        if (i < import.Signatures.Count && import.Signatures[i].CanRead)
        {
            var reader = import.Signatures[i].CreateReader();
            // Parse signature here ...
        }
    }
}
```

> [!NOTE]
> AsmResolver does not provide any high-level parsing on this level of
> abstraction. It is expected to be added to the `AsmResolver.DotNet` package
> in the future.
