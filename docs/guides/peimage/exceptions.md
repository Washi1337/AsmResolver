# Exceptions Directory

Structured Exception Handling (SEH) in native programming languages such
as C++ are for some platforms implemented using exception handler
tables. These are tables that store ranges of addresses that are
protected by an exception handler. In the PE file format, these tables
are stored in the Exceptions Data Directory.

The relevant classes for this article are stored in the following
namespace:

``` csharp
using AsmResolver.PE.Exceptions;
```

## Runtime Functions

The `PEImage` class exposes these tables through the `Exceptions`
property. This is of type `IExceptionsDirectory`, which allows for read
access to instances of `IRuntimeFunction` through the `GetEntries`
method. This interface models all the runtime functions through the
method, in a platform agnostic manner.

``` csharp
foreach (var function in peImage.Exceptions.GetEntries())
{
    Console.WriteLine($"Begin: {function.Begin.Rva:X8}.");
    Console.WriteLine($"End:   {function.End.Rva:X8}.");
}
```

Different platforms use different physical formats for their runtime
functions. To figure out what kind of format an image is using, check
the `Machine` field in the file header of the PE file.

> [!NOTE]
> Currently AsmResolver only supports reading exception tables of PE files
> targeting the AMD64 (x86 64-bit) architecture.

## AMD64 / x64

AsmResolver provides the `X64ExceptionsDirectory` and
`X64RuntimeFunction` classes that implement the exceptions directory for
the AMD64 (x86 64-bit) architecture. Next to just the start and end
address, this implementation also provides access to the unwind info.

``` csharp
var directory = (X64ExceptionsDirectory) peImage.Exceptions;

foreach (var function in directory.Entries)
{
    Console.WriteLine($"Begin: {function.Begin.Rva:X8}.");
    Console.WriteLine($"End:   {function.End.Rva:X8}.");

    var unwindInfo = function.UnwindInfo;

    // Get handler start.
    Console.WriteLine($"Handler Start: {unwindInfo.ExceptionHandler.Rva:X8}.");

    // Read custom SEH data associated to this unwind information.
    var dataReader = function.ExceptionHandlerData.CreateReader();
    // ...
}
```
