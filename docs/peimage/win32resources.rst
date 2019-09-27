Win32 Resources
===============

Win32 resources are additional files embedded into the PE image, and are typically stored in the ``.rsrc`` section.

Resources are exposed by the ``IPEImage.Resources`` property. This is an instance of an ``IResourceDirectory``, which contains the ``Entries`` property. Entries in a directory can either be another sub directory containing more entries, or a data entry (an instance of ``IResourceData``) with the raw contents of the resource.

Example
-------

The following example is a program that dumps the resources tree from a single PE image.

.. code-block:: csharp

    private const int IndentationWidth = 3;

    private static void Main(string[] args)
    {
        // Open the PE image.
        string filePath = args[0].Replace("\"", "");
        var peImage = PEImageBase.FromFile(filePath);

        // Dump the resources.
        PrintResourceDirectory(peImage.Resources);
    }

    private static void PrintResourceEntry(IResourceDirectoryEntry entry, int indentationLevel = 0)
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