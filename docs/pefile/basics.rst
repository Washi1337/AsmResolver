Basic I/O
=========

Every raw PE file interaction is done through classes defined by the ``AsmResolver.PE.File`` namespace:

.. code-block:: csharp

    using AsmResolver.PE.File;


Creating a new PE file
----------------------

Creating a PE file can be done through one of the ``PEFile`` constructors:

.. code-block:: csharp

    var peFile = new PEFile();


This will create a new empty PE file with 0 sections, and sets some values in the file header and optional header that are typical for a 32-bit Windows console application targeting the x86 platform.


Opening a PE file
-----------------

Opening a PE file can be done through one of the ``FromXXX`` methods:

.. code-block:: csharp

    byte[] raw = ...
    var peFile = PEFile.FromBytes(raw);
    
.. code-block:: csharp

    var peFile = PEFile.FromFile(@"C:\myfile.exe");

.. code-block:: csharp

    BinaryStreamReader reader = ...
    var peFile = PEFile.FromReader(reader);


By default, AsmResolver assumes the PE file is in its unmapped form. This is usually the case when files are read directly from the file system. For memory-mapped PE files, use the overload of the ``FromReader`` method, which allows for specifying the memory layout of the input.

.. code-block:: csharp

    BinaryStreamReader reader = ...
    var peFile = PEFile.FromReader(reader, PEMappingMode.Mapped);


If you want to read large files (+100MB), consider using memory-mapped I/O instead:

.. code-block:: csharp

    using var service = new MemoryMappedFileService();
    var peFile = PEFile.FromFile(service.OpenFile(@"C:\myfile.exe"));


On Windows, if a module is loaded and mapped in memory (e.g. as a native dependency or by the means of ``LoadLibrary``), it is possible to load the PE file from memory by providing the ``HINSTANCE`` (a.k.a. module base address):

.. code-block:: csharp

    IntPtr hInstance = ...
    var peFile = PEFile.FromModuleBaseAddress(hInstance);


Writing PE files
----------------

Writing PE files can be done through the ``PEFile.Write`` method:

.. code-block:: csharp

    using (var fs = File.Create(@"C:\patched.exe"))
    {
        peFile.Write(new BinaryStreamWriter(fs));
    }

AsmResolver will then reassemble the file with all the changes you made. Note that this will also recalculate some fields in the headers, such as ``FileHeader.NumberOfSections``. Furthermore, it will also recalculate the offsets and virtual addresses of each section.