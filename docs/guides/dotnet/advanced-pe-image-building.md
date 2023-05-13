# Advanced PE Image Building

The easiest way to write a .NET module to the disk is by using the
`Write` method:

``` csharp
module.Write(@"C:\Path\To\Output\Binary.exe");
```

This method is essentially a shortcut for invoking the
`ManagedPEImageBuilder` and `ManagedPEFileBuilder` classes, and will
completely reconstruct the PE image, serialize it into a PE file and
write the PE file to the disk.

While this is easy, and would probably work for most .NET module
processing, it does not provide much flexibility. To get more control
over the construction of the new PE image, it is therefore not
recommended to use a different overload of the `Write` method that takes
instances of `IPEImageBuilder` instead:

``` csharp
var imageBuilder = new ManagedPEImageBuilder();

/* Configuration of imageBuilder here... */

module.Write(@"C:\Path\To\Output\Binary.exe", imageBuilder);
```

Alternatively, it is possible to call `ModuleDefinition::ToPEImage` to
turn the module into a `PEImage` first, that can then later be
post-processed and transformed into a `PEFile` to write it to the disk:

``` csharp
var imageBuilder = new ManagedPEImageBuilder();

/* Configuration of imageBuilder here... */

// Construct image.
var image = module.ToPEImage(imageBuilder);

// Write image to the disk.
var fileBuilder = new ManagedPEFileBuilder();
var file = fileBuilder.CreateFile(image);
file.Write(@"C:\Path\To\Output\Binary.exe");
```

To get even more control, it is possible to call the `CreateImage`
method from the image builder directly. This allows for inspecting all
build artifacts, as well as post-processing of the constructed PE image
before it is written to the disk.

``` csharp
var imageBuilder = new ManagedPEImageBuilder();

/* Configuration of imageBuilder here... */

// Construct image.
var result = imageBuilder.CreateImage(module);

/* Inspect build result ... */

// Obtain constructed PE image.
var image = result.ConstructedImage;

/* Post processing of image happens here... */

// Write image to the disk.
var fileBuilder = new ManagedPEFileBuilder();
var file = fileBuilder.CreateFile(image);
file.Write(@"C:\Path\To\Output\Binary.exe");
```

This article explores various features about the `ManagedPEImageBuilder`
class.

## Token mappings

Upon constructing a new PE image for a module, members defined in the
module might be re-ordered. This can make post-processing of the PE
image difficult, as metadata members cannot be looked up by their
original metadata token anymore. The `PEImageBuildResult` object
returned by `CreateImage` defines a property called `TokenMapping`. This
object maps all members that were included in the construction of the PE
image to the newly assigned metadata tokens, allowing for new metadata
rows to be looked up easily and efficiently.

``` csharp
var mainMethod = module.ManagedEntrypointMethod;

// Build PE image.
var result = imageBuilder.CreateImage(module);

// Look up the new metadata row assigned to the main method.
var newToken = result.TokenMapping[mainMethod];
var mainMethodRow = result.ConstructedImage.DotNetDirectory.Metadata
    .GetStream<TablesStream>()
    .GetTable<MethodDefinitionRow>()
    .GetByRid(newToken.Rid);
```

## Preserving raw metadata structure

Some .NET modules are carefully crafted and rely on the raw structure of
all metadata streams. These kinds of modules often rely on one of the
following:

-   RIDs of rows within a metadata table.
-   Indices of blobs within the `#Blob`, `#Strings`, `#US` or `#GUID`
    heaps.
-   Unknown or unconventional metadata streams and their order.

The default PE image builder for .NET modules (`ManagedPEImageBuilder`)
defines a property called `DotNetDirectoryFactory`, which contains the
object responsible for constructing the .NET data directory, can be
configured to preserve as much of this structure as possible. With the
help of the `MetadataBuilderFlags` enum, it is possible to indicate
which structures of the metadata directory need to preserved. The
following table provides an overview of all preservation metadata
builder flags that can be used and combined:

| Flag                        | Description                                                     |
|-----------------------------|-----------------------------------------------------------------|
|`PreserveXXXIndices`         | Preserves all row indices of the original `XXX` metadata table. |
|`PreserveTableIndices`       | Preserves all row indices from all original metadata tables.    |
|`PreserveBlobIndices`        | Preserves all blob indices in the `#Blob` stream.               |
|`PreserveGuidIndices`        | Preserves all GUID indices in the `#GUID` stream.               |
|`PreserveStringIndices`      | Preserves all string indices in the `#Strings` stream.          |
|`PreserveUserStringIndices`  | Preserves all user-string indices in the `#US` stream.          |
|`PreserveUnknownStreams`     | Preserves any of the unknown / unconventional metadata streams. |
|`PreserveStreamOrder`        | Preserves the original order of all metadata streams.           |
|`PreserveAll`                | Preserves as much of the original metadata as possible.         |

Below is an example on how to configure the image builder to preserve
blob data and all metadata tokens to type references:

``` csharp
var factory = new DotNetDirectoryFactory();
factory.MetadataBuilderFlags = MetadataBuilderFlags.PreserveBlobIndices 
                             | MetadataBuilderFlags.PreserveTypeReferenceIndices;
imageBuilder.DotNetDirectoryFactory = factory;
```

> [!WARNING]
> Preserving heap indices copies over the original contents of the heaps
> to the new PE image \"as-is\". While AsmResolver tries to reuse blobs
> defined in the original heaps as much as possible, this is often not
> possible without also preserving RIDs in the tables stream. This might
> result in a significant increase in file size.

> [!NOTE]
> Preserving RIDs within metadata tables might require AsmResolver to
> inject placeholder rows in existing metadata tables that are solely
> there to fill up space between existing rows.

