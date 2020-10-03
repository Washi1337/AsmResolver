Basic I/O
=========

Every raw PE file interaction is done through classes defined by the ``AsmResolver.PE.File`` namespace:

.. code-block:: csharp

    using AsmResolver.PE.File;


Opening a PE file
-----------------

Opening a file can be done through one of the `FromXXX` methods:

.. code-block:: csharp

    var peFile = PEFile.FromFile(@"C:\myfile.exe");

.. code-block:: csharp

    byte[] raw = ...
    var peFile = PEFile.FromBytes(raw);

.. code-block:: csharp

    IBinaryStreamReader reader = ...
    var peFile = PEFile.FromReader(reader);

By default, AsmResolver assumes the PE file is in its unmapped form. This is usually the case when files are read directly from the file system. For memory mapped PE files, use the overload of the ``FromReader`` method, which allows for specifying the memory layout of the input.

.. code-block:: csharp

    IBinaryStreamReader reader = ...
    var peFile = PEFile.FromReader(reader, PEMappingMode.Mapped);


Writing PE files
----------------

Writing PE files can be done through the ``PEFile.Write`` method:

.. code-block:: csharp

    using (var fs = File.Create(@"C:\patched.exe"))
    {
        peFile.Write(new BinaryStreamWriter(fs));
    }

AsmResolver will then reassemble the file with all the changes you made. Note that this will also recalculate some fields in the headers, such as ``FileHeader.NumberOfSections``. Furthermore, it will also recalculate the offsets and virtual addresses of each section.