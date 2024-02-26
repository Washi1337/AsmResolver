# Reference Importing

.NET modules use entries in the TypeRef or MemberRef tables to reference
types or members from external assemblies. Importing references into the
current module, therefore, form a key role when creating new- or
modifying existing .NET modules. When a member is not imported into the
current module, a `MemberNotImportedException` will be thrown when you
are trying to create a PE image or write the module to the disk.

AsmResolver provides the `ReferenceImporter` class that does most of the
heavy lifting. Obtaining an instance of `ReferenceImporter` can be done
in two ways.

Either instantiate one yourself:

``` csharp
ModuleDefinition module = ...
var importer = new ReferenceImporter(module);
```

Or obtain the default instance that comes with every `ModuleDefinition`
object. This avoids allocating new reference importers every time.

``` csharp
ModuleDefinition module = ...
var importer = module.DefaultImporter;
```

The example snippets that will follow in this article assume that there
is such a `ReferenceImporter` object instantiated using either of these
two methods, and is stored in an `importer` variable.

## Importing existing members

Metadata members from external modules can be imported using the
`ReferenceImporter` class using one of the following members:

|Member type to import |Method to use   | Result type          |
|----------------------|----------------| ---------------------|
|`IResolutionScope`    |`ImportScope`   | `IResolutionScope`   |
|`AssemblyReference`   |`ImportScope`   | `IResolutionScope`   |
|`AssemblyDefinition`  |`ImportScope`   | `IResolutionScope`   |
|`ModuleReference`     |`ImportScope`   | `IResolutionScope`   |
|`ITypeDefOrRef`       |`ImportType`    | `ITypeDefOrRef`      |
|`TypeDefinition`      |`ImportType`    | `ITypeDefOrRef`      |
|`TypeReference`       |`ImportType`    | `ITypeDefOrRef`      |
|`TypeSpecification`   |`ImportType`    | `ITypeDefOrRef`      |
|`IMethodDefOrRef`     |`ImportMethod`  | `IMethodDefOrRef`    |
|`MethodDefinition`    |`ImportMethod`  | `IMethodDefOrRef`    |
|`MethodSpecification` |`ImportMethod`  | `IMethodDefOrRef`    |
|`IFieldDescriptor`    |`ImportField`   | `IFieldDescriptor`   |
|`FieldDefinition`     |`ImportField`   | `IFieldDescriptor`   |

Below an example of how to import a type definition called `SomeType`:

``` csharp
ModuleDefinition externalModule = ModuleDefinition.FromFile(...);
TypeDefinition typeToImport = externalModule.TopLevelTypes.First(t => t.Name == "SomeType");

ITypeDefOrRef importedType = importer.ImportType(typeToImport);
```

These types also implement the `IImportable` interface. This means you
can also use the `member.ImportWith` method instead:

``` csharp
ModuleDefinition externalModule = ModuleDefinition.FromFile(...);
TypeDefinition typeToImport = externalModule.TopLevelTypes.First(t => t.Name == "SomeType");

ITypeDefOrRef importedType = typeToImport.ImportWith(importer);
```

## Importing existing type signatures

Type signatures can also be imported using the `ReferenceImporter`
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

## Creating new references

Member references can also be created and imported without having direct
access to its member definition or `System.Reflection` instance. It is
possible to create new instances of `TypeReference` and
`MemberReference` using the constructors, but the preferred way is to
use the factory methods that allow for a more fluent syntax. Below is an
example of how to create a fully imported reference to
`void System.Console.WriteLine(string)`:

``` csharp
var factory = module.CorLibTypeFactory;
var importedMethod = factory.CorLibScope
    .CreateTypeReference("System", "Console")
    .CreateMemberReference("WriteLine", MethodSignature.CreateStatic(
        factory.Void, factory.String))
    .ImportWith(importer);

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
        new GenericParameterSignature(GenericParameterType.Type, 0)))
    .ImportWith(importer);

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
    .MakeGenericInstanceMethod(factory.String)
    .ImportWith(importer);

// importedMethod now references "!0[] System.Array.Empty<System.String>()"
```

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

// `reference` will target `[mscorlib] System.DateTime` when running on .NET Framework, and `[System.Runtime] System.DateTime` when running on .NET Core.
```

Therefore, always make sure you are importing from a .NET module that is
compatible with the target .NET module.
