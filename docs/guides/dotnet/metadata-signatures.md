# Metadata Signatures

Type and member signatures represent references to types within a blob signature.
They are not directly associated with a metadata token, but can reference types defined in one of the metadata tables.

All relevant classes in this document can be found in the following namespaces:

``` csharp
using AsmResolver.DotNet.Signatures;
```

## Overview

Basic leaf type signatures:

| Type signature name             | Example                                                 |
|---------------------------------|---------------------------------------------------------|
| `CorLibTypeSignature`           | `int32` (`System.Int32`)                                |
| `TypeDefOrRefSignature`         | `System.IO.Stream`, `System.Drawing.Point`              |
| `GenericInstanceTypeSignature`  | ``System.Collections.Generic.IList`1<System.Int32>``    |
| `FunctionPointerTypeSignature`  | `method void *(int32, int64)`                           |
| `GenericParameterSignature`     | `!0`, `!!0`                                             |
| `SentinelTypeSignature`         | (Used as a delimeter for vararg method signatures)      | 

Decorator type signatures:

| Type signature name            | Example                                                             |
|--------------------------------|---------------------------------------------------------------------|
| `SzArrayTypeSignature`         | `System.Int32[]`                                                    |
| `ArrayTypeSignature`           | `System.Int32[0.., 0..]`                                            |
| `ByReferenceTypeSignature`     | `System.Int32&`                                                     |
| `PointerTypeSignature`         | `System.Int32*`                                                     |
| `CustomModifierTypeSignature`  | `System.Int32 modreq (System.Runtime.CompilerServices.IsVolatile)`  |
| `BoxedTypeSignature`           | (Boxes a value type signature)                                      |
| `PinnedTypeSignature`          | (Pins the value of a local variable in memory)                      |


Member signatures:

| Signature name          | Example                                                   |
|-------------------------|-----------------------------------------------------------|
| `FieldSignature`        | `System.Int32` (attached to a `FieldDefinition`)          |
| `MethodSignature`       | `instance System.Void *(System.Int32, System.Int32)`      |
| `PropertySignature`     | `System.Int32 *()` (attached to a `PropertyDefinition`)   |


## Basic element types

The CLR defines a set of primitive types (such as `System.Int32`, `System.Object`) as basic element types that can be referenced using a single byte, rather than the fully qualified name.
These are represented using the `CorLibTypeSignature` class.

Every `ModuleDefinition` defines a property called `CorLibTypeFactory`, which exposes reusable instances of all corlib type signatures:

``` csharp
ModuleDefinition module = ...
TypeSignature int32Type = module.CorLibTypeFactory.Int32;
```

Corlib type signatures can also be looked up by their element type, by their full name, or by converting a type reference to a corlib type signature.

``` csharp
int32Type = module.CorLibTypeFactory.FromElementType(ElementType.I4);
int32Type = module.CorLibTypeFactory.FromName("System", "Int32");

var int32TypeRef = corlibScope.CreateTypeReference("System", "Int32");
int32Type = module.CorLibTypeFactory.FromType(int32TypeRef);
```

If an invalid element type, name or type descriptor is passed on, `FromType` returns `null`.


## Class and struct types

Type signatures from non-primitive types (e.g., `TypeDefinition`s or `TypeReference`s) can be created by using the `ToTypeSignature` on an instance of a `ITypeDefOrRef`:

``` csharp
// Create from a definition.
TypeDefinition type = module.TopLevelTypes.First(t => t.IsTypeOf("Namespace", "TypeName"));
var typeSig = type.ToTypeSignature();
```

``` csharp
// Create from a new type reference.
var typeSig = corlibScope
    .CreateTypeReference("System.IO", "Stream")
    .ToTypeSignature(isValueType: false);
```

``` csharp
// Create from an existing type reference, dynamically resolving isValueType
RuntimeContext context = ...
TypeReference reference = ...
var typeSig = reference.ToTypeSignature(context);
```

These examples create an `TypeDefOrRefSignature` instance (corresponding to either a class or valuetype type signature).
You can also create this signature manually, by using its constructor:

``` csharp
TypeReference streamTypeRef = new TypeReference(corlibScope, "System.IO", "Stream");
TypeSignature streamTypeSig = new TypeDefOrRefSignature(streamTypeRef, isValueType: false);
```

> [!WARNING]
> While it is technically possible to reference a basic type such as `System.Int32` as a `TypeDefOrRefSignature`, it renders the .NET module  invalid by most implementations of the CLR.
> Prefer to use `ToTypeSignature`, or always use the `CorLibTypeSignature` to reference basic types within your blob signatures.


## Generic instance types

To create generic instances of types, use `MakeGenericInstanceType`:

``` csharp
var listOfString = corlibScope
    .CreateTypeReference("System.Collections.Generic", "List`1")
    .MakeGenericInstanceType(isValueType: false, typeArguments: [module.CorLibTypeFactory.String]);

