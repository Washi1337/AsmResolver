Imports Directory
=================

Most portable executables import functions and fields from other, external libraries. These are stored in a table in the imports data directory of the PE file. Each entry in this table defines a module that is loaded at runtime, and a set of members that are looked up.

All code relevant to the imports directory of a PE resides in the following namespace:

.. code-block:: csharp
    
    using AsmResolver.PE.Imports;


Imported Modules and Symbols
----------------------------

The ``IPEImage`` interface exposes the ``Imports`` property, which contains all members that are resolved at runtime, grouped by the defining module. Below an example of a program that lists all members imported by a given ``IPEImage`` instance: 

.. code-block:: csharp

    foreach (var module in peImage.Imports)
    {
        Console.WriteLine("Module: " + module.Name);

        foreach (var member in module.Symbols)
        {
            if (member.IsImportByName)
                Console.WriteLine("\t- " + member.Name);
            else
                Console.WriteLine("\t- #" + member.Ordinal);
        }

        Console.WriteLine();
    }


.. _pe-import-hash:

Import Hash
-----------

An Import Hash (ImpHash) of a PE image is a hash code that is calculated based on all symbols imported by the image. `Originally introduced in 2014 by Mandiant <https://www.fireeye.com/blog/threat-research/2014/01/tracking-malware-import-hashing.html>`_, an Import Hash can be used to help identifying malware families quickly, as malware samples that belong to the same family often have the same set of dependencies.

AsmResolver provides a built-in implementation for calculating the Import Hash. The hash can be obtained by using the ``GetImportHash`` extension method on ``IPEImage``:

.. code-block:: csharp

    IPEImage image = ...
    byte[] hash = image.GetImportHash();


Since the hash is computed based on the names of all imported symbols, symbols that are imported by ordinal need to be resolved. By default, AsmResolver uses a static lookup table for a couple of common Windows libraries, but should this resolution process be customized, then this can be done by providing a custom instance of ``ISymbolResolver``:

.. code-block:: csharp

    public class MySymbolResolver : ISymbolResolver
    {
        public ExportedSymbol? Resolve(ImportedSymbol symbol)
        {
            /* Resolve symbol by ordinal here... */
        }
    }

    IPEImage image = ...
    byte[] hash = image.GetImportHash(new MySymbolResolver());



While the Import Hash can be a good identifier for native PE images, for .NET images this is not the case. .NET images usually only import a single external symbol (either ``mscoree.dll!_CorExeMain`` or ``mscoree.dll!_CorDllMain``), and as such they will almost always have the exact same Import Hash. See :ref:`pe-typereference-hash` for an alternative for .NET images.