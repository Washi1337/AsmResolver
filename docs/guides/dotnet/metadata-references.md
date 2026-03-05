# Member References

Next to metadata symbols defined in the current module (see [Metadata Definitions](./metadata-definitions.md)), .NET modules can also reference metadata defined in external assemblies using the `AssemblyRef`, `TypeRef` and `MemberRef` tables.

Below an overview of how AsmResolver represents definitions and their corresponding reference type.

| Definition           | Reference                                  | 
|----------------------|--------------------------------------------|
| `AssemblyDefinition` | `AssemblyReference`                        |
| `TypeDefinition`     | `TypeReference`                            |
| `MethodDefinition`   | `MemberReference` with a `MethodSignature` |
| `FieldDefinition`    | `MemberReference` with a `FieldSignature`  |

> [!NOTE]
> While there also exists both a `ModuleDefinition` and `ModuleReference`, these two are unrelated. `ModuleDefinition` defines a module in a .NET assembly manifest, while `ModuleReference` is used to reference native modules such as `kernel32.dll` or `libc.so`

> [!NOTE]
> Prior to AsmResolver 6.0, it is required to explicitly import member descriptors (e.g., `TypeDefinition`, `FieldDefinition` and `MethodDefinition`, `MemberReference` etc.) that are not defined or alraedy imported in the same assembly (e.g., using the `ImportWith` method.).
> For AsmResolver 6.0 and newer, importing is done automatically by the builder at build-time.
> You will only need to explicit importing when custom behavior is required (e.g., when cloning metadata, see [Member Cloning](./cloning.md)).


## Resolution scopes

The root scope of any external reference is an `IResolutionScope`.

Typically, resolution scopes are references to external assemblies, which are represented by an `AssemblyReference`.
All assemblies imported by a module are stored in `ModuleDefinition::AssemblyReferences`:

```csharp
ModuleDefinition module = ...;
foreach (var assembly in module.AssemblyReferences)
    Console.WriteLine(assembly);
```

Creating a new `AssemblyReference` can be done using its constructor:

```csharp
ModuleDefinition module = ...;
var systemConsole = new AssemblyReference(
    "System.Console", 
    new Version(8, 0, 0, 0)
);
```

> [!NOTE]
> Prior to AsmResolver 6.0, `AssemblyReference`s need to be manually imported using `ImportWith`. For example:
> ```csharp
> var systemConsole = new AssemblyReference(
>    "System.Console", 
>    new Version(8, 0, 0, 0)
> ).ImportWith(module.DefaultImporter); // <-- Explicitly import the reference.
> ```

An `AssemblyDefinition` can also be turned into an `AssemblyReference`:

```csharp
ModuleDefinition module = ...;
AssemblyDefinition otherAssembly = ...;
AssemblyReference otherAssemblyRef = otherAssembly.ToAssemblyReference();
```

`ModuleDefinition` also provides a default corlib assembly scope that the module targets:

```csharp
ModuleDefinition module = ...;
var corlib = module.CorLibTypeFactory.CorLibScope;
```

This will typically return an `AssemblyReference` to either `mscorlib`, `netstandard`, `System.Runtime` or `System.Private.Corlib`, depending on the target runtime the module was compiled for.
In case `module` is a corlib assembly itself, it will reference the `ModuleDefinition` itself instead.


## Type and member references

Similar to assembly references, external types and members are represented using `TypeReference` and `MemberReference`.

To create new references, use the fluent factory methods `CreateTypeReference` and `CreateMemberReference` on any `IResolutionScope` or `ITypeDescriptor`.
Below is an example of how to create a fully imported reference to `void System.Console.WriteLine(string)`:

``` csharp
var method = factory.CorLibScope
    .CreateTypeReference("System", "Console")
    .CreateMemberReference("WriteLine", MethodSignature.CreateStatic(
        returnType: factory.Void,
        parameterTypes: [factory.String]
    ));

// importedMethod now references "void System.Console.WriteLine(string)"
```

> [!NOTE]
> Prior to AsmResolver 6.0, `TypeReference`s and `MemberReference`s need to be manually imported using `ImportWith`.
> For example:
> ```csharp
> var method = factory.CorLibScope
>    .CreateTypeReference("System", "Console")
>    .CreateMemberReference("WriteLine", MethodSignature.CreateStatic(factory.Void, [factory.String]))
>    .ImportWith(module.DefaultImporter); // Explicitly import the reference.
> ```

Generic type instantiations can be created using `MakeGenericInstanceType`:

``` csharp
ModuleDefinition module = ...

var factory = module.CorLibTypeFactory;
var importedMethod = factory.CorLibScope
    .CreateTypeReference("System.Collections.Generic", "List`1")
    .MakeGenericInstanceType(factory.Int32)
    .ToTypeDefOrRef()
    .CreateMemberReference("Add", MethodSignature.CreateInstance(
        returnType: factory.Void,
        parameterTypes: [new GenericParameterSignature(GenericParameterType.Type, 0)]
    ));

// importedMethod now references "System.Collections.Generic.List`1<System.Int32>.Add(!0)"
```

Similarly, generic method instantiations can be constructed using `MakeGenericInstanceMethod`:

``` csharp
ModuleDefinition module = ...

