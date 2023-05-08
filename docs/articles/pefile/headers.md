# PE Headers

After obtaining an instance of the `PEFile` class, it is possible to
read and edit various properties in the DOS header, COFF file header and
optional header.

All relevant code for this article is found in the following namespace:

``` csharp
using AsmResolver.PE.File.Headers;
```

## DOS Header

The DOS header (also known as the MZ header or `IMAGE_DOS_HEADER`) is
the first header in every PE file, and is represented using the
`DosHeader` class in AsmResolver. While the minimal DOS header is 64
bytes long, and often is followed by a stub of MS DOS code, only one
field is read and used by Windows while preparing the PE file for
execution. This field (`e_lfanew`) is the offset to the NT Headers
(`IMAGE_NT_HEADERS`), which contains the COFF and Optional Header.

Typically this value is set to `0x80`, but AsmResolver supports reading
and changing this offset if desired:

``` csharp
PEFile file = ...

// Obtain e_lfanew:
Console.WriteLine("e_flanew: {0:X8}", file.DosHeader.NextHeaderOffset);

// Set a new e_lfanew:
file.DosHeader.NextHeaderOffset = 0x100;
```

## File Header

The file header describes general characteristics of the PE file. In
particular, it indicates the target architecture, as well as the total
size of the optional header and number of sections stored in the PE
file.

AsmResolver exposes the file header via the `PEFile::FileHeader`
property. The properties defined in this object correspond directly with
the fields in `IMAGE_FILE_HEADER` as defined in `winnt.h`, and are both
readable and writeable:

``` csharp
PEFile file = ...
FileHeader header = file.FileHeader;

Console.WriteLine($"Machine:             {header.Machine}");
Console.WriteLine("NumberOfSections:     {header.NumberOfSections}");
Console.WriteLine("TimeDateStamp:        0x{header.TimeDateStamp:X8}");
Console.WriteLine("PointerToSymbolTable: 0x{header.PointerToSymbolTable:X8}");
Console.WriteLine("NumberOfSymbols:      {header.NumberOfSymbols}");
Console.WriteLine("SizeOfOptionalHeader: 0x{header.SizeOfOptionalHeader:X4}");
Console.WriteLine("Characteristics:      {header.Characteristics}");
```

> [!NOTE]
> While `NumberOfSections` and `SizeOfOptionalHeader` are writeable, these
> properties are automatically updated when using `PEFile::Write` to
> ensure a valid PE file to be written to the disk.

## Optional Header

The optional header directly follows the file header of a PE file, and
describes information such as the entry point, as well as file alignment
and target subsystem. It also contains the locations of important data
directories stored in the PE file containing information such as import
address tables and resources.

AsmResolver exposes the file header via the `PEFile::OptionalHeader`
property.

``` csharp
PEFile file = ...
OptionalHeader header = file.OptionalHeader;
```

### PE32 and PE32+ Format

While the PE specification defines both a 32-bit and 64-bit version of
the structure, AsmResolver abstracts away the differences using a single
`OptionalHeader` class. The final file format that is used is dictated
by the `Magic` property. Changing the file format can be done by simply
writing to this property:

``` csharp
// Read currently used file format.
Console.WriteLine($"Magic: {header.Magic}");

// Change to PE32+ (64-bit format).
header.Magic = OptionalHeaderMagic.PE32Plus;
```

> [!WARNING]
> For a valid PE file, it is important to use the right file format of the
> optional header that matches with the target architecture as specified
> in `FileHeader::Machine`. A 32-bit target architecture will always
> expect a `PE32` file format of the optional header, while a 64-bit
> architecture will require a `PE32Plus` format. AsmResolver does not
> automatically keep these two properties in sync.

### Entry Point and Data Directories

The optional header references many segments in the sections of the PE
file via the `AddressOfEntryPoint` and `DataDirectories` properties.

``` csharp
// Reading the entry point:
Console.WriteLine($"AddressOfEntryPoint: 0x{header.AddressOfEntryPoint:X8}");

// Setting a new entry point:
header.AddressOfEntryPoint = 0x12345678;
```

Iterating all data directory headers can be done using the following:

``` csharp
for (int i = 0; i < header.DataDirectories.Count; i++) 
{
    var directory = header.DataDirectories[i];
    Console.WriteLine($"[{i}]: RVA = 0x{directory.VirtualAddress:X8}, Size = 0x{directory.Size:X8}");
}
```

Getting or setting a specific data directory header can also be done by
using its symbolic index via `GetDataDirectory` and `SetDataDirectory`:

``` csharp
// Get the import directory.
var directory = header.GetDataDirectory(DataDirectoryIndex.ImportDirectory);

// Set the import directory.
header.SetDataDirectory(DataDirectoryIndex.ImportDirectory, new DataDirectory(
    virtualAddress: 0x00002000,
    size: 0x80
));
```

Reading the contents behind these pointers can be done e.g., by using
`PEFile::CreateReaderAtRva` or `PEFile::CreateDataDirectoryReader`:

``` csharp
BinaryStreamReader entryPointReader = file.CreateReaderAtRva(header.AddressOfEntryPoint);
```

``` csharp
BinaryStreamReader importsReader = file.CreateDataDirectoryReader(
    header.GetDataDirectory(DataDirectoryIndex.ImportDirectory)
);
```

These functions throw when an invalid offset or size are provided. It is
also possible to use the `TryCreateXXX` methods instead, to immediately
test for validity and only return the reader if correct information was
provided:

``` csharp
var importDirectory = header.GetDataDirectory(DataDirectoryIndex.ImportDirectory);
if (file.TryCreateDataDirectoryReader(importDirectory, out var importsReader))
{
    // Valid RVA and size. Use importReader to read the contents.
}
```

### Sub System

The `SubSystem` field in the optional header defines the type of sub
system the executable will be run in. Typical values are either
`WindowsGui` for graphical applications, and `WindowsCui` for
applications requiring a console window.

``` csharp
// Reading the target sub system:
Console.WriteLine("SubSystem: {header.SubSystem}");

// Changing the application to a GUI application:
header.SubSystem = SubSystem.WindowsGui;
```

### Section Alignments

The optional header defines two properties `FileAlignment` and
`SectionAlignment` that determine the section alignment stored on the
disk and in memory at runtime respectively.

``` csharp
Console.WriteLine("FileAlignment:    0x{header.FileAlignment}");
Console.WriteLine("SectionAlignment: 0x{header.SectionAlignment}");
```

AsmResolver respects the value in `FileAlignment` when writing a
`PEFile` object to the disk, and automatically realigns sections when
appropriate. It is also possible to force the realignment of sections to
be done immediately after assigning a new value to these properties
using the `PEFile::AlignSections` method.

``` csharp
header.FileAlignment = 0x400;
file.AlignSections();
```

See [PE Sections](sections.md) for more information on how to use sections.

### Other PE Offsets and Sizes

The optional header defines a few more properties indicating some
important offsets and sizes in the PE file:

-   `SizeOfCode`
-   `SizeOfInitializedData`
-   `SizeOfUninitializedData`
-   `BaseOfCode`
-   `BaseOfData`
-   `SizeOfImage`
-   `SizeOfHeaders`

These properties can be read and written in the same way other fields
are read and written, but are automatically updated when using
`PEFile::Write` to ensure a valid binary.
