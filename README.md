#AsmResolver
[![Build status](https://ci.appveyor.com/api/projects/status/32r7s2skrgm9ubva?svg=true)](https://ci.appveyor.com/project/Washi1337/asmresolver)
AsmResolver is a PE inspection library allowing .NET programmers to read, modify and write executable files. This includes .NET as well as native native images. The library exposes high-level representations of the PE, while still allowing the user to access low-level structures.


AsmResolver is released under the LGPL license.


#Quick starters guide
##Reading an assembly
Opening an assembly can be done by specifying a file path:
```csharp
var assembly = WindowsAssembly.FromFile(@"C:\yourfile.exe");
```
... or passing a byte array:
```csharp
var assembly = WindowsAssembly.FromBytes(File.ReadAllBytes(@"C:\yourfile.exe"));
```
... or using an instance of a `IBinaryStreamReader`:
```csharp
var reader = new MemoryStreamReader(File.ReadAllBytes(@"C:\yourfile.exe"));
var assembly = WindowsAssembly.FromReader(reader);
```

##Writing an assembly
Building an assembly can be done by using one of the `WindowsAssembly.Write` overloads:
```csharp
assembly.Write(@"C:\yourfile.exe");
```
```csharp
using (var stream = File.Create(@"C:\yourfile.exe"))
{
    assembly.Write(new BinaryStreamWriter(stream));
}
```
```csharp
using (var stream = File.Create(@"C:\yourfile.exe"))
{
    var parameters = new BuildingParameters(new BinaryStreamWriter(stream));
    // ...
    assembly.Write(parameters);
}
```

*Currently, AsmResolver only has an inbuilt .NET assembly writer, and therefore only .NET applications can be built at the moment. Support for native images will be added in the future.*

##Disassembling x86 code
Reading x86 code can be done using the `X86Disassembler` class. 
```csharp
var disassembler = new X86Disassembler(assembly.ReadingContext.Reader.CreateSubReader(start));

while (reader.Position < reader.Length)
{
    var instruction = disassembler.ReadNextInstruction();
    // ...
}
```

Disassembled instructions can be formatted into a readable text using an instance of an `IX86Formatter`.

```csharp
var formatter = new FasmX86Formatter();
...
Console.WriteLine(formatter.FormatInstruction(instruction));
```

##Accessing .NET streams / heaps
The `WindowsAssembly` class holds a property called `NetDirectory`, which exposes members representing .NET-specific structures and metadata.

Accessing the metadata streams can be done by using the `MetadataHeader` class:

```csharp
var header = assembly.NetDirectory.MetadataHeader;
var streams = header.GetStreams();
```

Getting a specific metadata stream can be done using the `GetStream(string)` method.
```csharp
var tableStream = (TableStream)header.GetStream("#~");
var blobStream = (BlobStream)header.GetStream("#Blob");
```
AsmResolver also provides an overload of the method `GetStream<TStream>`, which takes away the need of specifying the name and a type-cast.
```csharp
var tableStream = header.GetStream<TableStream>();
var blobStream = header.GetStream<BlobStream>();
```
Adding a stream can be done by using the StreamHeaders property:
```csharp
var header = assembly.NetDirectory.MetadataHeader;
var stream = new UserStringStream();
header.StreamHeaders.Add(new MetadataStreamHeader("#US", stream));
```

##Accessing metadata tables
The metadata tables can be aquired using the `TableStream.GetTable(MetadataTokenType)` method.

```csharp
var typesTable = (MetadataTable<TypeDefinition>)tableStream.GetTable(MetadataTokenType.Type);
var myType = typesTable[0];
```

AsmResolver also provides the generic overload `GetTable<TMember>()` to reduce the amount of code.
```csharp
var typesTable = tableStream.GetTable<TypeDefinition>();
var myType = typesTable[0];
```

The `MetadataTable` class implements the `ICollection` interface, which makes adding and removing members possible
```csharp
var typesTable = tableStream.GetTable<TypeDefinition>();
var myType = new TypeDefinition(...);
typesTable.Add(myType);
```

*Please note that even though some metadata classes expose collections of metadata members, such as the TypeDefinition class exposing the Methods property, it is still required to manually add/remove the members to/from the corresponding tables. AsmResolver does not have an inbuilt metadata builder yet, but it is expected this feature will be added in the future.*

##Editing method bodies
AsmResolver fully supports reading and editing managed method bodies using the `MethodBody` class. This class exposes various members that will aid in editing the contents of the method. Method bodies can be accessed through the `MethodDefinition.MethodBody` property.

```csharp
var typesTable = tableStream.GetTable<TypeDefinition>();
var myType = typesTable[0];
var myMethod = myType.Methods[0];
var myMethodBody = myMethod.MethodBody;
```
