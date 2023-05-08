# Win32 Resources

Win32 resources are additional files embedded into the PE image, and are
typically stored in the `.rsrc` section. All classes relevant to Win32
resources can be found in the following namespace:

``` csharp
using AsmResolver.PE.Win32Resources;
```

## Directories

Resources are exposed by the `IPEImage.Resources` property, which
represents the root directory of all resources stored in the image.
Every directory (including the root directory) is represented by
instances of `IResourceDirectory`. This type contains the `Entries`
property. Entries in a directory can either be another sub directory
containing more entries, or a data entry (an instance of
`IResourceData`) with the raw contents of the resource.

``` csharp
IPEImage image = ...
IResourceDirectory root = image.Resources;

foreach (var entry in root.Entries)
{
    if (entry.IsDirectory)
        Console.WriteLine("Directory {0}.", entry.Id);
    else // if (entry.IsData)
        Console.WriteLine("Data {0}.", entry.Id);
}
```

Alternatively, you can access specific resources very easily by using
the `GetDirectory` and `GetData`:

``` csharp
IPEImage image = ...
IResourceData stringDataEntry = image.Resources
    .GetDirectory(ResourceType.String)  // Get string tables directory.
    .GetDirectory(251)                  // Get string block with ID 251
    .GetData(1033);                     // Get string with language ID 1033
```

Adding or replacing entries can be by either modifying the `Entries`
property directly, or by using the `AddOrReplace` method. The latter is
recommended as it ensures that an existing entry with the same ID is
replaced with the new one.

``` csharp
IPEImage image = ...
var newDirectory = new ResourceDirectory(ResourceType.String);
image.Resources.Entries.Add(newDirectory);
```

``` csharp
IPEImage image = ...
var newDirectory = new ResourceDirectory(ResourceType.String);
image.Resources.AddOrReplaceEntry(newDirectory);
```

Similarly, removing can be done by modifying the `Entries` directory, or
by using the `RemoveEntry` method:

``` csharp
IPEImage image = ...
image.Resources.RemoveEntry(ResourceType.String);
```

## Data Entries

Data entries are represented using the `IResourceData` interface, and
contain a property called `Contents` which is of type `ISegment`. You
can check if this is a `IReadableSegment`, or use the shortcuts
`CanRead` and `CreateReader` methods instead to read the raw contents of
the entry.

``` csharp
IPEImage image = ...
IResourceData dataEntry = image.Resources
    .GetDirectory(ResourceType.String)  // Get string tables directory.
    .GetDirectory(251)                  // Get string block with ID 251
    .GetData(1033);                     // Get string with language ID 1033

if (dataEntry.CanRead)
{
    BinaryStreamReader reader = dataEntry.CreateReader();
    // ...
}
```

Adding new data entries can be done by using the `ResourceData`
constructor:

``` csharp
IPEImage image = ...

var data = new ResourceData(id: 1033, contents: new DataSegment(new byte[] { ... }));
image.Resources
    .GetDirectory(ResourceType.String)
    .GetDirectory(251)
    .AddOrReplaceEntry(data);
```

## Example Traversal

The following example is a program that dumps the resources tree from a
single PE image.

``` csharp
private const int IndentationWidth = 3;

private static void Main(string[] args)
{
    // Open the PE image.
    string filePath = args[0].Replace("\"", "");
    var peImage = PEImage.FromFile(filePath);

    // Dump the resources.
    PrintResourceDirectory(peImage.Resources);
}

private static void PrintResourceEntry(IResourceEntry entry, int indentationLevel = 0)
{
    // Decide if we are dealing with a sub directory or a data entry.
    if (entry.IsDirectory)
        PrintResourceDirectory((IResourceDirectory) entry, indentationLevel);
    else if (entry.IsData)
        PrintResourcData((IResourceData) entry, indentationLevel);
}

private static void PrintResourceDirectory(IResourceDirectory directory, int indentationLevel = 0)
{
    string indentation = new string(' ', indentationLevel * IndentationWidth);

    // Print the name or ID of the directory.
    string displayName = directory.Name ?? "ID: " + directory.Id;
    Console.WriteLine("{0}+- Directory {1}", indentation, displayName);

    // Print all entries in the directory.
    foreach (var entry in directory.Entries)
        PrintResourceEntry(entry, indentationLevel + 1);
}

private static void PrintResourcData(IResourceData data, int indentationLevel)
{
    string indentation = new string(' ', indentationLevel * IndentationWidth);

    // Print the name of the data entry, as well as the size of the contents.
    string displayName = data.Name ?? "ID: " + data.Id;
    Console.WriteLine("{0}+- Data {1} ({2} bytes)", indentation, displayName, data.Contents.GetPhysicalSize());
}
```
