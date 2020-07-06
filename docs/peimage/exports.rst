Exports Directory
=================

Dynamically linked libraries (DLLs) often expose symbols through defining exports in the exports directory. 

The ``IPEImage`` interface exposes the ``Exports`` property, exposing a mutable instance of `ExportDirectory`, which defines the following properties:

- ``Name``: The name of the dynamically linked library.
- ``BaseOrdinal``: The base ordinal of all exported symbols.
- ``Entries``: A list of exported symbols.

Exported symbols are represented using the ``ExportedSymbol`` class, which defines:

- ``Name``: The name of the exported symbol. If this value is ``null``, the symbol is exported by ordinal rather than by name.
- ``Ordinal``: The ordinal of the exported symbol. This property is automatically updated when ``BaseOrdinal`` of the parent directory is updated.
- ``Address``: A reference to the segment representing the symbol. 
    - For exported functions, this reference points to the first instruction that is executed. 
    - For exported fields, this reference points to the first byte of data that this field consists of.


Example
-------

Below is an example of a program that lists all symbols exported by a given ``IPEImage`` instance: 

.. code-block:: csharp

    foreach (var symbol in peImage.Exports.Entries)
    {
        Console.WriteLine("Ordinal: " + symbol.Ordinal);
        if (symbol.IsByName) 
            Console.WriteLine("Name: " + symbol.Name);
        Console.WriteLine("Address: " + symbol.Address.Rva.ToString("X8"));
    }

