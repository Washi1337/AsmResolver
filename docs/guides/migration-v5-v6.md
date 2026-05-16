# Migrating from v5.x to v6.0

Most models in AsmResolver v5.x have stayed the same in v6.0. However there are a few breaking changes that likely will require attention when upgrading.

## Removal of redundant namespaces

To reduce the number of `using` directives in a typical use-case of AsmResolver, v6.0 flattens many namespaces into a single namespace.
This includes the following list:

- `AsmResolver.PE.File`
- `AsmResolver.PE.Debug` (except `.Builder`)
- `AsmResolver.PE.Exceptions`
- `AsmResolver.PE.DotNet.Metadata` (except `.Tables`)
- `AsmResolver.PE.DotNet.Metadata.Tables`
- `AsmResolver.DotNet.Builder.Metadata`
- `AsmResolver.DotNet.Signatures` (except `.Parsing`)


## Removal of IPEFile, IPEImage and other interfaces

Many redundant interfaces were removed in AsmResolver v6.0 because they only ended up having a single implementation.
Users are expected to simply use the default implementation class:

Code in v5.x:
```csharp
IPEFile file = PEFile.FromFile(...);
IPEImage image = PEImage.FromFile(...);
IExportDirectory exports = image.Exports;
IImportedModule import = image.Imports[0];
IResourceDirectory resources = image.Resources;
IDotNetDirectory dotnet = image.DotNetDirectory;
```

Code in v6.0:
```csharp
PEFile file = PEFile.FromFile(...);
PEImage image = PEImage.FromFile(...);
ExportDirectory exports = image.Exports;
ImportedModule import = image.Imports[0];
ResourceDirectory resources = image.Resources;
DotNetDirectory dotnet = image.DotNetDirectory;
```

## PESection.Name is Utf8String

The `Name` property on `PESection` is now of type `Utf8String` instead of `string`.
In most cases, the implicit operator will handle type-coercion with `System.String` accordingly, but in some cases you may need to unwrap the underlying string explicitly:

Code in v5.x:
```csharp
PEFile file = PEFile.FromFile(...);
string name = file.Sections[0].Name;
```

Code in v6.0:
```csharp
PEFile file = PEFile.FromFile(...);
string name = file.Sections[0].Name.Value;
```


## EmptyErrorListener is used by default for reading

AsmResolver's PE parser is by default instantiated with an `EmptyErrorListener` in v6.0 as opposed to `ThrowErrorListener` in v5.x.
This may have impact on how you load PE files and how errors are handled:

Code in v5.x:
```csharp
var image = PEImage.FromFile(...);
```

Code in v6.0:
```csharp
var image = PEImage.FromFile(..., new PEReaderParameters(ThrowErrorListener.Instance));
```

Conversely, the following v6.0 code...

```csharp
var image = PEImage.FromFile(...);
```

... is equivalent to the following in v5.x:
```csharp
var image = PEImage.FromFile(..., new PEReaderParameters(EmptyErrorListener.Instance));
```


## IExceptionDirectory::GetEntries renamed to GetFunctions

This function was renamed to `GetFunctions` to better reflect the intended meaning.

Code in v5.x:
```csharp
var image = PEImage.FromFile(...);
foreach (var function in image.Exports.GetEntries()) 
{
    ...
}
```

Code in v6.0:
```csharp
var image = PEImage.FromFile(...);
foreach (var function in image.Exports.GetFunctions()) 
{
    ...
}
```


## Changes to IMemberDescriptor::Resolve()

Metadata resolution now requires a `RuntimeContext`, which manages assembly and metadata caches:

Code in v5.x:
```csharp
TypeReference reference = ...;
TypeDefinition definition = reference.Resolve();
```

Code in v6.0:
```csharp
RuntimeContext context = ...;
TypeReference reference = ...;
TypeDefinition definition = reference.Resolve(context);
```

A runtime context can be obtained from an existing module:

```csharp
ModuleDefinition module = ...
var context = module.RuntimeContext;
```

