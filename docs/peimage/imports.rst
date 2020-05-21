Imports Directory
=================

Most portable executables import functions and fields from other, external libraries. These are stored in a table in the imports data directory of the PE file. Each entry in this table defines a module that is loaded at runtime, and a set of members that are looked up.

The ``IPEImage`` interface exposes the ``Imports`` property, which contains all members that are resolved at runtime, grouped by the defining module.

Example
-------

Below is an example of a program that lists all members imported by a given ``IPEImage`` instance: 

.. code-block:: csharp

    foreach (var module in peImage.Imports)
    {
        Console.WriteLine("Module: " + module.Name);

        foreach (var member in module.Members)
        {
            if (member.IsImportByName)
                Console.WriteLine("\t- " + member.Name);
            else
                Console.WriteLine("\t- #" + member.Ordinal);
        }

        Console.WriteLine();
    }

