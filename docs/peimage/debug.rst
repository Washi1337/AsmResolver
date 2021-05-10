Debug Directory
===============

The debug data directory is used in portable executables to store compiler-generated debug information. In most cases, this information is a reference to a Program Debug Database (``.pdb``) file.

The relevant classes for this article are stored in the following namespace:

.. code-block:: csharp

    using AsmResolver.PE.Debug;


The Debug Data Entries
----------------------

The ``IPEImage`` exposes all debug information through the ``DebugData`` property. This is a list of ``DebugDataEntry``, providing access to the type of the debug data, as well as the version and raw contents of the data that is stored.

.. code-block:: csharp

    foreach (DebugDataEntry entry in image.DebugData)
    {
        Console.WriteLine("Debug Data Type: {0}", entry.Contents.Type);
        Console.WriteLine("Version: {0}.{1}", entry.MajorVersion, entry.MinorVersion);
        Console.WriteLine("Data start: {0:X8}", entry.Contents.Rva);
    }

Depending on the type of the debug data entry, the ``Contents`` property will be modelled using different implementations of ``IDebugDataSegment``.

.. note::
    
    If a PE contains debug data using an unsuported or unrecognized format, then the contents will be modelled with a ``CustomDebugDataSegment`` instance instead, which exposes the raw contents as an ``ISegment``.

.. note:: 

    Currently, AsmResolver only has rich-support for ``CodeView`` debug data.


CodeView Data
-------------

CodeView data is perhaps the most common form of debug data that can appear in a portable executable. It is emitted by a lot of compilers, such as the Visual C++ compiler, and is also used by many .NET languages such as C# and VB.NET. CodeView data is modelled using implementations of the ``CodeViewDataSegment`` abstract class.

.. code-block:: csharp

    if (entry.Contents.Type == DebugDataType.CodeView)
    {
        var codeViewData = (CodeViewDataSegment) entry.Contents;
        ...
    }

There are various formats used by CodeView data segments, and this format is decided by the ``Signature`` property of the ``CodeViewDataSegment`` class. The most common format used in a CodeView segment, is the RSDS segment. This format stores a path to the Program Debug Database (``*.pdb``) file that is associated to the image.

.. code-block:: csharp

    if (codeViewData.Signature == CodeViewSignature.Rsds)
    {
        var rsdsData = (RsdsDataSegment) data;
        Console.WriteLine("PDB Path: {0}", rsdsData.Path);
    }
