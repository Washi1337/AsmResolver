# PE Sections

Sections can be read and modified by accessing the `PEFile.Sections`
property, which is a collection of `PESection` objects.

``` csharp
PEFile file = ...

foreach (var section in file.Sections)
{
    Console.WriteLine(section.Name);
    Console.WriteLine($"\tFile Offset:  0x{section.Offset:X8}");
    Console.WriteLine($"\tRva:          0x{section.Rva:X8}");
    Console.WriteLine($"\tFile Size:    0x{section.Contents.GetPhysicalSize():X8}");
    Console.WriteLine($"\tVirtual Size: 0x{section.Contents.GetVirtualSize():X8}");
}
```

## Reading Section Data

Each `PESection` object has a `Contents` property defined of type
`IReadableSegment`. This object is capable of creating a
`BinaryStreamReader` instance to read and parse data from the section:

``` csharp
var reader = section.CreateReader();
```

If you want to get the entire section in a byte array, you can take the
`ToArray` shortcut:

``` csharp
byte[] data = section.ToArray();
```

Alternatively, if you have a file offset or RVA to start read from, it
is also possible use one of the `PEFile::CreateReaderAtXXX` methods:

``` csharp
var reader = file.CreateReaderAtOffset(0x200);
```

``` csharp
var reader = file.CreateReaderAtRva(0x2000);
```

These methods will automatically find the right section to read from,
and provide a reader that points to the start of this data.

## Adding a new Section

The `Sections` property is mutable, which means it is possible to add
new sections and remove others from the PE. New sections can be created
using the `PESection` constructors:

``` csharp
var section = new PESection(".asmres", SectionFlags.MemoryRead | SectionFlags.ContentInitializedData);
section.Contents = new DataSegment(new byte[] {1, 2, 3, 4});

file.Sections.Add(section);
```

Some sections (such as `.data` or `.bss`) contain uninitialized data,
and might be resized in virtual memory at runtime. As such, the virtual
size of the contents might be different than its physical size. To make
dynamically sized sections, it is possible to use the `VirtualSegment`
to decorate a normal [ISegment]{.title-ref} with a different virtual
size.

``` csharp
var section = new PESection(".asmres", SectionFlags.MemoryRead | SectionFlags.ContentUninitializedData);
var physicalContents = new DataSegment(new byte[] {1, 2, 3, 4});
section.Contents = new VirtualSegment(physicalContents, 0x1000); // Create a new segment with a virtual size of 0x1000 bytes.

file.Sections.Add(section);
```

For more advanced section building, see
[Building Sections](/guides/peimage/pe-building.html#building-sections)
[Reading and Writing File Segments](../core/segments.md).

## Updating Section Offsets

For performance reasons, offsets and sizes are not computed unless you
explicitly tell AsmResolver to align all sections and update all offsets
within a section. To force a recomputation of all section offsets and
sizes, you can use the `PEFile::AlignSections` method:

``` csharp
PESection section = ...;    
file.Sections.Add(section);

file.AlignSections();

Console.WriteLine($"New section RVA: 0x{section.Rva:X8}");
```

If you want to align the sections and also automatically update the
fields in the file and optional header of the PE file, it is also
possible to use `PEFile::UpdateHeaders` instead:

``` csharp
PESection section = ...;    
file.Sections.Add(section);

file.UpdateHeaders();

Console.WriteLine($"New section RVA: 0x{section.Rva:X8}");
Console.WriteLine($"New section count: {file.FileHeader.NumberOfSections}");
```

> [!NOTE]
> While both `AlignSections` and `UpdateHeaders` do a traversal of the
> segment tree, they may not update all offsets and sizes stored in the
> sections themselves. When reading a PE file using any of the
> `PEFile::FromXXX`, AsmResolver initializes every section\'s `Contents`
> property with a single contiguous chunk of raw memory, and does not
> parse any of the section contents. As such, if some code or data stored
> in one of these raw section references code or data in another, this
> will not be automatically updated. If, however, the `Contents` property
> is an `ISegment` that does implement `UpdateOffsets` appropriately
> (e.g., when using a `SegmentBuilder`), then all references stored in
> such a segment will be updated accordingly.