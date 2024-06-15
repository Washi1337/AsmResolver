# PE File Building

To write a PE image to a file or output stream. it needs to be serialized into `PEFile` first. This can be done through the `ToPEFile` method, accompanied with an instance of `IPEFileBuilder`.

```csharp
IPEFileBuilder builder = ...;
var newPEFile = peImage.ToPEFile(builder);
```

Once a `PEFile` instance has been constructed from the image, it can be written to an output stream (as described in [Writing PE files](../pefile/basics.md#writing-pe-files)):

``` csharp
using var stream = File.Create(@"C:\mynewfile.exe");
newPEFile.Write(new BinaryStreamWriter(stream));
```

Depending on the type of PE image and use-case, you will need a different type of PE builder.
In the remainder of this article, the different types of built-in PE file builders are discussed, as well how you can create your own PE file builder.


## Built-in PE File Builders

Currently, AsmResolver provides two default implementations:

| Builder                  | Main Purpose                                                                      |
|--------------------------|-----------------------------------------------------------------------------------|
| `ManagedPEFileBuilder`   | Construct new fully managed .NET PE files from scratch.                           |
| `UnmanagedPEFileBuilder` | Construct PE files based on an existing unmanaged or mixed-mode PE file template. |


In the remainder of this section, the different features and limitations of the PE file builders are explained.


### Managed (.NET) PE Files

The easiest way to PE files targeting the .NET platform is to use the `ManagedPEFileBuilder`.

```csharp
var builder = new ManagedPEFileBuilder();
```

No further configuration is required.
The builder figures out everything on its own.


#### When to Use

When building .NET PE files, it is recommended to use `ManagedPEFileBuilder` when possible, as it aims to produce files that closely resemble files produced by a typical C# compiler, and also optimizes for file size.

In short, use the `ManagedPEFileBuilder` when you are dealing with the following situation:
- Your input binary is a typical .NET binary targeting .NET Framework, .NET Core or .NET 5+.
- Your input binary only has methods implemented in CIL or single contiguous blocks of (native) code.
- You do not care about raw offsets of headers and data directories.
- You want a file optimized for size.


#### Final PE File Layout

The `ManagedPEFileBuilder` creates files with at most four PE sections, depending on what is present in the image:
- `.text`: 
  Almost everything is put into this section, including method bodies and data directories containing .NET metadata, imports and debug data.
- `.rsrc`:
  Win32 resources are put into this section, and this section will only be added if there is at least one entry in the root resource directory of the PE image.
- `.sdata`:
  This section will be added when unmanaged PE exports and/or .NET VTable fixups are present.
- `.reloc`:
  Base relocations are put in this section, and they are only added if at least
  one base relocation was put into the directory, or when the CLR bootstrapper stub requires one.


> [!NOTE]
> This builder might add or remove the entry to `mscoree.dll!_CorExeMain` or `mscoree.dll!_CorDllMain` to the imports and base relocations directory, depending on the platform architecture the PE targets.

> [!WARNING] 
> As files are always constructed from scratch, offsets and sizes of various segments and data directories may change when rebuilding an existing file.

> [!WARNING]
> Files will only include what is absolutely necessary according to its headers. 
  Any code or data segment not explicitly defined in a header or for which their size could not be determined is stripped from the binary.


### Unmanaged or Mixed Mode PE Files

To build fully native/unmanaged PE files or mixed-mode PE files containing both managed and unmanaged code, it is possible to use the `UnmanagedPEFileBuilder` class:

```csharp
var builder = new UnmanagedPEFileBuilder();
```

#### When to use

This builder bases its final PE layout on a template PE file, and aims to preserve as much of the original structure of this file.

In short, use the `UnmanagedPEFileBuilder` when you are dealing with the following situation:
- The input binary is fully unmanaged or contains unmanaged code or sections that needs to be preserved.
- You care about raw offsets of headers and data directories.
- You do not mind a file that may get larger in size.


#### Specifying a Base File

By default, the `UnmanagedPEFileBuilder` uses `PEImage::PEFile` as base template, which is set for images read from the disk or input stream.
To override this behavior, it is possible to manually set the template PE file.

```csharp
builder.BaseFile = PEFile.FromFile(@"C:\path\to\file.exe");
```


#### Rebuilding Import and VTable Fixup Directories

Many unmanaged or mixed-mode binaries that use an Import Address Table (IAT) or .NET's VTable Fixups directory, reference individual entries within these tables by hardcoded virtual addresses.
Therefore, when modifying these tables, the original entries in these tables need to be trampolined such that code referencing the original tables remains functioning.

By default, for performance and file-size reasons, the `UnmanagedPEFileBuilder` does not rebuild the IAT directory.
AsmResolver can be instructed to rebuild and trampoline these tables by setting the appropriate flag:

```csharp
builder.TrampolineImports = true;
```

Similarly, the original VTable fixups also need to be trampolined if a new VTable Fixup directory is to be constructed:

```csharp
builder.TrampolineVTableFixups = true;
```

Trampolining is efficient at runtime, but only works if the imported symbol is a reference to an external function.
If the symbol is instead referencing a global data field (such as `std::cout` in a C++ binary), its corresponding IAT slot needs to be dynamically adjusted at runtime.

By default, AsmResolver assumes every imported symbol is a function reference.
To customize this behavior, the `ImportedSymbolClassifier` can be set to a custom implementation of `IImportedSymbolClassifier`:

```csharp
// Classify `std::cout` as a data ref, and everything else as a function ref.
builder.ImportedSymbolClassifier = new DelegatedSymbolClassifier(x => x.Name switch
{
    "?cout@std@@3V?$basic_ostream@DU?$char_traits@D@std@@@1@A" => ImportedSymbolType.Data,
    _ => ImportedSymbolType.Function
});
```


#### Final PE File Layout

The `UnmanagedPEFileBuilder` will clone all original sections of the template PE file, and try to byte-patch the changes that were made.
If a particular segment or data directory does not longer fit in its original place, the builder will add auxiliary PE sections and put the segments in there. 

At most five auxiliary sections will be added:

- `.auxtext`:
  This section may contain new code segments, reconstructed import lookup and address tables, .NET metadata, debug data, as well as any code required to initialize or trampoline these tables.
- `.rdata`:
  This section may contain a reconstructed TLS data directory when required.
- `.auxdata`:
  This may contain a reconstructed exports tables as well as VTable fixup tables.
  It may also contain the new TLS index variable if required.
- `.auxrsrc`:
  This section may contain the new Win32 resource data directory.
- `.reloc`:
  This section may contain the reconstructed base relocation tables.


> [!NOTE]
> When the input binary is a .NET file, this builder might add or remove the entry to `mscoree.dll!_CorExeMain` or `mscoree.dll!_CorDllMain` to the imports and base relocations directory, depending on the platform architecture the PE targets.

> [!NOTE]
> When trampolining IAT entries (specifically for data field imports), a TLS callback may be added to the PE file to dynamically initialize the original IAT.

> [!WARNING] 
> If an auxiliary section is added, the original section is not removed.
> This means a file produced by this builder may contain multiple sections with the same name, and thus may also significantly increase the total file size.


## Creating your own PE Builder

For more specific use-cases where more control on the final PE file layout is required, it is possible to create a custom PE file builder, by extending from the `PEFileBuilder` class:

```csharp
public class MyPEFileBuilder : PEFileBuilder
{
    protected override IEnumerable<PESection> CreateSections(PEFileBuilderContext context)
    {
        /* ... Create and return all sections for the final PE file ... */
    }

    protected override void AssignDataDirectories(PEFileBuilderContext context, PEFile outputFile)
    {
        /* ... Assign data directories in the optional header of `outputFile` ... */
    }

    protected override uint GetEntryPointAddress(PEFileBuilderContext context, PEFile outputFile)
    {
        /* ... Determine and return the new address of the entry point ... */
    }
}
```

The `PEFileBuilder` implements a skeleton for a pipeline involving three steps:
- Constructing the new data directories.
- Composing the final sections of the new file.
- Updating the PE file headers to reflect the changes.

In the remainder of this section, we will discuss these in more details.


### Populating Data Directory Buffers

The first step in building a PE file is to reconstruct all data directories.
By default, the `PEFileBuilder` base class automatically populates buffers for the following data directories based on the contents of the input PE image:
- Exports
- Imports Lookup and Address Tables
- Debug Data
- Win32 Resources
- Base Relocations

These buffers can be found in the `PEFileBuilderContext` that is accompanied with the current build of the PE image.

If the construction of these data directories is to be customized, the appropriate methods can be overridden. 
Below is an example of adding an additional entry to the imports directory of a PE:

```csharp
protected override void CreateImportDirectory(PEFileBuilderContext context)
{
    base.CreateImportDirectory(context);

    // Add an extra module to the import tables:
    var kernel32 = new ImportedModule("KERNEL32.dll");
    var virtualProtect = new ImportedSymbol(0, "VirtualProtect");
    kernel32.Symbols.Add(virtualProtect);

    context.ImportDirectory.AddModule(kernel32);
}
```

If other data directories are supposed to be reconstructed (such as a .NET data directory), it is also possible to extend the general `CreateDataDirectoryBuffers` method:

```csharp
protected override void CreateDataDirectoryBuffers(PEFileBuilderContext context)
{
    base.CreateDataDirectoryBuffers(context);

    /* ... Populate other directories here ... */
}
```

### Creating Sections

Creating the final sections is supposed to be done in the `CreateSections` method.
Individual sections can be composed by concatenating individual segments and data directory buffers into one single `SegmentBuilder`. 

Below is an example of constructing a `.text` section with an export directory followed by some additional data.

```csharp
protected override IEnumerable<PESection> CreateSections(PEFileBuilderContext context)
{
    var result = new List<PESection>();
    
    result.Add(CreateTextSection(context));

    /* ... Create remainder of the sections ... */

    return result;
}

private PESection CreateTextSection(PEFileBuilderContext context)
{
    var textBuilder = new SegmentBuilder();

    // Add the exports data directory buffer to the section contents.
    contents.Add(context.ExportDirectory);
    
    // Add some data to the section.
    contents.Add(new DataSegment(new byte[] {1, 2, 3, 4}))

    /* ... */

    return new PESection(
        ".text",
        SectionFlags.ContentCode | SectionFlags.MemoryExecute | SectionFlags.MemoryRead,
        contents
    );
}
```

See [PE Sections](../pefile/sections.md) and [Reading and Writing File Segments](../core/segments.md) for more details on how to create and compose section contents.


### Assigning New Data Directories

When all sections have been created, the optional header of the final PE file needs to be updated such that it references all the right data that was added to the sections.
This happens in the `AssignDataDirectories` method:

```csharp
protected override void AssignDataDirectories(PEFileBuilderContext context, PEFile outputFile)
{
    var header = outputFile.OptionalHeader;

    // Update the RVA and size of the export directory.
    header.SetDataDirectory(DataDirectoryIndex.ExportDirectory, context.ExportDirectory);

    /* ... Update other directories ... */
}
```