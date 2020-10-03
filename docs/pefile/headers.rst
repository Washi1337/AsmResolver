Inspecting the PE headers
=========================

After you obtained an instance of the ``PEFile`` class, it is possible to read and edit various properties in the DOS header, COFF file header and optional header. They each have a designated property:

.. code-block:: csharp

    Console.WriteLine("e_flanew: {0:X8}", peFile.DosHeader.NextHeaderOffset);
    Console.WriteLine("Machine: {0:X8}", peFile.FileHeader.Machine);
    Console.WriteLine("Entrypoint: {0:X8}", peFile.OptionalHeader.AddressOfEntrypoint);

Every change made to these headers will be reflected in the output executable, however very little verification on these values is done. 
