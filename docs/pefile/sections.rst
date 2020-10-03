Inspecting the PE sections
==========================

Sections can be read and modified by accessing the ``PEFile.Sections`` property, which is a collection of ``PESection`` objects.

.. code-block:: csharp

        foreach (var section in peFile.Sections)
        {
            Console.WriteLine(section.Name);
        }

Each ``PESection`` object also has the ``Contents`` property defined, which is a `IReadableSegment`. This object is capable of creating a `IBinaryStreamReader` instance:

.. code-block:: csharp

        var reader = section.CreateReader();

This can be used to read the data that is present in the section. If you want to get the entire section in a byte array, you can take the ``ToArray`` shortcut:

.. code-block:: csharp

        byte[] data = section.ToArray();
        

The ``Sections`` property is mutable, which means you can add new sections and remove others from the PE.

.. code-block:: csharp

        var section = new PESection(".asmres", SectionFlags.MemoryRead | SectionFlags.ContentInitializedData);
        section.Contents = new DataSegment(new byte[] {1, 2, 3, 4});

        peFile.Sections.Add(section);


Some sections (such as `.data` or `.bss`) contain uninitialized data, and might be resized in virtual memory at runtime. As such, the virtual size of the contents might be different than its physical size. You can use `VirtualSegment` to decorate a normal `ISegment` with a different virtual size.

.. code-block:: csharp

        var section = new PESection(".asmres", SectionFlags.MemoryRead | SectionFlags.ContentUninitializedData);
        var physicalContents = new DataSegment(new byte[] {1, 2, 3, 4});
        section = new VirtualSegment(physicalContents, 0x1000); // Create a new segment with a virtual size of 0x1000 bytes.
        
        peFile.Sections.Add(section);