They can also be created manually (e.g., to force certain environment parameters such as the assumed .NET runtime version):
```csharp
var context = new RuntimeContext(DotNetRuntimeInfo.NetCoreApp(3, 1));
```

See also [Runtime Contexts](dotnet/runtime-contexts.md) for more details.


## Implementing IAssemblyResolver

AsmResolver v6.0 drastically simplified the definition of `IAssemblyResolver`:

Interface in v5.x:
```csharp
public interface IAssemblyResolver
{
    AssemblyDefinition? Resolve(AssemblyDescriptor assembly);
    void AddToCache(AssemblyDescriptor descriptor, AssemblyDefinition definition);
    bool RemoveFromCache(AssemblyDescriptor descriptor);
    bool HasCached(AssemblyDescriptor descriptor);
    void ClearCache();
}
```

Interface in v6.0:
```csharp
public interface IAssemblyResolver
{
    ResolutionStatus Resolve(AssemblyDescriptor assembly, ModuleDefinition? originModule, out AssemblyDfinition? result);
}
```

It is expected that when `Resolve` returns `ResolutionStatus.Success`, `result` is non-`null` and `null` otherwise.
It is also expected that assemblies returned from this method are not added to a `RuntimeContext`.
This can be done e.g., by setting `createRuntimeContext` to `false` in `AssemblyDefinition.FromFile`:

```csharp

public ResolutionStatus Resolve(AssemblyDescriptor assembly, ModuleDefinition? originModule, out AssemblyDfinition? result)
{
    string path = /* .. probe path ... */
    result = AssemblyDefinition.FromFile(path, createRuntimeContext: false);
    return ResolutionStatus.Success;
}
```

Finally, it is also no longer expected that `IAssemblyResolver` caches previously read assemblies and always produces a new `AssemblyDefinition` instance.

See also [Runtime Contexts](dotnet/runtime-contexts.md) for more details.


## Implementing IMetadataResolver

`IMetadataResolver` was removed in v6.0 thus it is no longer possible to override logic for this.
All resolution of metadata goes through `RuntimeContext`.

See also [Runtime Contexts](dotnet/runtime-contexts.md) for more details.


## Type comparisons using SignatureComparer

