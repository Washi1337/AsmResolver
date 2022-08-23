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

Adding a new TLS directory to an image can be done using the parameterless constructor of the ``TlsDirectory`` class:

.. code-block:: csharp

    var tlsDirectory = new TlsDirectory();
    image.TlsDirectory = tlsDirectory;

A TLS directory references all its sub segments using virtual addresses (VA) rather than relative addresses (RVA). This means that constructing a relocatable PE image with a TLS directory requires base relocation entries to be registered that let the Windows PE loader rebase all virtual addresses used in the directory when necessary. To quickly register all the required base relocations, you can call the ``GetRequiredBaseRelocations`` method and add all returned entries to the base relocation directory of the PE image:

.. code-block:: csharp

    using AsmResolver.PE.Relocations;

    IPEImage image = ...;

    foreach (var relocation in tlsDirectory.GetRequiredBaseRelocations())
        image.Relocations.Add(relocation);
