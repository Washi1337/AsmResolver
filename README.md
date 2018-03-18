# AsmResolver

[![Build status](https://ci.appveyor.com/api/projects/status/32r7s2skrgm9ubva?svg=true)](https://ci.appveyor.com/project/Washi1337/asmresolver)

AsmResolver is a PE inspection library allowing .NET programmers to read, modify and write executable files. This includes .NET as well as native native images. The library exposes high-level representations of the PE, while still allowing the user to access low-level structures.


AsmResolver is released under the LGPL license.

# Quick starters guide

## Reading an assembly

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

## Writing an assembly

Building an assembly can be done by initializing an instance of a WindowsAssemblyBuilder derived class, and using one of the `WindowsAssembly.Write` overloads:

```csharp
assembly.Write("C:\yourfile.exe", builder);
```

```csharp
using (var stream = File.Create("C:\yourfile.exe"))
{
    assembly.Write(new BinaryStreamWriter(stream), builder);
}
```

AsmResolver ships with one standard .NET assembly builder, the CompactNetAssemblyBuilder, which constructs a new assembly which layout is similar to the ones produced by standard compilers. Example:

```csharp
var builder = new CompactNetAssemblyBuilder(assembly);
```

## Disassembling x86 code

Disassembled instructions can be formatted into a readable text using an instance of an `IX86Formatter`.

```csharp
var formatter = new FasmX86Formatter();
...
Console.WriteLine(formatter.FormatInstruction(instruction));
```

## Accessing .NET streams / heaps

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

## Accessing low level .NET metadata

The metadata tables can be aquired using the `TableStream.GetTable(MetadataTokenType)` method.

```csharp
var typesTable = (TypeDefinitionTable) tableStream.GetTable(MetadataTokenType.Type);
var myTypeRow = typesTable[0];
```

AsmResolver also provides the generic overload `GetTable<TTable>()` to reduce the amount of code.
```csharp
var typesTable = tableStream.GetTable<TypeDefinitionTable>();
var myTypeRow = typesTable[0];
```

The `MetadataTable` class implements the `ICollection` interface, which makes adding and removing entries possible:
```csharp
var typesTable = tableStream.GetTable<TypeDefinitionTable>();
var myTypeRow = new MetadataRow<TypeAttributes, uint, uint, uint, uint, uint>
{
    Column1 = ...,
    Column2 = ...,
    Column3 = ...,
    // ...
};
typesTable.Add(myTypeRow);
```

## Accessing high level .NET metadata

While AsmResolver provides low-level access to .NET metadata, often we are not interested in the raw representations in the form of tables and table entries. Therefore, AsmResolver provides a different view for the metadata, which resembles a more hierarchical view of the loaded .NET assembly.

In order to switch to this mode, it is required to lock the metadata using  `MetadataHeader.LockMetadata()` which returns an instance of the `MetadataImage` class.

```csharp
var image = assembly.NetDirectory.MetadataHeader.LockMetadata();
```

From there on, the declared members can be read and updated using a more familiar hierarchical approach:

```csharp
var assemblyDef = image.Assembly;
foreach (var module in assemblyDef.Modules)
{
    foreach (var type in module.TopLevelTypes)
    {
        // ...
    }
}
```

After the changes are made, it is time to unlock the metadata again to commit the changes to the metadata streams:

```csharp
var mapping = image.UnlockMetadata();
```

This will also return a mapping that maps all metadata members to their new metadata tokens. This is useful when you want to look up the created rows in the tables again.

## Editing method bodies

When in locked metadata mode, AsmResolver fully supports reading and editing managed method bodies using the `CilMethodBody` class. This class exposes various members that will aid in editing the contents of the method. Method bodies can be accessed through the `MethodDefinition.MethodBody` or `MethodDefinition.CilMethodBody` property.

```csharp
var myMethod = new MethodDefinition("MyMethod",
    MethodAttributes.Static, 
    new MethodSignature(image.TypeSystem.Int32));

var cilBody = myMethod.CilMethodBody;
cilBody.Instructions.Add(CilInstructions.Create(CilOpCodes.Ldc_I4, 1337));
cilBody.Instructions.Add(CilInstructions.Create(CilOpCodes.Ret));
// ...
```

AsmResolver also supports adding native method bodies, using the built in x86 assembler:

```csharp
var nativeMethod = new MethodDefinition("MyNativeMethod", 
    MethodAttributes.Static | MethodAttributes.PInvokeImpl, 
    new MethodSignature(image.TypeSystem.Int32));

nativeMethod.ImplAttributes = MethodImplAttributes.Native
                                | MethodImplAttributes.Unmanaged
                                | MethodImplAttributes.PreserveSig;

var nativeBody = new X86MethodBody();
nativeBody.Instructions.Add(new X86Instruction
{
    Mnemonic = X86Mnemonic.Mov,
    OpCode = X86OpCodes.Mov_Eax_Imm1632,
    Operand1 = new X86Operand(X86Register.Eax),
    Operand2 = new X86Operand(1337),
});

nativeBody.Instructions.Add(new X86Instruction
{
    Mnemonic = X86Mnemonic.Retn,
    OpCode = X86OpCodes.Retn,
});

nativeMethod.MethodBody = nativeBody;
```

Be sure to unset the ILOnly flag in the NetDirectory if your intention is to write mixed-mode applications:

```csharp
assembly.NetDirectory.Flags &= ~ImageNetDirectoryFlags.IlOnly;
```