// listOfString now contains a reference to List<string>.
```

This can also be created by manually instantiating a `GenericInstanceTypeSignature` class:

``` csharp
var listTypeRef = new TypeReference(corlibScope, "System.Collections.Generic", "List`1");

var listOfString = new GenericInstanceTypeSignature(
    listTypeRef, 
    isValueType: false, 
    typeArguments: [module.CorLibTypeFactory.String]
);

// listOfString now contains a reference to List<string>.
```


## Function pointer types

Function pointer signatures are strongly-typed pointer types used to describe addresses to functions or methods.
In AsmResolver, they are represented by wrapping a `MethodSignature` into a `FunctionPointerTypeSignature` using `MakeFunctionPointerType`:

``` csharp
var factory = module.CorLibTypeFactory;
var type = MethodSignature.CreateStatic(
    returnType: factory.Void,
    parameterTypes: [factory.Int32, factory.Int32]
).MakeFunctionPointerType();

// type now contains a reference to `method void *(int32, int32)`.
```

Alternatively, a `FunctionPointerTypeSignature` can be created manually as well:

``` csharp
var factory = module.CorLibTypeFactory;
var signature = MethodSignature.CreateStatic(
    returnType: factory.Void,
    parameterTypes: [factory.Int32, factory.Int32]
);

var type = new FunctionPointerTypeSignature(signature);

// type now contains a reference to `method void *(int32, int32)`.
```


## Decorating types with annotations

Type signatures can be annotated with extra properties, such as an array or pointer specifier.
This is done using the `MakeXXXType` methods.

``` csharp
var arrayTypeSig = module.CorLibTypeFactory.Int32.MakeSzArrayType();
// `arrayTypeSig` now references `System.Int32[]`
```

Below an overview of all factory methods:

| Factory method                                                  | Description                                                                                                   |
|-----------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------|
| `MakeArrayType(int dimensionCount)`                             | Wraps the type in a new `ArrayTypeSignature` with `dimensionCount` zero based dimensions with no upperbound.  |
| `MakeArrayType(ArrayDimension[] dimensions)`                    | Wraps the type in a new `ArrayTypeSignature` with `dimensions` set as dimensions                              |
| `MakeByReferenceType()`                                         | Wraps the type in a new `ByReferenceTypeSignature`                                                            |
| `MakeModifierType(ITypeDefOrRef modifierType, bool isRequired)` | Wraps the type in a new `CustomModifierTypeSignature` with the specified modifier type.                       |
| `MakePinnedType()`                                              | Wraps the type in a new `PinnedTypeSignature`                                                                 |
| `MakePointerType()`                                             | Wraps the type in a new `PointerTypeSignature`                                                                |
| `MakeSzArrayType()`                                             | Wraps the type in a new `SzArrayTypeSignature`                                                                |
| `MakeGenericInstanceType(TypeSignature[] typeArguments)`        | Wraps the type in a new `GenericInstanceTypeSignature` with the provided type arguments.   

Decorations can also be created manually, by manually calling the constructor for each type signature:

``` csharp
var arrayTypeSig = new SzArrayTypeSignature(module.CorLibTypeFactory.Int32);
```

## Traversing Signatures

Traversing type signature annotations can be done by accessing the `BaseType` property of `TypeSignature`.

``` csharp
var arrayElementType = arrayTypeSig.BaseType; // returns System.Int32
```

This gets the immediate base type as a `TypeSignature`.

Alternatively, all `TypeSignature`s available in AsmResolver also implement the [visitor pattern](https://en.wikipedia.org/wiki/Visitor_pattern), allowing for strongly-typed traversals of a type signature.
This is typically very useful when traversing or extracting specific details from deep/complex type signatures:

```csharp
public class MyVisitor : ITypeSignatureVisitor<TResult>
{
    public TResult VisitArrayType(ArrayTypeSignature signature)
    {
        /* ... handle boxed types ... */
        return signature.AcceptVisitor(this);
    }

    public TResult VisitBoxedType(BoxedTypeSignature signature)
    {        
        /* ... handle boxed types ... */
        return signature.AcceptVisitor(this);
    }

    /* ... */
}

TypeSignature signature = ...;
var result = signature.AcceptVisitor(new MyVisitor());
```

## Creating member signatures

`TypeSignature`s form the building blocks for more complex signatures.

This includes field signatures, used in `FieldDefinition` and `MemberReference`:

```csharp
// System.Int32 Foo;
var field = new FieldDefinition(
    name: "Foo",
    attributes: FieldAttributes.Public,
    signature: new FieldSignature(module.CorLibTypeFactory.Int32)
);
```

```csharp
// reference to `System.String::Empty`
var reference = module.CorLibTypeFactory.CorLibScope
    .CreateTypeReference("System", "String")
    .CreateMemberReference(
        name: "Empty",
        signature: new FieldSignature(module.CorLibTypeFactory.String)
    );
