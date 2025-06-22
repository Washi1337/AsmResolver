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

The `PEImage` class exposes the data directory through the `Exceptions`
property.
This is of type `IExceptionsDirectory`, which exposes individual `IRuntimeFunction`s through its `GetEntries` method:

``` csharp
PEImage image = ...;
foreach (var function in image.Exceptions.GetEntries())
{
    Console.WriteLine($"Begin:   {function.Begin.Rva:X8}");
    Console.WriteLine($"End:     {function.End.Rva:X8}");
    Console.WriteLine($"Handler: {function.UnwindInfo?.ExceptionHandler.Rva:X8}");
}
```

Different platforms use different physical formats for their runtime
functions.
To figure out what kind of format an image is using, check
the `Machine` field in the file header of the PE file.

```csharp
PEImage image = ...;
var machineType = image.MachineType;
```

Below a table of the currently supported architectures and their corresponding runtime function types:

| Machine Type | Runtime Function Type  |
|--------------|------------------------|
| `Amd64`      | `X64RuntimeFunction`   |
| `Arm64`      | `Arm64RuntimeFunction` |


Entries in the data directory can be casted to the appropriate specific function types.
For example, for AMD64 targets, you can cast all entries to a `X64RuntimeFunction`, and then access their platform-specific fields:

``` csharp
PEImage image = ...;
var directory = peImage.Exceptions;

if (image.MachineType == MachineType.Amd64)
{
    foreach (var function in directory.GetEntries().Cast<X64RuntimeFunction>())
    {
        X64UnwindInfo unwindInfo = function.UnwindInfo;
        // ...
    }
}
```


## AMD64 / x64 Unwind Info

For AMD64 targets, AsmResolver represents functions in the data directory using `X64RuntimeFunction`.
This class also exposes the x64-specific unwind info using the `X64UnwindInfo` class (see [msdn](https://learn.microsoft.com/en-us/cpp/build/exception-handling-x64?view=msvc-170#struct-unwind_info) for all fields):

```csharp
X64RuntimeFunction function = ...;
X64UnwindInfo unwindInfo = function.UnwindInfo;

// Get handler start.
Console.WriteLine($"Unwind Codes: {unwindInfo.UnwindCodes.Length}");

// Read custom SEH data associated to this unwind information.
var dataReader = function.ExceptionHandlerData.CreateReader();
```

## ARM64

For AMD64 targets, AsmResolver represents functions in the data directory using the `Arm64RuntimeFunction` class.
Unwind info on ARM64 can either be packed or unpacked format, which is represented by `Arm64PackedUnwindInfo` and `Arm64UnpackedUnwindInfo` respectively.

To determine whether a function has packed or unpacked unwind info, you can use e.g., pattern matching:

``` csharp
Arm64RuntimeFunction function = ...;
switch (function.UnwindInfo)
{
    case Arm64PackedUnwindInfo packedInfo:
        // ...
        break;

    case Arm64UnpackedUnwindInfo unpackedInfo:
        // ...
        break;
}
```

Packed unwind information is a bitvector that stores flags and counts suitable for many (smaller) functions (see [msdn](https://learn.microsoft.com/en-us/cpp/build/arm64-exception-handling?view=msvc-170#packed-unwind-data) for all fields).

```csharp
Arm64PackedUnwindInfo packedInfo = ...;
Console.WriteLine($"Frame Size: {packedInfo.FrameSize}");
```

Unpacked unwind information follows the `xdata` format (see [msdn](https://learn.microsoft.com/en-us/cpp/build/arm64-exception-handling?view=msvc-170#xdata-records) for all fields), and exposes properties such as unwind codes, epilog scopes and additional (custom) exception handler data introduced by the compiler:

```csharp
Arm64UnpackedUnwindInfo unpackedInfo = ...;
Console.WriteLine($"Unwind Codes: {unpackedInfo.UnwindCodes.Length}");
```