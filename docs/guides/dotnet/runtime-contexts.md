# Metadata Resolution and Runtime Contexts

A single .NET assembly is rarely fully self-contained and typically references code in external assemblies (e.g., DLLs).

AsmResolver mimics the lifetime of a .NET process using `RuntimeContext`s.
A `RuntimeContext` implements the same assembly resolution and management logic as seen at runtime, and maintains metadata caches for fast lookup and traversal of external references.
It is the spiritual static counterpart to an [AppDomain](https://learn.microsoft.com/en-us/dotnet/api/system.appdomain) or [AssemblyLoadContext](https://learn.microsoft.com/en-us/dotnet/core/dependency-loading/understanding-assemblyloadcontext).


## Creating Runtime Contexts

By default, when opening an existing assembly or module, AsmResolver automatically creates a new runtime context that is tuned to the original target runtime of the input file:

```csharp
var assembly = AssemblyDefinition.FromFile(@"C:\Path\To\File.exe");
var context = assembly.RuntimeContext; // Automatically detected and configured.
```

You can also explicitly create a new (empty) runtime context, targeting a specific runtime:

```csharp
// Create empty .NET Core 3.1 context.
var context = new RuntimeContext(DotNetRuntimeInfo.NetCoreApp(3, 1));
```
```csharp
// Create based on the original target runtime of an existing assembly.
var assembly = AssemblyDefinition.FromFile(@"C:\Path\To\File.exe", createRuntimeContext: false);
var context = new RuntimeContext(assembly.ManifestModule.OriginalTargetRuntime);
```
```csharp
// Create based on the contents of a runtime config JSON file.
var config = RuntimeConfiguration.FromFile(@"C:\Path\To\File.runtimeconfig.json");
var context = new RuntimeContext(config);
```
```csharp
// Create based on the contents of a runtime config JSON file, also specifying source directory (e.g., for self-contained apps).
var config = RuntimeConfiguration.FromFile(@"C:\Path\To\File.runtimeconfig.json");
var context = new RuntimeContext(config, sourceDirectory: @"C:\Path\To\");
```
```csharp
// Create based on the contents of a .NET PE image.
PEImage baseImage = ...
var context = new RuntimeContext(baseImage);
```
```csharp
// Create based based on the contents of a single-file bundle.
BundleManifest bundle = ...
var context = new RuntimeContext(bundle);
```


A `RuntimeContext` can also be configured with a custom assembly resolver (e.g., for cases where assemblies are embedded in a module or located at unconventional file paths):
```csharp
class MyAssemblyResolver : IAssemblyResolver
{
    public override ResolutionStatus Resolve(AssemblyDescriptor assembly, ModuleDefinition? originModule, out AssemblyDefinition? result) 
    { 
        // ...
    }
}

var context = new RuntimeContext(
    targetRuntime: DotNetRuntimeInfo.NetFramework(3, 1), 
    assemblyResolver: new MyAssemblyResolver()
);
```

> [!NOTE]
> When creating your own `IAssemblyResolver`, ensure that all resolved assemblies are not added to a `RuntimeContext` yet (e.g., by explicitly specifying `createRuntimeContext: false` in the `AssemblyDefinition.FromXXX` methods).


## Managing Assemblies

Assemblies can be loaded directly into the context:

```csharp
AssemblyDefinition assembly = context.LoadAssembly(@"C:\Path\To\File.exe");
```

When an assembly is not added to a context yet (e.g., new assemblies or manually read assemblies using `FromXXX` with `createRuntimeContext: false`), they can be added explicitly using `AddAssembly`:
```csharp
var assembly = new AssemblyDefinition("Foo", new Version(1, 0, 0, 0));
context.AddAssembly(assembly);
```
```csharp
var assembly = AssemblyDefinition.FromFile(@"C:\Foo.dll", createRuntimeContext: false);
context.AddAssembly(assembly);
```

Multiple assemblies can be loaded in the same context:

```csharp
// Load other assemblies  within the context.
var dependency1 = context.LoadAssembly(@"C:\Path\To\Dependency1.dll");
var dependency2 = context.LoadAssembly(@"C:\Path\To\Dependency2.dll");
var dependency3 = context.LoadAssembly(@"C:\Path\To\Dependency3.dll");
...
```

Loading an assembly with the same name as a previously loaded assembly will result in the same assembly definition instance:

```csharp
var assembly = context.LoadAssembly(@"C:\Path\To\Dependency1.dll");
var assembly2 = context.LoadAssembly(@"C:\Path\To\Dependency1.dll"); // returns same instance as `assembly`.
```


All currently loaded assemblies can be enumerated:
```csharp
foreach (var assembly in context.GetLoadedAssemblies())
    Console.WriteLine(assembly.FullName);
```

> [!NOTE]
> Once an `AssemblyDefinition` is added to a context, it cannot be removed.


## Metadata Resolution

References to external metadata (e.g., `AssemblyReference`, `TypeReference`, `MemberReference`) can be automatically  located and loaded into the context, and resolved to their corresponding definitions (i.e., `AssemblyDefinition`, `TypeDefinition`, `MethodDefinition`, `FieldDefinition`).

This process is facilitated by the `Resolve` and `TryResolve` methods (throwing and non-throwing respectively), defined on the metadata reference itself:
```csharp
TypeReference reference = ...;
TypeDefinition definition = reference.Resolve(context); // throws on failure.
```
```csharp
TypeReference reference = ...;
if (reference.TryResolve(context, out TypeDefinition definition))
{
    // `definition` contains the resolved type definition.
}
```

`Resolve` also has an overload that returns a status code, allows for more fine-grained error handling:
```csharp
TypeReference reference = ...;
switch (reference.Resolve(context, out var definition))
{
    case ResolutionStatus.Success:
        Console.WriteLine($"Resolved type: {definition}");
        break;
    case ResolutionStatus.AssemblyNotFound:
        Console.WriteLine("Declaring assembly not found.");
        break;
    case ResolutionStatus.TypeNotFound:
        Console.WriteLine("Type was not found in resolved assembly.");
        break;
    ...
}
```

Resolving references will result in the runtime context getting populated with the declaring assemblies, and will be directly reflected in the output of `RuntimeContext.GetLoadedAssemblies()`.
Furthermore, the runtime context will ensure the same member definition instance is returned even if the same reference is requested twice.

> [!NOTE]
> Any functionality provided by AsmResolver that may require metadata resolution requires a `RuntimeContext` to be specified.
>
> For example:
> ```csharp
> ITypeDescriptor type = ...;
> bool isValueType = type.GetIsValueType(context);
> ```