```

Method signatures can be created using the factory methods `MethodSiganture.CreateStatic` or `MethodSignature.CreateInstance`, and are used in both `MethodDefinition` and `MemberReference`:

```csharp
// static System.Void Foo(System.Int32)
var method = new MethodDefinition(
    name: "Foo", 
    attributes: MethodAttributes.Static,
    signature: MethodSignature.CreateStatic(
        returnType: module.CorLibTypeFactory.Void,
        parameterTypes: [module.CorLibTypeFactory.Int32]
    )
);
```

```csharp
// instance System.String System.Int32::ToString()
var method = module.CorLibTypeFactory.CorLibScope
    .CreateTypeReference("System", "Int32")
    .CreateMemberReference(
        name: "ToString",
        signature: MethodSignature.CreateInstance(module.CorLibTypeFactory.String)
    );
```

```csharp
// static !0 System.Linq.Enumerable::Empty<?>()
var method = module.CorLibTypeFactory.CorLibScope
    .CreateTypeReference("System.Linq", "Enumerable")
    .CreateMemberReference(
        name: "Empty",
        signature: MethodSignature.CreateInstance(
            returnType: new GenericParameterSignature(GenericParameterType.Method, 0), 
            genericParameterCount: 1,
            parameterTypes: []
        )
    );
```

> [!WARNING]
> Ensure that you use the right method signature factory method for the right method.
> Creating an instance signature for a static method or vice versa will cause the CLR to reject the final binary.


## Comparing signatures

Signatures can be tested for semantic equivalence using the `SignatureComparer` class.
Most use-cases of this class will not require any customization.
In these cases, the default `SignatureComparer` can be used:

``` csharp
var comparer = SignatureComparer.Default;
```

Note that, by default, `SignatureComparer` does not take into account forwarded types.
Therefore, it treats types such as `[System.Runtime] System.Object` and `[System.Private.CoreLib] System.Object` as two distinct types (even though at runtime they are the same).
To mimic comparisons that the runtime performs (including following forwarded types and strong name handling), use the signature comparer as stored in the `RuntimeContext` you are operating in:

``` csharp
RuntimeContext context = ...;
var comparer = context.SignatureComparer;
```

If you wish to configure the comparer yourself (e.g., for relaxing some of the declaring assembly version comparison rules), it is possible to create a new instance instead:

``` csharp
var comparer = new SignatureComparer(SignatureComparisonFlags.AllowNewerVersions);
```

Once a comparer is obtained, metadata signature equality can be tested using any of the overloaded `Equals` methods:

``` csharp
TypeSignature type1 = ...;
TypeSignature type2 = ...;

if (comparer.Equals(type1, type2)) 
{
    // type1 and type2 are equivalent.
}
```

The `SignatureComparer` class implements various instances of the `IEqualityComparer<T>` interface, and as such, it can be used as a comparer for dictionaries and related types:

``` csharp
var dictionary = new Dictionary<TypeSignature, TValue>(comparer);
```

> [!TIP]
> The `SignatureComparer` class also implements equality comparers for other kinds of metadata, such as field and method descriptors and their  signatures.

Since .NET facilitates an object oriented environment, many types will inherit or derive from each other.
In some cases where inheritance needs to be taken into account, exact equivalence comparisons may therefore be too strict.

Section I.8.7 of the ECMA-335 specification defines a set of rules that dictate whether values of a certain type are compatible with or assignable to variables of another type.
These rules are implemented in AsmResolver using the `IsCompatibleWith` and `IsAssignableTo` methods:

``` csharp
RuntimeContext? context = ...;
if (type1.IsCompatibleWith(type2, context)) 
{
    // type1 can be converted to type2.
}
```

``` csharp
RuntimeContext? context = ...;
if (type1.IsAssignableTo(type2, context)) 
{
    // Values of type1 can be assigned to variables of type2.
}
```

By default, this uses the `SigantureComparer` of the `RuntimeContext`, but these methods come with overloads that take in a custom `SignatureComparer` as well:

``` csharp
RuntimeContext? context = ...;
SignatureComparer comparer = ...;
if (type1.IsCompatibleWith(type2, context, comparer)) 
{
    // type1 can be converted to type2.
}
```

``` csharp
RuntimeContext? context = ...;
SignatureComparer comparer = ...;
if (type1.IsAssignableTo(type2, context, comparer)) 
{
    // Values of type1 can be assigned to variables of type2.
}
```
