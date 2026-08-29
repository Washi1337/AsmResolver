# Contributing to AsmResolver

This document presents guidelines on making contributions to the AsmResolver project. Following this guide as close as possible will increase the chances your pull request will get merged faster (assuming the written code has no mistakes in itself).


## License

If you make any changes to AsmResolver, you are agreeing to the license conditions as specified in [LICENSE.md](LICENSE.md).


## General Workflow

The AsmResolver project generally follows the principles of [Git Flow](https://www.atlassian.com/git/tutorials/comparing-workflows/gitflow-workflow), with a few variations. Below a summary:

- Prefer to create a branch based on `development`.
- Prefix your branch accordingly, depending on what kind of change you are trying to make.
    - For new features, use `feature/name-of-feature`.
    - For issues and/or bug fixes, use `issue/name-of-issue-or-bug`.
- Push your changes, taking into account [our code-style](#coding-style) and [AI usage policy](#ai-usage-policy).
- Open a [Pull Request](https://github.com/Washi1337/AsmResolver/pulls), setting the `development` branch as a base branch to merge into.
- Wait for your pull request to be reviewed and accepted.

**Note: Pull requests will only be accepted if all unit tests succeed and follow the guidelines as described in this document**.


## AI Usage Policy

The use of AI/LLMs is allowed for both development as well as writing the text of issue/PR itself, as long as you are upfront about it and everything is carefully vetted by a human before any PR is submitted.

**Large code submissions fully authored by AI are in most cases not a contribution but a burden.**
We are not bottlenecked by raw throughput but by maintainers willing to vet and maintain the code.

In short, when using AI/LLMs for AsmResolver development, don't be a [meat proxy](https://meatproxy.me/) and keep in mind the following guidelines:
- **Validate:** Have a human verify everything before submitting a PR.
- **Aim for code reuse:** LLMs tends to create new (duplicated) helper functions for everything unnecessarily.
- **Be concise:** Long essays over-elaborating as well as over-commenting code is a burden, not a contribution.
- **No emojis:** This is not a social media platform.


**FAILURE IN COMPLYING WITH THESE GUIDELINES WILL VERY LIKELY MEAN YOUR PR WILL GET REJECTED**.


## Coding Style

The general idea behind the coding style the AsmResolver project can be summarized in a few key points:

- Do not be afraid to be verbose.
- Separate different parts of the code from each other wherever possible.

Furthermore:
- Use 4 spaces for indentation.
- Try to limit the number of characters on one line to 120 characters.
- Avoid preprocessor directives or `#region`s

For editors that support EditorConfig, there is an `.editorconfig` file for you to use in the root directory of the repository.


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

Avoid truncating any words within the name of a member (within reason):

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
public namespace N;

public class T
{
    public void Method()
    {
        Console.WriteLine("Hello, world!");
    }
}
```

Don't:
```csharp
public namespace N;

public class T {
    public void Method() {
        Console.WriteLine("Hello, world!");
    }
}
```

### Embedded statement bracing

The general rules for embedded statements (e.g., arms of an `if` statement or a loop body) are as follows:
- An embedded statement _must_ be wrapped in braces if the statement takes more than a single line of code.
- If _any_ of the arms in an `if` statement requires multiple lines, then _all_ arms must be wrapped (including the ones that fit on one line only).
- If the statement fits on a single line, braces may be omitted. In such a case, the statement must appear on the next line, indented.
- It is always allowed to add braces, even if the statement fits on a single line.

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

Loops follow the same rules as `if` statements for omitting braces:

Do
```csharp
foreach (var x in collection)
    MethodA(x);
```

Don't:
```csharp
foreach (var x in collection) MethodA(x);
```

### Local variable typing

Use `var` for anything that is not a primitive type for which a dedicated keyword exists.

Do:
```csharp
int x = 123;
string y = "Hello, world!";
byte[] z = new byte[10];

var instance = new MyClass(...);
```

Don't:
```csharp
var x = 123;
var y = "Hello, world!";
var array = new byte[10];

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

Prefer to place the arms of a ternary expression on separate lines, unless both arms are short and are easily readable.

Do:
```csharp
var temp = condition
    ? MethodA("foo", "bar", "baz")
    : MethodB();

int y = condition ? 1 : 0;
```

Don't:
```csharp
var temp = condition ? MethodA("foo", "bar", "baz") : MethodB();

int y = condition 
    ? 1
    : 0;
```

### Loops

Prefer `for` loops over `foreach` when heap allocated enumerators can be avoided. For example, if `GetEnumerator` returns a non-struct type enumerator, but accessing elements by index is possible, prefer to use a `for` loop with an indexer variable.

Do:
```csharp
var items = assembly.Modules;
for (int i = 0; i < items.Count; i++)
{
    // Use items[i]
}
```

Don't:
```csharp
var items = assembly.Modules;
foreach (var item in items) // IList<T>.GetEnumerator() returns a heap allocated enumerator
{
    // Use item
}
```

### Method call chains and LINQ

The use of LINQ is allowed on non-performance critical paths using method syntax over query syntax.

Furthermore, prefer to place individual method calls in a method call chain on separate lines:

Do:
```csharp
var allClassMethods = assembly.Modules
    .SelectMany(m => m.GetAllTypes())
    .Where(t => t.IsClass)
    .SelectMany(t => t.Methods)
    .ToArray();
```

Don't:
```csharp
var allClasses = assembly.Modules.SelectMany(m => m.GetAllTypes()).Where(t => t.IsClass).SelectMany(t => t.Methods).ToArray();
```

### XML documentation

Always provide at least the `<summary>` xml documentation tag for non-`private` and non-`internal` members.