> [!WARNING]
> Preserving RIDs within metadata tables might require AsmResolver to make
> use of the Edit-And-Continue metadata tables (such as the pointer
> tables). The resulting tables stream could therefore be renamed from
> `#~` to `#-`, and the file size might increase.


## String folding in #Strings stream

Named metadata members (such as types, methods and fields) are assigned
a name by referencing a string in the `#Strings` stream by its starting
offset. When a metadata member has a name that is a suffix of another
member\'s name, then it is possible to only store the longer name in the
`#Strings` stream, and let the member with the shorter name use an
offset within the middle of this longer name. For example, consider two
members with the names `ABCDEFG` and `DEFG`. If `ABCDEFG` is stored at
offset `1`, then the name `DEFG` is implicitly defined at offset
`1 + 3 = 4`, and can thus be referenced without appending `DEFG` to the
stream a second time.

By default, the PE image builder will fold strings in the `#Strings`
stream as described in the above. However, for some input binaries, this
might make the building process take a significant amount of time.
Therefore, to disable this folding of strings, specify the
`NoStringsStreamOptimization` flag in your `DotNetDirectoryFactory`:

``` csharp
factory.MetadataBuilderFlags |= MetadataBuilderFlags.NoStringsStreamOptimization;
```

> [!WARNING]
> Some obfuscated binaries might include lots of members that have very
> long but similar names. For these types of binaries, disabling this
> optimization can result in a significantly larger output file size.

> [!NOTE]
> When `PreserveStringIndices` is set and string folding is enabled
> (`NoStringsStreamOptimization` is unset), the PE image builder will not
> fold strings from the original `#Strings` stream into each other.
> However, it will still try to reuse these original strings as much as
> possible.

## Preserving maximum stack depth

CIL method bodies work with a stack, and the stack has a pre-defined
size. This pre-defined size is defined by the `MaxStack` property of the
`CilMethodBody` class. By default, AsmResolver automatically calculates
the maximum stack depth of a method body upon writing the module to the
disk. However, this is not always desirable.

To override this behaviour, set `ComputeMaxStackOnBuild` to `false` on
all method bodies to exclude in the maximum stack depth calculation.

Alternatively, if you want to force the maximum stack depths should be
either preserved or recalculated, it is possible to provide a custom
implemenmtation of the `IMethodBodySerializer`, or configure the
`CilMethodBodySerializer`.

Below an example on how to preserve maximum stack depths for all methods
in the assembly:

``` csharp
DotNetDirectoryFactory factory = ...;
factory.MethodBodySerializer = new CilMethodBodySerializer
{
    ComputeMaxStackOnBuildOverride = false
}
```

> [!WARNING]
> Disabling max stack computation may have unexpected side-effects (such
> as rendering certain CIL method bodies invalid).

## Strong name signing

Assemblies can be signed with a strong-name signature. Open a strong
name private key from a file:

``` csharp
var snk = StrongNamePrivateKey.FromFile(@"C:\Path\To\keyfile.snk");
```

Prepare the image builder to delay-sign the PE image:

``` csharp
DotNetDirectoryFactory factory = ...;
factory.StrongNamePrivateKey = snk;
```

After writing the module to an output stream, use the `StrongNameSigner`
class to sign the image.

``` csharp
using Stream outputStream = ...
module.Write(outputStream, factory);

var signer = new StrongNameSigner(snk);
signer.SignImage(outputStream, module.Assembly.HashAlgorithm);
```

## Image Builder Diagnostics

.NET modules that contain invalid metadata and/or method bodies might
cause problems upon serializing it to a PE image or file. To inspect all
errors that occurred during the construction of a PE image, call the
`CreateImage` method with the `ErrorListener` property set to an
instance of the `DiagnosticBag` property. This is an implementation of
`IErrorListener` that collects all the problems that occurred during the
process:

``` csharp
// Set up a diagnostic bag as an error listener.
var diagnosticBag = new DiagnosticBag();
imageBuilder.ErrorListener = diagnosticBag;

// Build image.
var result = imageBuilder.CreateImage(module);

// Print all errors.
Console.WriteLine("Construction finished with {0} errors.", diagnosticBag.Exceptions.Count);
foreach (var error in diagnosticBag.Exceptions)
    Console.WriteLine(error.Message);
```

Whenever a problem is reported, AsmResolver attempts to recover or fill
in default data where corrupted data was encountered. To simply build
the PE image ignoring all diagnostic errors, it is also possible to pass
in `EmptyErrorListener.Instance` instead:

``` csharp
imageBuilder.ErrorListener = EmptyErrorListener.Instance;
```

> [!WARNING]
> Using `EmptyErrorListener` will surpress any non-critical builder
> errors, however these errors are typically indicative of an invalid
> executable being constructed. Therefore, even if an output file is
> produced, it may have unexpected side-effects (such as the file not
> functioning properly).

> [!NOTE]
> Setting an instance of `IErrorListener` in the image builder will only
> affect the building process. If the input module is initialized from a
> file containing invalid metadata, you may still experience reader
> errors, even if an `EmptyErrorListener` is specified. See
> [Advanced Module Reading](advanced-module-reading.md) for
> handling reader diagnostics.

To test whether any of the errors resulted in AsmResolver to abort the
construction of the image, use the `PEImageBuildResult::HasFailed`
property. If this property is set to `false`, the image stored in the
`ConstructedImage` property can be written to the disk:

``` csharp
if (!result.HasFailed)
{
    var fileBuilder = new ManagedPEFileBuilder();
    var file = fileBuilder.CreateFile(result.ConstructedImage);
    file.Write("output.exe");
}
```
