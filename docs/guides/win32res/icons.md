# Icons and Cursors (RT_GROUP_CURSOR, RT_GROUP_ICON, RT_CURSOR, RT_ICON)

Icons and cursors follow a near-identical structure in the PE file format, and closely resemble the ICO file format.
Each icon is actually a set of individual icon files containing the same image in various resolutions, pixel formats and color palettes.

The relevant code for reading and writing icon resources can be found in the following namespace:

```csharp
using AsmResolver.PE.Win32Resources.Icon;
```

Icon resources are represented using the `IconResource` class.
In the following, basic usage of this class is described.

## Creating Icon Resources

To create a new icon resource, simply use the constructor of the `IconResource` class, specifying the type of icons to store.

```csharp
var icons = new IconResource(IconType.Icon);
```

```csharp
var cursors = new IconResource(IconType.Cursor);
```


## Reading Icon Resources

To extract existing icons from a Portable Executable file, you will need to access the root resource directory of a `PEImage`. 
Then, all icon groups can be obtained using the `FromDirectory` factory method:

```csharp
PEImage image = ...;
var icons = IconResource.FromDirectory(image.Resources, IconType.Icon);
```


## Icon Groups

Icon groups are exposed using the `Groups` property, and can be iterated and modified:

```csharp
foreach (var iconGroup in icons.Groups)
{
    Console.WriteLine($"ID: {iconGroup.Id}, LCID: {iconGroup.Lcid}");
}
```

Each icon group consists of a set of `Icons`, each containing pixel data for a specific icon of a particular resolution and format:

```csharp
IconGroup iconGroup = ...;
foreach (var icon in iconGroup.Icons)
{
    Console.WriteLine($"- {icon.Id}, {icon.Width}x{icon.Height}, {icon.PixelData.GetPhysicalSize()} bytes");
}
```

A single `IconEntry` contains both fields for the ICO or CUR header such as `Width` and `Height`, as well as the raw pixel data in the `PixelData` property.
AsmResolver does not provide a way to interpret or reconstruct pixel data stored in an icon entry.
It only exposes a raw `ISegment` which can be read using a `BinaryStreamReader` or turned into a byte array (see [Reading Segment Contents](../core/segments.md#reading-segment-contents)).

```csharp
byte[] rawData = icon.PixelData.WriteIntoArray();
```

> [!NOTE]
> The `PixelData` property contains the icon's raw data **excluding** the ICO or CUR header.
> When modifying icon entries, make sure the header fields in the `IconEntry` object itself are therefore updated accordingly.


All collections and properties are mutable, and as such can be used to modify icons stored in a PE file.


## Writing Icon Resources

To serialize the (modified) icon resource back to the resources of a PE image, use the `InsertIntoDirectory` method:

```csharp
PEImage image = ...;
IconResource icons = ...;
icons.InsertIntoDirectory(image.Resources);
```

The PE image can then be saved as normal.
Either rebuild the PE image (see [Writing a PE Image](../peimage/basics.md#writing-a-pe-image)), or simply add a new section to the PE file with the contents of `image.Resources` (see [Adding a new Section](../pefile/sections.md#adding-a-new-section)).