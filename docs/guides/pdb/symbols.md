# Symbols

Symbol records define the top-level metadata of a binary, such as public
functions, global fields, compiler information, and user-defined type
definitions. They are stored in either the global symbols stream, or in
individual module streams.

In the following, we will discuss various methods for accessing symbols
defined in a PDB file.

## Global Symbols

The `PdbImage` class defines a `Symbols` property, exposing all globally
defined symbols in the PDB:

``` csharp
PdbImage image = ...;

foreach (var symbol in image.Symbols)
{
    Console.WriteLine(symbol);
}
```

You can use LINQ\'s collection extensions to obtain symbols of a
specific type. For example, the following iterates over all global
public symbols:

``` csharp
PdbImage image = ...;

foreach (var symbol in image.Symbols.Where(s => s.CodeViewSymbolType == CodeViewSymbolType.Pub32))
{
    Console.WriteLine(symbol);
}
```

Most types of symbol are represented by their own dedicated subclass of
`CodeViewSymbol` in AsmResolver. To get a more strongly typed filtering,
use the `OfType<T>()` extension to also automatically cast to the
specific symbol type. The following iterates over all global public
symbols, and automatically casts them to the appropriate `PublicSymbol`
type:

``` csharp
PdbImage image = ...;

foreach (var symbol in image.Symbols.OfType<PublicSymbol>())
{
    Console.WriteLine("Name: {0}, Section: {1}, Offset: {2:X8}.",
        symbol.Name,
        symbol.SegmentIndex,
        symbol.Offset);
}
```

> [!NOTE]
> Since this part of the API is a work-in-process, any symbol that is
> currently not supported or recognized by AsmResolver\'s parser will be
> represented by an instance of the `UnknownSymbol` class, exposing the
> raw data of the symbol.


## Module Symbols

PDB images may define symbols that are only valid within the scope of a
single module (typically single compilands and object files). These can
be accessed via the `PdbImage::Modules` property, and each module
defines its own `Symbols` property:

``` csharp
PdbImage image = ...;

foreach (var module in image.Modules)
{
    Console.WriteLine(module.Name);
    foreach (var symbol in module.Symbols)
        Console.WriteLine($"\t- {symbol}");
    Console.WriteLine();
}
```

## Local Symbols

Records such as the `ProcedureSymbol` class may contain multiple
sub-symbols. These type of symbols all implement `ICodeViewScopeSymbol`,
and define their own set of `Symbols`. This way, the symbol tree can be
traversed recursively.

Below is an example snippet of obtaining all local variable symbols
defined within the `DllMain` function of a library:

``` csharp
PdbImage image = ...;

var module = image.Modules.First(m => m.Name == @"c:\simpledll\release\dllmain.obj");
var procedure = module.Symbols.OfType<ProcedureSymbol>().First(p => p.Name == "DllMain");

foreach (var local in image.Symbols.OfType<LocalSymbol>())
{
    Console.WriteLine("Name: {0}, Type: {1}", 
        local.Name,
        local.VariableType);
}
```

> [!NOTE]
> In the PDB file format, symbols that define a scope (such as `S_LPROC32`
> records) end their scope with a special `S_END` symbol record in the
> file. However, AsmResolver does **not** include these ending records in
> the list of symbols. Ending records are automatically interpreted and
> inserted when appropriate during the writing process by AsmResolver, and
> should thus not be expected in the list, nor added to the list manually.
