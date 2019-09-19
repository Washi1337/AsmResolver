The basics
==========

Every raw PE file interaction is done through classes defined by the **AsmResolver.PE.File** namespace:

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


Inspecting the PE headers
-------------------------

After you obtained an instance of the **PEFile** class, it is possible to read and edit various properties in the DOS header, COFF file header and optional header. They each have a designated property:

.. code-block:: csharp

    Console.WriteLine("e_flanew: {0:X8}", peFile.DosHeader.NextHeaderOffset);
    Console.WriteLine("Machine: {0:X8}", peFile.FileHeader.Machine);
    Console.WriteLine("Entrypoint: {0:X8}", peFile.OptionalHeader.AddressOfEntrypoint);

Every change made to these headers will be reflected in the output executable, however very little verification on these values is done. 

Inspecting the PE sections
--------------------------

Sections can be read and modified by accessing the **PEFile.Sections** property, which is a collection of **PESection** objects. Each of these objects has a **Header** property, representing the section header as it appears in the PE header, and a **Contents** property, representing the code or data that this section stores:

.. code-block:: csharp

        foreach (var section in peFile.Sections)
        {
            Console.WriteLine(section.Header.Name);
        }
