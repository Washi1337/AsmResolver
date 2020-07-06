AsmResolver Coding Style and Licensing
======================================

If you make any changes to AsmResolver, you are agreeing to the license conditions as specified in [LICENSE.md](LICENSE.md).

Aims
----

This guide is for developers who wish to contribute to the AsmResolver codebase. It will detail how to properly style and format code to fit this project.

Following this guide and formatting your code as detailed will likely get your pull request merged much faster than if you don't (assuming the written code has no mistakes in itself).

General
-------

The general idea behind the coding style that is adopted in the AsmResolver project follows a few key points:

- Do not to be afraid to be verbose.
- Separate different parts of the code from each other wherever possible.

Furthermore:
- Use 4 spaces for indentation.
- Try to limit the number of characters on one line to 120 characters.
- Avoid preprocessor directives or `#region`s

C# Coding Style
---------------

### General naming conventions

Below an overview of the naming conventions used within the project.

| Member type                 | Naming style                        |
|-----------------------------|-------------------------------------|
| Namespaces                  | `PascalCase`                        |
| Classes                     | `PascalCase`                        |
| Structs                     | `PascalCase`                        |
| Interfaces                  | `IPrefixedPascalCase`               |
| Private instance fields     | `_camelCaseWithUnderscore`          | 
| Any other field             | `PascalCase`                        |
| Methods                     | `PascalCase`                        |
| Properties                  | `PascalCase`                        |
| Events                      | `PascalCase`                        |
| Parameters                  | `camelCase`                         |
| Local variables             | `camelCase`                         |
| Local constants             | `camelCase`                         |


### Prefer verbose names over abbreviations

Avoid truncating any words within the name of a member:

Do:
```csharp
public class ModuleDefinition
```

Don't:
```csharp
public class ModuleDef
```

In the case an abbreviation is still used, only capitalize all letters of the abbreviation if the abbreviation is at most two letters. Otherwise, only capitalize the first letter in the abbreviation.

Do:
```csharp
public class PEImage { ... }
public struct CilOpCode { ... }
```

Don't:
```csharp
public class PeImage { ... }
public struct CILOpCode { ... }
```

In the case of an interface name starting with an abbreviation, we do not count the prefix `I` when counting the letters of an abbreviation. 

Do:
```
public interface IPEImage { ... }
```

Don't:
```
public interface IPeImage { ... }
public interface IpeImage { ... }
```

### Grouping of members

The general order of members within a single file is as followed:

1. Events
2. Fields
3. Constructors
4. Properties
5. Methods
6. Nested types

Prefer to mark the current `class` as `partial` over using `#region` directives when the file gets too large.

### General brace style

Always place opening and closing braces on a new line, and indent after.

Do:
```csharp
public namespace N
{
    public class T
    {
        public void Method()
        {
            Console.WriteLine("Hello, world!");
        }
    }
}
```

Don't:
```csharp
public namespace N {
    public class T {
        public void Method() {
            Console.WriteLine("Hello, world!");
        }
    }
}
```

### Embedded statement bracing

Arms of an `if` statement should always be wrapped using braces. The only place when it is acceptable to exclude braces is when __all__ arms only consist of one line of code. In such a case, the embedded statement should be on a new line, indented.

Do:
```csharp
if (x == y)
    MethodA();

if (x == y)
    MethodA();
else
    MethodB();

if (x == y)
{
    MethodA();
}
else 
{
    MethodB();
    MethodC();
}
```

Don't:
```csharp
if (x == y) MethodA();

if (x == y)
    MethodA();
else
{
    MethodB();
    MethodC();
}
```

For loops, both including and excluding braces is acceptable, even if the embedded statement only spans one line. Similar to `if` statements, when braces are excluded, always place the embedded statement on a new indented line.

```csharp
foreach (var x in collection)
    Method(x);

foreach (var x in collection)
{
    Method(x);
}
```

### Local variable typing

Use `var` for anything that is not a primitive type for which a dedicated keyword exists.

Do:
```csharp
int x = 123;
string y = "Hello, world!";

var array = new byte[10];
var instance = new MyClass(...);
```

Don't:
```csharp
var x = 123;
var y = "Hello, world!";

byte[] array = new byte[10];
MyClass instance = new MyClass(...);
```

### Public method, field and property typing

Prefer to use the most general type of the returned object that still conveys the overal structure of the returned object, as much as possible. For instance, prefer using `IList<T>` over `List<T>` for mutable collection properties, or `IEnumerable<T>` over `List<T>` for methods that return a collection.

Do:
```csharp
public IList<MethodDefinition> Methods
{
    get;
}

public IEnumerable<TypeDefinition> GetAllTypes() 
{ 
    // ... 
}
```

Don't:
```csharp
public List<MethodDefinition> Methods
{
    get;
}

public List<TypeDefinition> GetAllTypes()
{ 
    // ... 
}
```

For non-public members, using the more specific type is acceptable.

### The `this` keywords

Only use `this` when absolutely necessary. Prefer to omit it from any expression unless there is ambiguity or when `this` needs to be used as an argument.

Do:
```csharp
int x = _myField;
SomeMethod();
```

Don't:
```csharp
int x = this._myField;
this.SomeMethod();
```

### Ternary experssions

Always place the arms of a ternary expression on separate lines:

Do:
```csharp
var temp = condition
    ? MethodA()
    : MethodB();
```

Don't:
```csharp
var temp = condition ? MethodA() : MethodB();
```

### Loops

Prefer `for` loops over `foreach` when heap allocated enumerators can be avoided. For example, if `GetEnumerator` returns a non-struct type enumerator, but accessing elements by index is possible, prefer to use a `for` loop with an indexer variable.

Do:
```csharp
IList<T> items = ...;
for (int i = 0; i < items.Count; i++)
{
    // Use items[i]
}
```

Don't:
```csharp
IList<T> items = ...;
foreach (var item in items) // IList<T>.GetEnumerator() returns a heap allocated enumerator
{
    // Use item
}
```

### Usage of LINQ

Using Linq is acceptable, but prefer the method syntax over the query syntax. When multiple method calls are chained together, prefer to put them on separate lines:

Do:
```csharp
var allClasses = assembly.Modules
    .SelectMany(m => m.GetAllTypes())
    .Where(t => t.IsClass)
    .ToArray();
```

Don't:
```csharp
var allClasses = assembly.Modules.SelectMany(m => m.GetAllTypes()).Where(t => t.IsClass).ToArray();
```

### XML documentation

Always provide at least the `<summary>` xml documentation tag for non-`private` and non-`internal` members.