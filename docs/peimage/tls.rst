TLS Directory
=============

Executables that use multiple threads might require static (non-stack) memory that is local to a specific thread. The PE file format allows for defining segments of memory within the file that specifies what this memory should like, and how it should be initialized. This information is stored inside the Thread Local Storage (TLS) data directory.

All code relevant to the TLS data directory of a PE resides in the following namespace:

.. code-block:: csharp
    
    using AsmResolver.PE.Tls;


Template Data, Zero Fill and Index
----------------------------------

The PE file format defines a segment of memory within the TLS data directory that specifies how the thread local data should be initialized. This is represented using the following three properties:

- ``TemplateData``: A segment representing the initial data of the static memory.
- ``SizeOfZeroFill``: The number of extra zeroes appended to the end of the initial data as specified by ``TemplateData``
- ``Index``: A reference that will receive the thread index. This is supposed to be a reference in a writable PE section (typically ``.data``).


.. code-block:: csharp
    
    var indexSegment = new DataSegment(new byte[8]);

    var directory = new TlsDirectory
    {
        TemplateData = new DataSegment(new byte[] { ... }),
        SizeOfZeroFill = 0x1000,
        Index = indexSegment.ToReference()
    };


TLS Callback Functions
----------------------

Next to static initialization data, it is also possible to specify a list of functions called TLS Callbacks that are supposed to further initialize the thread local storage. This is exposed through the ``CallbackFunctions`` property, which is a list of references to the start of every TLS callback function.

.. code-block:: csharp

    for (int i = 0; i < directory.CallbackFunctions.Count; i++)
    {
        Console.WriteLine("TLS Callback #{0}: {1:X8}", directory.CallbackFunctions.Rva);
    }


Creating new TLS directories
----------------------------

Since the TLS data directory stores its data using virtual addresses (VA) rather than relative virtual addresses (RVA), AsmResolver requires the image base as well as the pointer size. This is done through the ``ImageBase`` and ``Is32Bit`` properties. By default, the following values are assumed:

.. code-block:: csharp

    var directory = new TlsDirectory();
    directory.ImageBase = 0x00400000;
    directory.Is32Bit = true;


Typically, you should make sure they are in sync with the values found in the file and optional header of the final PE file. Upon reading from an existing PE file, these two properties are initialized to the values stored in these two headers.

When building a relocatable PE file, you might also need to add base address relocations to the VAs inside the TLS directory. To quickly get all the base relocations required, use the ``GetRequiredBaseRelocations`` method:

.. code-block:: csharp

    using AsmResolver.PE.Relocations;

    IPEImage image = ...;

    foreach (var relocation in image.TlsDirectory.GetRequiredBaseRelocations())
        image.Relocations.Add(relocation);