# Overview

The .NET image layer is the third layer of abstraction of the portable
executable (PE) file format. It provides a high-level abstraction of the
.NET metadata stored in a PE image, that is similar to APIs like
`System.Reflection`. Its root objects are `AssemblyDefinition` and
`ModuleDefinition`, and from there it is possible to dissect the .NET
assembly hierarchically.

In short, this means the following:

-   Assemblies define modules,
-   Modules define types, resources and external references,
-   Types define members such as methods, fields, properties and events,
-   Methods include method bodies,
-   \... and so on.

The third layer of abstraction is the highest level of abstraction for a
.NET assembly that AsmResolver provides. All objects exposed by this
layer are completely mutable and can be serialized back to a `PEImage`
from the second layer, to a `PEFile` from the first layer, or to the
disk.