var factory = module.CorLibTypeFactory;
var importedMethod = factory.CorLibScope
    .CreateTypeReference("System", "Array")
    .CreateMemberReference("Empty", MethodSignature.CreateStatic(
        returnType: new GenericParameterSignature(GenericParameterType.Method, 0).MakeSzArrayType(), 
        genericParameterCount: 1, 
        parameterTypes: []))
    .MakeGenericInstanceMethod(factory.String);

// importedMethod now references "!0[] System.Array.Empty<System.String>()"
```

See also [Metadata Signatures](metadata-signatures.md) for more information on type and method signatures.


## Importing existing metadata definitions

References to existing metadata definitions can also be automatically converted to their appropriate reference type using an importer.

Below an example of how to import a type definition called `SomeType`:

``` csharp
ModuleDefinition externalModule = ModuleDefinition.FromFile(...);
TypeDefinition typeToImport = externalModule.TopLevelTypes.First(t => t.Name == "SomeType");

ITypeDefOrRef importedType = module.ImportType(typeToImport);
```

Most metadata descriptors implement the `IImportable` interface, and expose a `member.ImportWith` method allowing for fluent syntax:

``` csharp
ModuleDefinition externalModule = ModuleDefinition.FromFile(...);
ITypeDefOrRef importedType = externalModule
    .TopLevelTypes.First(t => t.Name == "SomeType")
    .ImportWith(module.DefaultImporter);
```


## Importing existing type signatures

Type signatures can be imported in a similar fashion:

```csharp
TypeSignature signature = ...;
TypeSignature imported = importer.ImportTypeSignature(signature);
```

```csharp
TypeSignature signature = ...;
TypeSignature imported = signature.ImportWith(importer);
```

> [!NOTE]
> If a corlib type signature is imported, the appropriate type from the
> `CorLibTypeFactory` of the target module will be selected, regardless of
> whether CorLib versions are compatible with each other.


## Importing using System.Reflection

Types and members can also be imported by passing on an instance of various `System.Reflection` classes.

|Member type to import |Method to use        |Result type          |
|----------------------|---------------------|---------------------|
|`Type`                |`ImportType`         |`ITypeDefOrRef`      |
|`Type`                |`ImportTypeSignature`|`TypeSignature`      |
|`MethodBase`          |`ImportMethod`       |`IMethodDefOrRef`    |
|`MethodInfo`          |`ImportMethod`       |`IMethodDefOrRef`    |
|`ConstructorInfo`     |`ImportMethod`       |`IMethodDefOrRef`    |
|`FieldInfo`           |`ImportScope`        |`MemberReference`    |

There is limited support for importing complex types and members.
Types that can be imported through reflection include:

-   Pointer types.
-   By-reference types.
-   Array types (If an array contains only one dimension, a `SzArrayTypeSignature` is returned. Otherwise a `ArrayTypeSignature` is created).
-   Generic parameters.
-   Generic type instantiations.
-   Generic method instantiations.
-   Function pointer types (.NET 8.0+ only).

> [!WARNING]
> Due to quirks in the `System.Reflection` sub-system of .NET, `System.Type` instances that represent function pointer types often lose their calling conventions, unless explicitly attained with methods such as [GetModifiedFieldType](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.fieldinfo.getmodifiedfieldtype), [GetModifiedPropertyType](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.propertyinfo.getmodifiedpropertytype), or [GetModifiedParameterType](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.parameterinfo.getmodifiedparametertype?view=net-10.0#system-reflection-parameterinfo-getmodifiedparametertype). 
> This includes the use of `typeof` (e.g., `typeof(delegate*unmanaged[Cdecl]<void>)` returns the same as `typeof(delegate*managed<void>)`.
>
> References imported using `ImportField` and `ImportMethod` may therefore also strip the calling conventions from function pointers that are the types of fields or in method signatures.
> If you need to handle this, manually set the types in the resulting `IMethodDefOrRef` or `MemberReference` to the appropriate type from `ImportType`.


## Common Caveats using the Importer

### Caching and reuse of instances

The default implementation of `ReferenceImporter` does not maintain a
cache. Each call to any of the import methods will result in a new
instance of the imported member. The exception to this rule is when the
member passed onto the importer is defined in the module the importer is
targeting itself, or was already a reference imported by an importer
into the target module. In both of these cases, the same instance of
this member definition or reference will be returned instead.

### Importing cross-framework versions

The `ReferenceImporter` does not support importing across different
versions of the target framework. Members are being imported as-is, and
are not automatically adjusted to conform with other versions of a
library.

As a result, trying to import from for example a library part of the
.NET Framework into a module targeting .NET Core or vice versa has a
high chance of producing an invalid .NET binary that cannot be executed
by the runtime. For example, attempting to import a reference to
`[System.Runtime] System.DateTime` into a module targeting .NET
Framework will result in a new reference targeting a .NET Core library
(`System.Runtime`) as opposed to the appropriate .NET Framework library
(`mscorlib`).

This is a common mistake when trying to import using metadata provided
by `System.Reflection`. For example, if the host application that uses
AsmResolver targets .NET Core but the input file is targeting .NET
Framework, then you will run in the exact issue described in the above.

``` csharp
var reference = importer.ImportType(typeof(DateTime));

// `reference` will target `[mscorlib] System.DateTime` when running on .NET Framework, and `[System.Private.CoreLib] System.DateTime` when running on .NET Core.
```

Therefore, always make sure you are importing from a .NET module that is
compatible with the target .NET module.
