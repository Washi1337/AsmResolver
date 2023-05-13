# Reading and Writing File Segments

Segments are the basis of everything in AsmResolver. They are the
fundamental building blocks that together make up a binary file (such as
a PE file). Segments are organized as a tree, where the leaves are
single contiguous chunk of memory, while the nodes are segments that
comprise multiple smaller sub-segments. The aim of segments is to
abstract away the complicated mess that comes with calculating offsets,
sizes and updating them accordingly, allowing programmers to easily read
binary files, as well as construct new ones.

Every class that directly translates to a concrete segment in a file on
the disk implements the `ISegment` interface. In the following, some of
the basics of `ISegment` as well as common examples will be introduced.

## Basic Data Segments

The simplest and arguably the most commonly used form of segment is the
`DataSegment` class. This is a class that wraps around a `byte[]` into
an instance of `ISegment`, allowing it to be used in any context where a
segment are expected in AsmResolver.

``` csharp
byte[] data = new byte[] { 1, 2, 3, 4 };
var segment = new DataSegment(data);
```

While the name of the `DataSegment` class implies it is used for
defining literal data (such as a constant for a variable), it can be
used to define *any* type of contiguous memory. This also includes a raw
code stream of a function body and sometimes entire program sections.

## Reading Segment Contents

Some implementations of `ISegment` (such as `DataSegment`) allow for
reading binary data directly. Segments that allow for this implement
`IReadableSegment`, which defines a function `CreateReader` that can be
used to create an instance of `BinaryStreamReader` that starts at the
beginning of the raw contents of the segment. This reader can then be
used to read the contents of the segment.

``` csharp
byte[] data = new byte[] { 1, 2, 3, 4 };
IReadableSegment segment = new DataSegment(data);

var reader = segment.CreateReader();
reader.ReadByte(); // returns 1
reader.ReadByte(); // returns 2
reader.ReadByte(); // returns 3
reader.ReadByte(); // returns 4
reader.ReadByte(); // throws EndOfStreamException.
```

Alternatively, a `IReadableSegment` can be turned into a `byte[]`
quickly using the `ToArray()` method.

``` csharp
byte[] data = new byte[] { 1, 2, 3, 4 };
IReadableSegment segment = new DataSegment(data);

byte[] allData = segment.ToArray(); // Returns { 1, 2, 3, 4 }
```

## Composing new Segments

Many segments comprise multiple smaller sub-segments. For example, PE
sections often do not contain just a single data structure, but are a
collection of structures concatenated together. To facilitate more
complicated structures like these, the `SegmentBuilder` class can be
used to combine `ISegment` instances into one effortlessly:

``` csharp
var builder = new SegmentBuilder();

builder.Add(new DataSegment(...));
builder.Add(new DataSegment(...));
```

Many segments in an executable file format require segments to be
aligned to a certain byte-boundary. The `SegmentBuilder::Add` method
allows for specifying this alignment, and automatically adjust the
offsets and sizes accordingly:

``` csharp
var builder = new SegmentBuilder();

// Add some segment with potentially a size that is not a multiple of 4 bytes.
builder.Add(new DataSegment(...));

// Ensure the next segment is aligned to a 4-byte boundary in the final file.
builder.Add(new DataSegment(...), alignment: 4);
```

Since `SegmentBuilder` implements `ISegment` itself, it can also be used
within another `SegmentBuilder`, allowing for recursive constructions
like the following:

``` csharp
var child = new SegmentBuilder();
child.Add(new DataSegment(...));
child.Add(new DataSegment(...));

var root = new SegmentBuilder();
root.Add(new DataSegment(...));
root.Add(child); // Nest segment builders into each other.
```

## Resizing Segments at Runtime

Most segments in an executable file retain their size at runtime.
However, some segments (such as a `.bss` section in a PE file) may be
resized upon mapping it into memory. AsmResolver represents these
segments using the `VirtualSegment` class:

``` csharp
var physicalContents = new DataSegment(new byte[] {1, 2, 3, 4});
section.Contents = new VirtualSegment(physicalContents, 0x1000); // Create a new segment with a virtual size of 0x1000 bytes.
```

## Patching Segments

Some use-cases of AsmResolver require segments to be hot-patched with
new data after serialization. This is done via the `PatchedSegment`
class.

