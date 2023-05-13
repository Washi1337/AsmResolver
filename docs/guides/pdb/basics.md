# Basic I/O

Every PDB image interaction is done through classes defined by the
`AsmResolver.Symbols.Pdb` namespace:

``` csharp
using AsmResolver.Symbols.Pdb;
```

## Creating a new PDB Image

Creating a new image can be done by instantiating a `PdbImage` class:

``` csharp
var image = new PdbImage();
```

## Opening a PDB Image

Opening a PDB Image can be done through one of the `FromXXX` methods
from the `PdbImage` class:

``` csharp
byte[] raw = ...
var image = PdbImage.FromBytes(raw);
```

``` csharp
var image = PdbImage.FromFile(@"C:\myfile.pdb");
```

``` csharp
MsfFile msfFile = ...
var image = PdbImage.FromFile(msfFile);
```

``` csharp
BinaryStreamReader reader = ...
var image = PdbImage.FromReader(reader);
```

If you want to read large files (+100MB), consider using memory mapped
I/O instead:

``` csharp
using var service = new MemoryMappedFileService();
var image = PdbImage.FromFile(service.OpenFile(@"C:\myfile.pdb"));
```

## Writing a PDB Image

Writing PDB images directly is currently not supported yet, however
there are plans to making this format fully serializable.

## Creating a new MSF File

Multi-Stream Format (MSF) files are files that form the backbone
structure of all PDB images. AsmResolver fully supports this lower level
type of access to MSF files using the `MsfFile` class.

To create a new MSF file, use one of its constructors:

``` csharp
var msfFile = new MsfFile();
```

``` csharp
var msfFile = new MsfFile(blockSize: 4096);
```

## Opening an MSF File

Opening existing MSF files can be done in a very similar fashion as
reading a PDB Image:

``` csharp
byte[] raw = ...
var msfFile = MsfFile.FromBytes(raw);
```

``` csharp
var msfFile = MsfFile.FromFile(@"C:\myfile.pdb");
```

``` csharp
BinaryStreamReader reader = ...
var msfFile = MsfFile.FromReader(reader);
```

Similar to reading PDB images, if you want to read large files (+100MB),
consider using memory mapped I/O instead:

``` csharp
using var service = new MemoryMappedFileService();
var msfFile = MsfFile.FromFile(service.OpenFile(@"C:\myfile.pdb"));
```

## Writing an MSF File

Writing an MSF file can be done through one of the `Write` method
overloads.

``` csharp
msfFile.Write(@"C:\myfile.patched.pdb");
```
