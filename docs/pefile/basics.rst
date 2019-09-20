Basic PE File operations
========================

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

Sections can be read and modified by accessing the **PEFile.Sections** property, which is a collection of **PESection** objects. Each of these objects has a **Header** property, representing the section header as it appears in the PE header:

.. code-block:: csharp

        foreach (var section in peFile.Sections)
        {
            Console.WriteLine(section.Header.Name);
        }

Each **PESection** object also has the **Contents** property defined, which is a `IReadableSegment`. This object is capable of creating a `IBinaryStreamReader` instance:

.. code-block:: csharp

        var reader = section.CreateReader();

This can be used to read the data that is present in the section. If you want to get the entire section in a byte array, you can take the **ToArray** shortcut:

.. code-block:: csharp

        byte[] data = section.ToArray();
        

The **Sections** property is mutable, which means you can add new sections and remove others from the PE.

.. code-block:: csharp

        var section = new PESection(".asmres", SectionFlags.MemoryRead | SectionFlags.ContentInitializedData);
        section.Contents = new DataSegment(new byte[] {1, 2, 3, 4});

        peFile.Sections.Add(section);

Writing PE files
----------------

Writing PE files can be done through the **PEFile.Write** method:

.. code-block:: csharp

    using (var fs = File.Create(@"C:\patched.exe"))
    {
        peFile.Write(new BinaryStreamWriter(fs));
    }

AsmResolver will then reassemble the file with all the changes you made. Note that this will also recalculate some fields in the headers, such as **FileHeader.NumberOfSections**. Furthermore, it will also recalculate the offsets and virtual addresses of each section.