# The Member Tree

## Assemblies and Modules

The root of every .NET assembly is represented by the
`AssemblyDefinition` class. This class exposes basic information such as
name, version and public key token, but also a collection of all modules
that are defined in the assembly. Modules are represented by the
`ModuleDefinition` class.

Below an example that enumerates all modules defined in an assembly.

``` csharp
var assembly = AssemblyDefinition.FromFile(...);
foreach (var module in assembly.Modules)
    Console.WriteLine(module.Name);
```

Most .NET assemblies only have one module. This main module is also
known as the manifest module, and can be accessed directly through the
`AssemblyDefinition.ManifestModule` property.

Executable modules can have an entry point, which can be obtained using
the `ManagedEntryPoint` property:

```csharp
var entryPoint = module.ManagedEntryPoint;
```

Often, modules also contain a static module constructor, which is executed
the moment the module is loaded into memory by the CLR (and thus before the
entry point is executed). AsmResolver provides helper methods to quickly
locate such a constructor:

```csharp
var cctor = module.GetModuleConstructor();
```

## Types

Types form logical units or data type defined in a module, and are
represented by the `TypeDefinition` class.

### Inspecting Types in a Module

Types defined in a module are exposed through the `ModuleDefinition.TopLevelTypes`
property. A top level types is any non-nested type. Nested types
are exposed through the `TypeDefinition.NestedTypes`.

Below is an example program that iterates through all types recursively
and prints them:

``` csharp
public const int IndentationWidth = 3;

private static void Main(string[] args)
{
    var module = ModuleDefinition.FromFile(...);
    DumpTypes(module.TopLevelTypes);
}

private static void DumpTypes(IEnumerable<TypeDefinition> types, int indentationLevel = 0)
{
    string indentation = new string(' ', indentationLevel * IndentationWidth);
    foreach (var type in types)
    {
        // Print the name of the current type.
        Console.WriteLine($"{indentation}- {type.Name} : {type.MetadataToken}");

        // Dump any nested types.
        DumpTypes(type.NestedTypes, indentationLevel + 1);
    }
}
```

Alternatively, you can get all the types including nested types using the
`ModuleDefinition.GetAllTypes()` method:

``` csharp
var module = ModuleDefinition.FromFile(...);
foreach (var type in module.GetAllTypes())
    Console.WriteLine(type.FullName);
```

### Creating New Types

New types can be created by calling one of its constructors:

```csharp
ModuleDefinition module = ...
var newType = new TypeDefinition(
    "Namespace",
    "Name",
    TypeAttributes.Public,
    module.CorLibTypeFactory.Object);
```

> [!WARNING]
> For classes, ensure that you specify a non-null base type or the CLR will
> not load the binary properly.

For structures, make sure that your type inherits from `System.ValueType`:

```csharp
ModuleDefinition module = ...
var newType = new TypeDefinition(
    "Namespace",
    "Name",
    TypeAttributes.Public,
    module.CorLibTypeFactory.CorLibScope
        .CreateTypeReference("System", "ValueType")
        .ImportWith(module.DefaultImporter));
```

Interfaces in a .NET module do not have a base type, and as such, creating
new interfaces will not require specifying one:

```csharp
ModuleDefinition module = ...
var newType = new TypeDefinition(
    "Namespace",
    "IName",
    TypeAttributes.Public | TypeAttributes.Interface);
```

Once a type has been constructed, it can be added to either a `ModuleDefinition`
as a top-level type, or to another `TypeDefinition` as a nested type:

```csharp
ModuleDefinition module = ...;
module.TopLevelTypes.Add(newType);
```
```csharp
TypeDefinition type = ...;
type.NestedTypes.Add(newType);
```

## Fields

Fields comprise all the data a type stores, and form the internal structure
of a class or value type. They are represented using the `FieldDefinition`
class.

### Inspecting Fields in a Type

The `TypeDefinition` class exposes a collection of fields that the type
defines:

``` csharp
foreach (var field in type.Fields)
    Console.WriteLine($"{field.Name} : {field.MetadataToken}");
```

Fields have a signature which contains the field's type.

``` csharp
FieldDefinition field = ...
Console.WriteLine($"Field type: {field.Signature.FieldType}");
```

Fields can also have constants attached, exposed via the `Constant`
property:

``` csharp
FieldDefinition field = ...
if (field.Constant is { } constant)
    Console.WriteLine($"Field Constant Data: {BitConverter.ToString(constant.Value>Data)}");
```

For fields that have an RVA attached (such as fields with an initial
value set), you can access the `FieldRva` property containing the
`ISegment` value with the raw data. This is in particular useful for
inspecting fields containing the initial raw data of an array.

``` csharp
FieldDefinition field = ...
if (field.FieldRva is { } rva)
    Console.WriteLine($"Field Initial Data: {BitConverter.ToString(rva.WriteIntoArray())}");
```

Refer to [Reading and Writing File Segments](../core/segments.md) for more
information on how to use `ISegment`s.


### Creating New Fields

Creating and adding new fields can be done by using one of its constructors.

``` csharp
ModuleDefinition module = ...;
var field = new FieldDefinition(
    "MyField",
    FieldAttributes.Public,
    module.CorLibTypeFactory.Int32);"
```

Fields can be added to a type:

```csharp
TypeDefinition type = ...;
type.Fields.Add(field);
```

Most properties in `FieldDefinition` are mutable, allowing you to configure
however you want your new field to be.


## Methods

Methods are functions defined in a type, and provide a way to define
operations that can be applied to a type. They are represented using
the `MethodDefinition` class.

### Inspecting Methods in a Type

The `TypeDefinition` class exposes a collection of methods that the type
defines:

