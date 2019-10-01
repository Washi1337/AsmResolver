.NET executables
================

Managed executables (applications written using a .NET language) contain an extra data directory in the optional header of the PE file format. This small data directory contains a header which is also known as the CLR 2.0 header, and references other structures such as the metadata directory, raw data for manifest resources and sometimes an extra native header in the case of mixed mode applications or zapped (ngen'ed) applications. 

.NET Directory
--------------

The .NET data directory can be accessed by the `IPEImage.DotNetDirectory` property.

.. code-block:: csharp

    IPEImage peImage = ...

    Console.WriteLine("Managed entrypoint: {0:X8}", peImage.DotNetDirectory.Entrypoint);


.NET metadata directory 
-----------------------

The metadata data directory is perhaps the most important data directory that is referenced by the .NET directory. It contains the metadata streams, such as the table and the blob stream, which play a key role in the execution of a .NET binary.

To access the metadata directory, access the `IDotNetDirectory.Metadata` property, which will provide you an instance of the `IMetadata` interface:

.. code-block:: csharp

    IMetadata metadata = peImage.DotNetDirectory.Metadata;

    Console.WriteLine("Metadata file format version: {0}.{1}", metadata.MajorVersion, metadata.MinorVersion);
    Console.WriteLine("Target .NET runtime version: " + metadata.VersionString);


.NET metadata streams
---------------------

The `IMetadata` interface also exposes the `Streams` property, a list of `IMetadataStream` instances.

.. code-block:: csharp

    foreach (var stream in metadata.Streams)
        Console.WriteLine("Name: " + stream.Name);


Some streams support reading the raw contents using a `IBinaryStreamReader`. Effectively, every stream that was read from the disk is readable in this way. Below an example of a program that dumps for each readable stream the contents to a file on the disk:

.. code-block:: csharp

    // Iterate over all readable streams.
    foreach (var stream in metadata.Streams.Where(s => s.CanRead))
    {
        // Create a reader that reads the raw contents of the stream.
        var reader = stream.CreateReader();

        // Write the contents to the disk.
        File.WriteAllBytes(stream.Name + ".bin", reader.ReadToEnd());
    }


The `Streams` property is mutable. You can add new streams, or remove existing streams:

.. code-block:: csharp

    // Create a new stream with the contents 1, 2, 3, 4.
    var data = new byte[] {1, 2, 3, 4};
    var newStream = new CustomMetadataStream("#Custom", data);

    // Add the stream to the metadata directory.
    metadata.Streams.Add(newStream);

    // Remove it again.
    metadata.Streams.RemoveAt(metadata.Streams.Count - 1);