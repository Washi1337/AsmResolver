# Load Configuration Directory

The Windows PE loader can be configured to load PE images with additional parameters and security mechanisms.
This is done through the load configuration data directory.

All code relevant to the loader configuration of a PE resides in the following namespace:

``` csharp
using AsmResolver.PE.Configuration;
```


## Creating new loader configurations

```csharp
var config = new LoadConfiguration();
```

As newer versions of Windows got released, more fields were added to the structure.
The size of the configuration determines which fields of the configuration are actually included.

```csharp
var config = new LoadConfiguration(
    size: 0x148,
    is32Bit: false
);
```

You can also provide the last field that should be included in the configuration and have AsmResolver figure out the appropriate size
```csharp
var config = new LoadConfiguration(
    lastIncludedField: LoadConfigurationField.UmaFunctionPointers
);
```

Once created, you can add it to a PE image:

```csharp
PEImage image = ...;
image.LoadConfiguration = config;
```


## Reading loader configurations

AsmResolver automatically detects and interprets existing load configurations and places them in `PEImage::LoadConfiguration`:

```csharp
PEImage image = ...;
LoadConfiguration config = image.LoadConfiguration;
```


## Directory size and included fields

Different platforms and different versions of Windows determine the underlying structure of the load configuration directory.

AsmResolver provides a mechanism for determining the offset of a field, based on the current PE image's architecture:

```csharp
uint fieldStartOffset = config.GetFieldOffset(LoadConfigurationField.SecurityCookie);
uint fieldEndOffset = config.GetFieldEndOffset(LoadConfigurationField.SecurityCookie);
```

AsmResolver also provides helper functions to determine whether a field is included in the directory based on its `Size`:

```csharp
if (config.HasField(LoadConfigurationField.SecurityCookie))
{
    var cookie = config.SecurityCookie;
    // ...
}
```

Existing configuration directories can be resized to ensure that it includes a specific field:

```csharp
config.Resize(lastIncludedField: LoadConfigurationField.SecurityCookie);
// `config` is now exactly resized to fit up to the security cookie field
```

```csharp
config.EnsureFieldIsIncluded(field: LoadConfigurationField.SecurityCookie);
// If `config` was too small, it is now resized to fit up to the security cookie field
```

> [!TIP]
> Ttables stored in the configuration are often represented by two fields (a pointer and a length).
> For a valid PE, make sure that both are included (i.e., by including the field with the highest offset):
> ```csharp
> config.SEHandlerTable.Add(...);
> config.EnsureFieldIsIncluded(field: LoadConfigurationField.SEHandlerCount); // also implicitly includes SEHandlerTable
> ```


> [!TIP]
> When calling `UpdateOffsets` with relocation parameters that also changes the bitness of the directory, the `Size` property will automatically be adjusted to fit the same fields as before.


## Control Flow Guard function tables

The loader configuration directory defines multiple tables referencing functions that have control flow guard enabled.
In AsmResolver, these references are represented by `ControlFlowGuardFunction`:

```csharp
ISegmentReference entryPoint = ...
config.GuardCFFunctionTable.Add(new ControlFlowGuardFunction(entryPoint));
```

Functions can have additional flags attached:

```csharp
ISegmentReference entryPoint = ...
config.GuardCFFunctionTable.Add(new ControlFlowGuardFunction(
    entryPoint, 
    ControlFlowGuardFunctionFlags.FidSuppressed
));
```

Note that in order to preserve flags in a loader configuration directory, the appropriate `GuardFlags` have to be set in the directory itself:

```csharp
config.GuardFlags |= GuardFlags.CfFunctionTableSizeHasFlags;
config.EnsureFieldIsIncluded(LoadConfigurationField.GuardFlags);
```
