# References to External Metadata

Next to metadata symbols defined in the current module (see [The Member Tree](./member-tree.md)), .NET modules can also reference metadata defined in external assemblies using the `AssemblyRef`, `TypeRef` and `MemberRef` tables.
Thus, when you want to use a type, field or method defined in a different assembly, it is required to turn these definitions into the appropriate references first.

Below an overview of how AsmResolver represents definitions and their corresponding reference type.

| Definition           | Reference                                  | 
|----------------------|--------------------------------------------|
| `AssemblyDefinition` | `AssemblyReference`                        |
| `TypeDefinition`     | `TypeReference`                            |
| `MethodDefinition`   | `MemberReference` with a `MethodSignature` |
| `FieldDefinition`    | `MemberReference` with a `FieldSignature`  |

> [!NOTE]
> While there also exists both a `ModuleDefinition` and `ModuleReference`, these two are unrelated. `ModuleDefinition` defines a module in a .NET assembly manifest, while `ModuleReference` is used to reference native modules such as `kernel32.dll` or `libc.so`


## Resolution scopes

The root scope of any external reference is an `IResolutionScope`.

Typically, resolution scopes are references to external assemblies, which are represented by an `AssemblyReference`.
All assemblies imported by a module are stored in `ModuleDefinition::AssemblyReferences`:

```csharp
ModuleDefinition module = ...;
foreach (var assembly in module.AssemblyReferences)
    Console.WriteLine(assembly);
```

You can add new `AssemblyReference`s to this list, but the preferred way of adding a new assembly reference to is by using an importer:

```csharp
ModuleDefinition module = ...;
var systemConsole = new AssemblyReference(
        "System.Console", 
        new Version(8, 0, 0, 0)
    ).ImportWith(module.DefaultImporter);
```

You can also import existing `AssemblyDefinition`s and turn them into `AssemblyReference`s:

```csharp
ModuleDefinition module = ...;
AssemblyDefinition otherAssembly = ...;
var otherAssemblyRef = otherAssembly.ImportWith(module.DefaultImporter);
```

`ModuleDefinition` also provides a default corlib assembly scope that the module targets:

```csharp
ModuleDefinition module = ...;
var corlib = module.CorLibTypeFactory.CorLibScope;
```

In most cases this will return an `AssemblyReference` to either `mscorlib`, `netstandard`, `System.Runtime` or `System.Private.Corlib`.
In case `module` is a corlib assembly itself, it will reference itself instead.


## Type and member references

Similar to assembly references, external types and members are represented using `TypeReference` and
`MemberReference`.

To create new references, use the fluent factory methods `CreateTypeReference` and `CreateMemberReference` on any `IResolutionScope` or `ITypeDescriptor`.
Below is an example of how to create a fully imported reference to
`void System.Console.WriteLine(string)`:

``` csharp
var method = factory.CorLibScope
    .CreateTypeReference("System", "Console")
    .CreateMemberReference("WriteLine", MethodSignature.CreateStatic(
        factory.Void, factory.String));

// importedMethod now references "void System.Console.WriteLine(string)"
```

Generic type instantiations can also be created using
`MakeGenericInstanceType`:

``` csharp
ModuleDefinition module = ...

var factory = module.CorLibTypeFactory;
var importedMethod = factory.CorLibScope
    .CreateTypeReference("System.Collections.Generic", "List`1")
    .MakeGenericInstanceType(factory.Int32)
    .ToTypeDefOrRef()
    .CreateMemberReference("Add", MethodSignature.CreateInstance(
        factory.Void,
        new GenericParameterSignature(GenericParameterType.Type, 0)));

// importedMethod now references "System.Collections.Generic.List`1<System.Int32>.Add(!0)"
```

Similarly, generic method instantiations can be constructed using
`MakeGenericInstanceMethod`:

``` csharp
ModuleDefinition module = ...

var factory = module.CorLibTypeFactory;
var importedMethod = factory.CorLibScope
    .CreateTypeReference("System", "Array")
    .CreateMemberReference("Empty", MethodSignature.CreateStatic(
        new GenericParameterSignature(GenericParameterType.Method, 0).MakeSzArrayType(), 1))
    .MakeGenericInstanceMethod(factory.String);

// importedMethod now references "!0[] System.Array.Empty<System.String>()"
```


## Importing existing metadata definitions

References to existing metadata definitions can also be automatically converted to their appropriate reference type using an importer.

Below an example of how to import a type definition called `SomeType`:

``` csharp
ModuleDefinition externalModule = ModuleDefinition.FromFile(...);
TypeDefinition typeToImport = externalModule.TopLevelTypes.First(t => t.Name == "SomeType");

ITypeDefOrRef importedType = importer.ImportType(typeToImport);
```

Most metadata definitions also implement the `IImportable` interface.
This means you can also use the `member.ImportWith` method instead:

``` csharp
ModuleDefinition externalModule = ModuleDefinition.FromFile(...);
TypeDefinition typeToImport = externalModule.TopLevelTypes.First(t => t.Name == "SomeType");

ITypeDefOrRef importedType = typeToImport.ImportWith(importer);
```


## Importing existing type signatures

Type signatures can also be imported using a reference importer.
class, but these should be imported using the `ImportTypeSignature`
method instead.

> [!NOTE]
> If a corlib type signature is imported, the appropriate type from the
> `CorLibTypeFactory` of the target module will be selected, regardless of
> whether CorLib versions are compatible with each other.


## Importing using System.Reflection

Types and members can also be imported by passing on an instance of
various `System.Reflection` classes.

|Member type to import |Method to use        |Result type          |
|----------------------|---------------------|---------------------|
|`Type`                |`ImportType`         |`ITypeDefOrRef`      |
|`Type`                |`ImportTypeSignature`|`TypeSignature`      |
|`MethodBase`          |`ImportMethod`       |`IMethodDefOrRef`    |
|`MethodInfo`          |`ImportMethod`       |`IMethodDefOrRef`    |
|`ConstructorInfo`     |`ImportMethod`       |`IMethodDefOrRef`    |
|`FieldInfo`           |`ImportScope`        |`MemberReference`    |

There is limited support for importing complex types. Types that can be
imported through reflection include:

-   Pointer types.
-   By-reference types.
-   Array types (If an array contains only one dimension, a
    `SzArrayTypeSignature` is returned. Otherwise a `ArrayTypeSignature`
    is created).
-   Generic parameters.
-   Generic type instantiations.
-   Function pointer types (.NET 8.0+ only. TFM doesn't matter for this, the runtime used at runtime is all that matters.) (!!WARNING!! )

> [!WARNING]
> Function pointer `Type` instances lose their calling conventions unless attained with
> `GetModified(Field/Property/Parameter)Type`. This includes `typeof`!
> `typeof(delegate*unmanaged[Cdecl]<void>)` is the same as `typeof(delegate*managed<void>)`.
> `Import(Field/Method)` will also strip the calling conventions from function pointers
> that are the types of fields or in method signatures.
> If you need to handle this, manually set the types in the resulting
> `IMethodDefOrRef` or `MemberReference` to the appropriate type from `ImportType`.

Instantiations of generic methods are also supported.


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
