# Dynamic Methods

Dynamic methods are methods that are constructed and assembled at run
time. They allow for dynamically generating managed code, without having
to go through the process of compiling or generating new assemblies.
This is used a lot in obfuscators that implement for example reference
proxies or virtual machines.

AsmResolver has support for reading dynamic methods and transforming
them into `MethodDefinition` objects that can be processed further. All
relevant classes are present in the following namespace:

``` csharp
using AsmResolver.DotNet.Dynamic;
```

> [!NOTE]
> Since AsmResolver 5.0, this namespace exists in a separate
> `AsmResolver.DotNet.Dynamic` nuget package.


## Reading dynamic methods

The following example demonstrates how to transform an instance of
`System.Reflection.Emit.DynamicMethod` into a `DynamicMethodDefinition`:

``` csharp
DynamicMethod dynamicMethod = ...

var contextModule = ModuleDefinition.FromFile(typeof(Program).Assembly.Location);
var definition = new DynamicMethodDefinition(contextModule, dynamicMethod);
```

Note that the constructor requires a context module. This is the module
that will be used to import or resolve any references within the method.

> [!WARNING]
> Reading dynamic methods relies on dynamic analysis, and may therefore
> result in arbitrary code execution. Make sure to only use this in a safe
> environment if the input module is not trusted.

## Using dynamic methods

An instance of `DynamicMethodDefinition` is virtually the same as any
other `MethodDefinition`, and thus all its properties can be inspected
and modified. Below an example that prints all the instructions that
were present in the body of the dynamic method:

``` csharp
DynamicMethodDefinition definition = ...
foreach (var instr in definition.CilMethodBody.Instructions)
     Console.WriteLine(instr);
```

`DynamicMethodDefinition` are fully imported method definitions. This
means we can safely add them to the context module:

``` csharp
// Read dynamic method.
var contextModule = ModuleDefinition.FromFile(typeof(Program).Assembly.Location);
var definition = new DynamicMethodDefinition(contextModule, dynamicMethod);

// Add to <Module> type.
contextModule.GetOrCreateModuleType().Methods.Add(definition);

// Save
contextModule.Write("Program.patched.dll");
```

See [Obtaining methods and fields](member-tree.md#obtaining-methods-and-fields)
and [CIL Method Bodies](managed-method-bodies.md) for more
information on how to use `MethodDefinition` objects.