AsmResolver v6.0 changes the way types are resolved.
This also affects the way `SignatureComparer` compares types that are forwarded through an `ExportedType` (`TypeForwardedTo` attribute in C#), and in particular, the use of `SignatureComparer.Default`.

In v5.x, `SignatureComparer` always attempts to resolve any potentially exported types.
As a result, comparisons between type references `[System.Runtime] System.Object` and `[System.Private.CoreLib] System.Object` returns `true` even though they have a different assembly reference defined as their resolution scope:

```csharp
// Using AsmResolver v5.x
var t1 = KnownCorLibs.SystemRuntime_v10_0_0_0.CreateTypeReference("System", "Object");
var t2 = KnownCorLibs.SystemPrivateCoreLib_v10_0_0_0.CreateTypeReference("System", "Object");

bool equal = SignatureComparer.Default.Equals(t1, t2); // returns true
```

In v6.0, `SignatureComparer` by default **does not resolve forwarded types**, and thus treat these two type references as distinct:

```csharp
// Using AsmResolver v6.0
var t1 = KnownCorLibs.SystemRuntime_v10_0_0_0.CreateTypeReference("System", "Object");
var t2 = KnownCorLibs.SystemPrivateCoreLib_v10_0_0_0.CreateTypeReference("System", "Object");

bool equal = SignatureComparer.Default.Equals(t1, t2); // returns false
```

However, if a comparer is initialized with a non-`null` instance of `RuntimeContext`, the comparer will use the original behavior again:

```csharp
// Using AsmResolver v6.0
var t1 = KnownCorLibs.SystemRuntime_v10_0_0_0.CreateTypeReference("System", "Object");
var t2 = KnownCorLibs.SystemPrivateCoreLib_v10_0_0_0.CreateTypeReference("System", "Object");

var context = new RuntimeContext(DotNetRuntimeInfo.NetCoreApp(10, 0));
var comparer = context.SignatureComparer; // or `var comparer = new SignatureComparer(context)`;

bool equal = comparer.Equals(t1, t2); // returns true
```

Note that `SignatureComparer.Default` is not initialized with a `RuntimeContext`.
Users that rely on the equality of potentially forwarded types will therefore have to migrate to a manually initialized `SignatureComparer` instead of using `SignatureComparer.Default`.


## Creating .NET method bodies

In AsmResolver v5.x, when instantiating a new body for a method definition, it was required to specify the owner method definition in its constructor.
With v6.0 this is no longer necessary.

Code in v5.x:
```csharp
MethodDefinition method = ...;
method.CilMethodBody = new CilMethodBody(owner: method);
```

Code in v6.0:
```csharp
MethodDefinition method = ...;
method.CilMethodBody = new CilMethodBody();
```

## Creating Member References

AsmResolver v6.0 removes many of the `params`-keyed parameters in many metadata reference factory methods such as `MethodSignature::CreateInstance` and `MethodSignature::CreateStatic`.
This was done to better differentiate between parameters and return types.
Users are expected to use C# collection expressions or other types of collection syntax instead.

Code in v5.x:
```csharp
// void System.Console::WriteLine(string, object, object)
var method = factory.CorLibScope
    .CreateTypeReference("System", "Console")
    .CreateMemberReference("WriteLine", MethodSignature.CreateStatic(
        factory.Void, 
        factory.String, factory.Object, factory.Object
    ))
```

Code in v6.0:
```csharp
// void System.Console::WriteLine(string, object, object)
var method = factory.CorLibScope
    .CreateTypeReference("System", "Console")
    .CreateMemberReference("WriteLine", MethodSignature.CreateStatic(
        factory.Void, 
        [factory.String, factory.Object, factory.Object]
    ))    
```


## Auto Importing of References

In AsmResolver v5.x, referencing metadata defined in external assemblies/references required making calls to a `ReferenceImporter` (e.g., via `ModuleDefinition.DefaultImporter`), or else a `MemberNotImportedException` is thrown.
With v6.0, most importing is done automatically at build-time.


Code in v5.x:
```csharp
var method = module.CorLibFactory.CorLibScope
    .CreateTypeReference("System", "Console")
    .CreateMemberReference("WriteLine", MethodSignature.CreateStatic(factory.Void, [factory.String]))
    .ImportWith(module.DefaultImporter); // Explicitly import the reference.
```

Code in v6.0:
```csharp
var method = module.CorLibFactory.CorLibScope
    .CreateTypeReference("System", "Console")
    .CreateMemberReference("WriteLine", MethodSignature.CreateStatic(factory.Void, [factory.String]));
```

> [!NOTE]
> It is not necessarily incorrect to keep calling `ImportWith` in v6.0.
> In most cases however, this is redundant and may unnecessarily allocate copies of the same metadata object.

> [!NOTE]
> Importing is still required if you rely on custom overridden behavior of `ReferenceImporter` (e.g., with `MemberCloner`).

See also [Metadata References](dotnet/metadata-references.md).


## IMemberDescriptor::Module is removed

AsmResolver v6.0 splits `IMemberDescriptor::Module` into two properties `ContextModule` and `DeclaringModule` for references and definitions respectively.
This was done to avoid confusing cases where e.g., `TypeReference::Module` would be misinterpreted as the declaring module as opposed to the referencing module.
All member _definition_ classes now expose a `DeclaringModule`, returning the module the definition was declared in.
Additionally, all member _reference_ classes define a `ContextModule` representing the module the reference was present in.

Code in v5.x:
```csharp
TypeDefinition definition = ...;
var declaringModule = definition.Module;
```
```csharp
TypeReference reference = ...;
var referencingModule = reference.Module;
```

Code in v6.0:
```csharp
TypeDefinition definition = ...;
var declaringModule = definition.DeclaringModule;
```
```csharp
TypeReference reference = ...;
var referencingModule = reference.ContextModule;
```
