# Advanced Module Reading

Advanced users might need to configure AsmResolver\'s module reader. For
example, instead of letting the module reader ignore exceptions upon
reading invalid data, errors could be collected or thrown. Other
uses might include changing the way the underlying PE or method bodies
are read. These kinds of settings can be configured using the
`ModuleReaderParameters` class.

``` csharp
var parameters = new ModuleReaderParameters();
```

These parameters can then be passed on to any of the
`ModuleDefinition.FromXXX` methods.

``` csharp
var module = ModuleDefinition.FromFile(@"C:\Path\To\File.exe", parameters);
```

## PE image reading parameters

.NET modules are stored in a normal PE file. To customize the way
AsmResolver reads the underlying PE image before it is being interpreted
as a .NET image, `ModuleReaderParameters` provides a
`PEReaderParameters` property that can be modified or replaced
completely.

``` csharp
parameters.PEReaderParameters = new PEReaderParameters
{
    ...
};
```

For example, this can be in particular useful if you want to let
AsmResolver ignore and recover from invalid data in the input file:

``` csharp
parameters.PEReaderParameters.ErrorListener = ThrowErrorListener.Instance;
```

Alternatively, this property can also be set through the constructor of
the `ModuleReaderParameters` class directly:

``` csharp
var parameters = new ModuleReaderParameters(ThrowErrorListener.Instance);
```

For more information on customizing the underlying PE image reading
process, see [Advanced PE Image Reading](../peimage/advanced-pe-reading.md).

## Changing working directory

Modules often depend on other assemblies. These assemblies often are
placed in the same directory as the original module. However, should
this not be the case, it is possible to change the path of the working
directory of the resolvers.

``` csharp
parameters.WorkingDirectory = @"C:\Path\To\Different\Folder";
```

Alternatively, this property can also be set through the constructor of
the `ModuleReaderParameters` class directly:

``` csharp
var parameters = new ModuleReaderParameters(@"C:\Path\To\Different\Folder");
```

## Custom .netmodule resolvers

For multi-module assemblies, AsmResolver looks into the path stored in
`WorkingDirectory` for files with the .netmodule extension by default.
If it is necessary to change this behaviour, it is possible to provide a
custom implementation of the `INetModuleResolver` interface.

``` csharp
public class CustomNetModuleResolver : INetModuleResolver
{
    public ModuleDefinition Resolve(string name)
    {
        // ...
    }
}
```

To let the reader use this implementation of the `INetModuleResolver`,
set the `NetModuleResolver` property of the reader parameters.

``` csharp
parameters.NetModuleResolver = new CustomNetModuleResolver();
```

## Custom method body readers

Some .NET obfuscators store the implementation of method definitions in
an encrypted form, use native method bodies, or use a custom format that
is interpreted at runtime by the means of JIT hooking. To change the way
of how method bodies are being read, it is possible to provide a custom
implementation of the `IMethodBodyReader` interface, or extend the
default implementation.

Below an example of how to add support for reading simple x86 method
bodies:

``` csharp
public class CustomMethodBodyReader : DefaultMethodBodyReader
{
    public override MethodBody ReadMethodBody(
        ModuleReaderContext context,
        MethodDefinition owner,
        in MethodDefinitionRow row)
    {
        if (owner.IsNative && row.Body.CanRead)
        {
            // Create raw binary reader if method is native.
            var reader = row.Body.CreateReader();

            // Read until the first occurrence of a ret instruction (opcode 0xC3).
            // Note: This is for demonstration purposes only, and is by no means
            // a very accurate heuristic for finding the boundaries of native
            // method bodies.

            var code = reader.ReadBytesUntil(0xC3);

            // Create native method body.
            return new NativeMethodBody(owner, code);
        }

        // Off-load to default implementation.
        return base.ReadMethodBody(context, owner, row);
    }
}
```

To let the reader use this implementation of the `IMethodBodyReader`,
set the `MethodBodyReader` property of the reader parameters.

``` csharp
parameters.MethodBodyReader = new CustomMethodBodyReader();
```

## Custom Field RVA reading

By default, the field RVA data storing the initial binary value of a
field is interpreted as raw byte blobs, and are turned into instances of
the `DataSegment` class. To adjust this behaviour, it is possible to
provide a custom implementation of the `IFieldRvaDataReader` interface.

``` csharp
public class CustomFieldRvaDataReader : FieldRvaDataReader
{
    public override ISegment ResolveFieldData(
        IErrorListener listener,
        Platform platform,
        DotNetDirectory directory,
        in FieldRvaRow fieldRvaRow)
    {
        // ...
    }
}
```

To let the reader use this implementation of the `IFieldRvaDataReader`,
set the `FieldRvaDataReader` property of the reader parameters.

``` csharp
parameters.FieldRvaDataReader = new CustomFieldRvaDataReader();
```
