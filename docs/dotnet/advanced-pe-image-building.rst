Advanced PE Image Building
--------------------------

The easiest way to write a .NET module to the disk is by using the ``Write`` method:

..code-block:: csharp

    module.Write(@"C:\Path\To\Output\Binary.exe");


This method is essentially a shortcut for invoking the ``ManagedPEImageBuilder`` and ``ManagedPEFileBuilder`` classes, and will completely reconstruct the PE image, serialize it into a PE file and write the PE file to the disk. 

While this is easy, and would probably work for most .NET module processing, it does not provide much flexibility. To get more control over the construction of the new PE image, it is therefore not recommended to use a different overload of the ``Write`` method, were we pass on a custom ``IPEFileBuilder``, or a configured ``ManagedPEImageBuilder``:

.. code-block:: csharp

    var imageBuilder = new ManagedPEFileBuilder();
    
    /* Configuration of imageBuilder here... */

    module.Write(@"C:\Path\To\Output\Binary.exe", imageBuilder);

This article explores various features about the ``ManagedPEImageBuilder`` class that can be configured.

Preserving raw metadata structure
---------------------------------

Some .NET modules are carefully crafted and rely on the raw structure of all metadata streams. These kinds of modules often rely on one of the following:

- RIDs of rows within a metadata table.
- Indices of blobs within the ``#Blob``, ``#Strings``, ``#US`` or ``#GUID`` heaps.

The default PE image builder for .NET modules (``ManagedPEImageBuilder``) defines a property called ``DotNetDirectoryFactory``, which contains the object responsible for constructing the .NET data directory, can be configured to preserve as much of this structure as possible. With the help of the ``MetadataBuilderFlags`` enum, it is possible to indicate which structures of the metadata directory need to preserved.

Below an example on how to configure the image builder to preserve blob data and all metadata tokens to type references:

.. code-block:: csharp

    var factory = new DotNetDirectoryFactory();
    factory.MetadataBuilderFlags = MetadataBuilderFlags.PreserveBlobIndices 
                                 | MetadataBuilderFlags.PreserveTypeReferenceIndices;
    imageBuilder.DotNetDirectoryFactory = factory;

If everything is supposed to be preserved as much as possible, then instead of specifying all flags defined in the ``MetadataBuilderFlags`` enum, we can also use ``MetadataBuilderFlags.PreserveAll`` as a shortcut. 

.. warning::

    Preserving heap indices copies over the original contents of the heaps to the new PE image "as-is". While AsmResolver tries to reuse blobs defined in the original heaps as much as possible, this is often not possible without also preserving RIDs in the tables stream. This might result in a significant increase in file size.

.. note::

    Preserving RIDs within metadata tables might require AsmResolver to inject placeholder rows in existing metadata tables that are solely there to fill up space between existing rows.

.. warning::

    Preserving RIDs within metadata tables might require AsmResolver to make use of the Edit-And-Continue metadata tables (such as the pointer tables). The resulting tables stream could therefore be renamed from ``#~`` to ``#-``, and the file size might increase.