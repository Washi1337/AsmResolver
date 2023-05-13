# PE File Building

An `IPEImage` is a higher-level abstraction of an actual PE file, and
hides many of the actual implementation details such as raw section
layout and contents of data directories. While this makes reading and
interpreting a PE very easy, it makes writing a more involved process.

Unfortunately, the PE file format is not well-structured. Compilers and
linkers have a lot of freedom when it comes to the actual design and
layout of the final PE file. For example, one compiler might place the
Import Address Table (IAT) in a separate section (such as the MSVC),
while others will put it next to the code in the text section (such as
the C# compiler). This degree of freedom makes it virtually impossible
to make a completely generic PE file builder, and as such AsmResolver
does not come with one out of the box.

It is still possible to build PE files from instances of `IPEImage`, it
might just take more manual labor than you might expect at first. This
article will discuss certain strategies that can be adopted when
constructing new PE files from PE images.

## Building Sections

As discussed in [PE Sections](../pefile/sections.md), the `PESection` class
represents a single section in the PE file. This class exposes a property 
called `Contents` which is of type `ISegment`. A lot of models in AsmResolver
implement this interface, and as such, these models can directly be used
as the contents of a new section.

``` csharp
var peFile = new PEFile();

var text = new PESection(".text",
    SectionFlags.ContentCode | SectionFlags.MemoryRead | SectionFlags.MemoryExecute);
text.Content = new DataSegment(new byte[] { ... } );
peFile.Sections.Add(text);
```

In a lot of cases, however, sections do not consist of a single data
structure, but often comprise many segments concatenated together,
potentially with some padding in between. To compose new segments from a
collection of smaller sub-segments structures, the `SegmentBuilder`
class can be used. This is a class that combines a collection of
`ISegment` instances into one, allowing you to concatenate segments that
belong together easily. Below an example that builds up a `.text`
section that consists of a .NET data directory as well as some native
code. In the example, the native code is aligned to a 4-byte boundary:

``` csharp
var peFile = new PEFile();

var textBuilder = new SegmentBuilder();
textBuilder.Add(new DotNetDirectory { ... });
textBuilder.Add(new DataSegment(new byte[] { ... } ), alignment: 4);

var text = new PESection(".text",
    SectionFlags.ContentCode | SectionFlags.MemoryRead | SectionFlags.MemoryExecute);
text.Content = sectionBuilder;

peFile.Sections.Add(text);
```

PE files can also be patched using the `PatchedSegment` API. Below is an
example of patching some data in the `.text` section of a PE File with
some literal data `{1, 2, 3, 4}`, as well as writing an address to a
symbol in the imports table:

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

More detailed information on how to use segments can be found in
[Segments](../core/segments.md).

## Using complex PE models

The PE file format defines many complex data structures. While some of
these models are represented in AsmResolver using classes that derive
from `ISegment`, a lot of the classes that represent these data
structures do not implement this interface, and as such cannot be used
as a value for the `Contents` property of a `PESection` directly. This
is due to the fact that most of these models are not required to be one
single entity or chunk of continuous memory within the PE file. Instead,
they are often scattered around the PE file by a compiler. For example,
the Import Directory has a second component the Import Address Table
which is often put in a completely different PE section (usually `.text`
or `.data`) than the Import Directory itself (in `.idata` or `.rdata`).
To make reading and interpreting these data structures more convenient
for the end-user, the `AsmResolver.PE` package adopted some design
choices to abstract these details away to make things more natural to
work with. The downside of this is that writing these structures
requires you to specify where AsmResolver should place these models in
the final PE file.

In `AsmResolver.PE`, most models for which is the case reside in their
own namespace, and have their own set of classes dedicated to
constructing new segments defined in a `Builder` sub-namespace. For
example, the Win32 resources directory models reside in
`AsmResolver.PE.Win32Resources`, but the actual builder classes are put
in a sub namespace called `AsmResolver.PE.Win32Resources.Builder`.

``` csharp
IPEImage image = ...

// Construct a resources directory.
var resources = new ResourceDirectoryBuffer();
resources.AddDirectory(image.Resources);

var file = new PEFile();

// Place in a read-only section.
var rsrc = new PESection(".rsrc", SectionFlags.MemoryRead | SectionFlags.ContentInitializedData);
rsrc.Contents = resources;
file.Sections.Add(rsrc);
```

A more complicated structure such as the Imports Directory can be build
like the following:

``` csharp
IPEImage image = ...

// Construct an imports directory.
var buffer = new ImportDirectoryBuffer();
foreach (var module in image.Imports)
    buffer.AddModule(module);

var file = new PEFile();

// Place import directory in a read-only section.
var rdata = new PESection(".rdata", SectionFlags.MemoryRead | SectionFlags.ContentInitializedData);
rdata.Contents = buffer;
file.Sections.Add(rdata);

// Place the IAT in a writable section.
var data = new PESection(".data", SectionFlags.MemoryRead | SectionFlags.MemoryWrite | SectionFlags.ContentInitializedData);
data.Contents = buffer.ImportAddressDirectory;
file.Sections.Add(ddata);
```

## Using PEFileBuilders

As a lot of the PE file-building process will be similar for many types
of PE file layouts (such as the construction of the file and optional
headers), AsmResolver comes with a base class called `PEFileBuilderBase`
that abstracts many of these similarities away. Rather than defining and
building up everything yourself, the `PEFileBuilderBase` allows you to
override a couple of methods:

``` csharp
public class MyPEFileBuidler : PEFileBuilderBase<MyBuilderContext>
{
    protected override MyBuilderContext CreateContext(IPEImage image) => new();

    protected override uint GetFileAlignment(PEFile peFile, IPEImage image, MyBuilderContext context) => 0x200;

    protected override uint GetSectionAlignment(PEFile peFile, IPEImage image, MyBuilderContext context) => 0x2000;

    protected override uint GetImageBase(PEFile peFile, IPEImage image, MyBuilderContext context) => 0x00400000;

    protected override IEnumerable<PESection> CreateSections(IPEImage image, MyBuilderContext context)
    {
        /* Create sections here */
    }

    protected override IEnumerable<DataDirectory> CreateDataDirectories(PEFile peFile, IPEImage image, MyBuilderContext context)
    {
        /* Create data directories here */
    }
}

public class MyBuilderContext
{
    /* Define here additional state data to be used in your builder. */
}
```

This can then be used like the following:

``` csharp
IPEImage image = ...

var builder = new MyPEFileBuilder();
PEFile file = builder.CreateFile(image);
```