Any segment can be wrapped into a `PatchedSegment` via its constructor:

``` csharp
using AsmResolver.Patching;

ISegment segment = ...
var patchedSegment = new PatchedSegment(segment);
```

Alternatively, you can use (the preferred) fluent syntax:

``` csharp
using AsmResolver.Patching;

ISegment segment = ...
var patchedSegment = segment.AsPatchedSegment();
```

Applying the patches can then be done by repeatedly calling one of the
`Patch` method overloads. Below is an example of patching a section
within a PE file:

``` csharp
var peFile = PEFile.FromFile("input.exe");
var section = peFile.Sections.First(s => s.Name == ".text");

var someSymbol = peImage
   .Imports.First(m => m.Name == "ucrtbased.dll")
   .Symbols.First(s => s.Name == "puts");

section.Contents = section.Contents.AsPatchedSegment()                      // Create patched segment.
   .Patch(offset: 0x10, data: new byte[] {1, 2, 3, 4})                      // Apply literal bytes patch
   .Patch(offset: 0x20, AddressFixupType.Absolute64BitAddress, someSymbol); // Apply address fixup patch.
```

The patching API can be extended by implementing the `IPatch` yourself.

## Calculating Offsets and Sizes

Typically, the `ISegment` API aims to abstract away any raw offset,
relative virtual address (RVA), and/or size of a data structure within a
binary file. However, in case the final offset and/or size of a segment
still need to be determined and used (e.g., when implementing new
segments), it is important to understand how this is done.

Two properties are responsible for representing the offsets:

-   `Offset`: The starting file or memory address of the segment.
-   `Rva`: The virtual address of the segment, relative to the
    executable\'s image base at runtime.

Typically, these properties are read-only and managed by AsmResolver
itself. However, to update the offsets and RVAs of a segment, you can
call the `UpdateOffsets` method. This method traverses the entire
segment recursively, and updates the offsets accordingly.

``` csharp
ISegment segment = ...

// Relocate a segment to an offsets-rva pair:
segment.UpdateOffsets(new RelocationParameters(offset: 0x200, rva: 0x2000);

Console.WriteLine($"Offset: 0x{segment.Offset:X8}"); // Prints 0x200
Console.WriteLine($"Rva: 0x{segment.Rva:X8}");       // Prints 0x2000
```

> [!WARNING]
> Try to call `UpdateOffsets()` as sparsely as possible. The method does a
> full pass on the entire segment, and updates all offsets of all
> sub-segments as well. It can thus be very inefficient to call them
> repeatedly.

The size (in bytes) of a segment can be calculated using either the
`GetPhysicalSize()` or `GetVirtualSize()`. Typically, these two
measurements are going to be equal, but for some segments (such as a
`VirtualSegment`) this may differ:

``` csharp
ISegment segment = ...

// Measure the size of the segment:
uint physicalSize = segment.GetPhysicalSize();
uint virtualSize = segment.GetVirtualSize();

Console.WriteLine($"Physical (File) Size: 0x{physicalSize:X8}");
Console.WriteLine($"Virtual (Runtime) Size: 0x{virtualSize:X8}");
```

> [!WARNING]
> Only call `GetPhysicalSize()` and `GetVirtualSize()` whenever you know
> the offsets of the segment are up to date. Due to padding requirements,
> many segments will have a slightly different size depending on the final
> file offset they are placed at.

> [!WARNING]
> Try to call `GetPhysicalSize()` and `GetVirtualSize()` as sparsely as
> possible. These methods do a full pass on the entire segment, and
> measure the total amount of bytes required to represent it. It can thus
> be very inefficient to call them repeatedly.


## Serializing Segments

Segments are serialized using the `ISegment::Write` method.

``` csharp
ISegment segment = ...

using var stream = new MemoryStream();
segment.Write(new BinaryStreamWriter(stream));

byte[] serializedData = stream.ToArray();
```

Alternatively, you can quickly serialize a segment to a `byte[]` using
the `WriteIntoArray()` extension method:

``` csharp
ISegment segment = ...

byte[] serializedData = stream.WriteIntoArray();
```

> [!WARNING]
> Only call `Write` whenever you know the offsets of the segment are up to
> date. Many segments will contain offsets to other segments in the file,
> which may not be accurate until all offsets are calculated.