``` csharp
foreach (var method in type.Methods)
    Console.WriteLine($"{method.Name} : {method.MetadataToken}");
```

AsmResolver provides helper methods to find constructors in a type:

``` csharp
var parameterlessCtor = type.GetConstructor();
var parameterizedCtor = type.GetConstructor(module.CorLibFactory.Int32);
var cctor = type.GetStaticConstructor();
```

Methods and fields have a `Signature` property, that contain the return
and parameter types:

``` csharp
MethodDefinition method = ...
Console.WriteLine($"Return type:     {method.Signature.ReturnType}");
Console.WriteLine($"Parameter types: {string.Join(", ", method.Signature.ParameterTypes)}");
```

However, for reading parameters from a method definition, it is
preferred to use the `Parameters` property instead of the
`ParameterTypes` property stored in the signature. This is because the
`Parameters` property automatically binds the types to the parameter
definitions that are associated to these parameter types. This provides
additional information, such as the name of the parameter:

``` csharp
foreach (var parameter in method.Parameters)
    Console.WriteLine($"{parameter.Name} : {parameter.ParameterType}");
```

Methods may or may not be assigned a method body. This can be verified
using `HasMethodBody`:

```csharp
if (method.HasMethodBody)
{
    // ...
}
```


Typically, a method body implemented using the Common Intermediate
Language (CIL), the bytecode used by .NET. This method body can be
inspected using the `CilMethodBody` property:

```csharp
if (method.CilMethodBody is { } body)
{
    foreach (var instruction in body.Instructions)
        Console.WriteLine(instruction);
}
```

For more information on CIL method bodies, refer to
[CIL Method Bodies](managed-method-bodies.md).


### Creating New Methods

Creating new methods can be done either through one of its constructors,
taking a name, attributes, and a method signature.

For static methods, use the `MethodSignature.CreateStatic` to create
the signature:

``` csharp
ModuleDefinition module = ...;
var method = new MethodDefinition(
    "MyMethod",
    MethodAttributes.Public | MethodAttributes.Static,
    MethodSignature.CreateStatic(
        module.CorLibTypeFactory.Void,   // Return type
        module.CorLibTypeFactory.Int32,  // Parameter 1
        module.CorLibTypeFactory.String // Parameter 2
    ));
```

Similarly, for instance methods, use the `MethodSignature.CreateInstance`
to create the signature:

``` csharp
ModuleDefinition module = ...;
var method = new MethodDefinition(
    "MyMethod",
    MethodAttributes.Public,
    MethodSignature.CreateInstance(
        module.CorLibTypeFactory.Void,   // Return type
        module.CorLibTypeFactory.Int32,  // Parameter 1
        module.CorLibTypeFactory.String // Parameter 2
    ));
```

AsmResolver provides helper methods to create special methods such as
constructors that automatically set the right attributes and initialize
it with a default method body.

```csharp
ModuleDefinition module = ...;
var ctor = MethodDefinition.CreateConstructor(module.CorLibTypeFactory.Int32);
var cctor = MethodDefinition.CreateStaticConstructor();
```

After creating methods, they can be added to a type:

```csharp
TypeDefinition type = ...;
type.Methods.Add(method);
```

Most properties in `MethodDefinition` are mutable, allowing you to configure
however you want your new method to be.


## Properties and Events

Properties and Events add special semantics to groups of methods, and are
represented using the `PropertyDefinition` and `EventDefinition` classes
respectively.

Obtaining properties and events is similar to obtaining methods and
fields; `TypeDefinition` exposes them in a list as well:

``` csharp
foreach (var property in type.Properties)
    Console.WriteLine($"{property.Name} : {property.MetadataToken}");
```

``` csharp
foreach (var @event in type.Events)
    Console.WriteLine($"{@event.Name} : {@event.MetadataToken}");
```

Properties and events have methods associated to them. These are
accessible through the `Semantics` property:

``` csharp
foreach (MethodSemantics semantic in property.Semantics)
    Console.WriteLine($"{semantic.Attributes} {semantic.Method.Name} : {semantic.MetadataToken}");
```

For properties, there are helpers defined to quickly access the getter
or setter methods of the property:

```csharp
MethodDefinition getter = property.GetMethod;
MethodDefinition setter = property.SetMethod;
```

Similarly, for events, there exists helpers for obtaining the add,
remove, and fire methods:

```csharp
MethodDefinition adder = property.AddMethod;
MethodDefinition remover = property.RemoveMethod;
MethodDefinition fire = property.FireMethod;
```

## Accessibility Tests

Most member definitions are marked with accessibility modifiers
(e.g., `public` or `private`). These modifiers puts limits on
which entities in an assembly can use the definition.

AsmResolver provides built-in methods to test whether a given
definition is accessible by another.

For example, given the following two classes:

```csharp
public class Class1
{
    private int _field;

    public void Method() { ... }
}

public class Class2 { ... }
```

AsmResolver can programmatically determine whether the private field
`_field` is accessible by another entity using the `IsAccessibleFromType`
and`CanAccessDefinition` methods:

```csharp
var module = ModuleDefinition.FromFile(...);

var class1 = module.TopLevelTypes.First(t => t.Name == "Class1");
var field = class1.Fields.First(f => f.Name == "_field");
var method = class1.Methods.First(m => m.Name == "Method");

var class2 = module.TopLevelTypes.First(t => t.Name == "Class2");

Console.WriteLine(field.IsAccessibleFromType(class1)); // True
Console.WriteLine(field.IsAccessibleFromType(class2)); // False
Console.WriteLine(method.CanAccessDefinition(field)); // True
Console.WriteLine(class2.CanAccessDefinition(field)); // False
```
