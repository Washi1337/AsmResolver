# Version Info (RT_VERSIONINFO)

The `RT_VERSIONINFO` resource type stores metadata describing product names, version numbers and copyright holders that are associated to a Portable Executable (PE) file.

The relevant code for reading and writing version information can be found in the following namespace:

```csharp
using AsmResolver.PE.Win32Resources.Version;
```

AsmResolver represents version metadata using the `VersionInfoResource` class.
In the following, basic usage of this class is described.


## Creating Version Info

Creating a new version info resource can be done using the constructor of `VersionInfoResource`:

```csharp
var versionInfo = new VersionInfoResource();
```

By default, this creates a language-neutral version info (LCID: 0). To customize this, use the constructor overload:

```csharp
var versionInfo = new VersionInfoResource(lcid: 1033);
```


## Reading Version Info

To extract existing version information from a Portable Executable file, you will need to access the root resource directory of a `PEImage`. 
Then, the main version info directory can be obtained using the `FromDirectory` factory method:

```csharp
PEImage image = ...;
var versionInfo = VersionInfoResource.FromDirectory(image.Resources);
```

If the executable contains multiple version info entries for different languages, use the `FindAllFromDirectory` method instead:

```csharp
PEImage image = ...;
var versionInfos = VersionInfoResource.FindAllFromDirectory(image.Resources);
```

To retrieve version info with a specific language identifier (LCID), use the `FromDirectory` overload accepting an extra parameter:

```csharp
PEImage image = ...;
var versionInfo = VersionInfoResource.FromDirectory(image.Resources, lcid: 1033);
```

## Fixed File Version Info

Every version info resource starts with a fixed file version info header, exposed via the `FixedVersionInfo` property.

```csharp
Console.WriteLine($"File Version:    {versionInfo.FixedVersionInfo.FileVersion}");
Console.WriteLine($"Product Version: {versionInfo.FixedVersionInfo.ProductVersion}");
Console.WriteLine($"Target OS:       {versionInfo.FixedVersionInfo.FileOS}");
```

All properties in this object are mutable, and thus can be modified to change the version info of the PE file.


## String and Var Tables

Version info metadata may also contain additional string tables that can contain arbitrary strings.
The way they are organized in optional blobs after the fixed version info, in good PE file format fashion with a good amount of redundancy.

String tables are stored in `StringFileInfo` instances, which can be created or extracted as follows:

```csharp
var stringInfo = new StringFileInfo();
versionInfo.AddEntry(stringInfo);
```

```csharp
var stringInfo = versionInfo.GetChild<StringFileInfo>(StringFileInfo.StringFileInfoKey);
```

A single string table can then be created as follows:

```csharp
var stringTable = new StringTable(languageIdentifier: 1033, codePage: 1200);
stringInfo.Tables.Add(stringTable);
```
```csharp
var stringTable = stringInfo.Tables[0];
```

String tables contain information such as product name and copyright information:

```csharp
Console.WriteLine($"Product Name: {stringTable[StringTable.ProductNameKey]}");
Console.WriteLine($"Copyright:    {stringTable[StringTable.LegalCopyrightKey]}");
```

Each string table is accompanied with a `VarTable`, stored in the `VarFileInfo` blob, containing both the language identifier and code page:

```csharp
var varInfo = new VarFileInfo();
versionInfo.AddEntry(varTable);
```

```csharp
var varTable = new VarTable();
varTable.Values.Add(1033u | 1200u << 16);
varInfo.Tables.Add(varTable);
```


## Writing Version Info

To serialize the (modified) version info metadata back to the resources of a PE image, use the `InsertIntoDirectory` method:

```csharp
PEImage image = ...;
VersionInfoResource versionInfo = ...;
versionInfo.InsertIntoDirectory(image.Resources);
```

The PE image can then be saved as normal.
Either rebuild the PE image (see [Writing a PE Image](../peimage/basics.md#writing-a-pe-image)), or simply add a new section to the PE file with the contents of `image.Resources` (see [Adding a new Section](../pefile/sections.md#adding-a-new-section)).