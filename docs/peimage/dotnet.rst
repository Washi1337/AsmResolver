.NET Data Directories
=====================

Managed executables (applications written using a .NET language) contain an extra data directory in the optional header of the PE file format. This small data directory contains a header which is also known as the CLR 2.0 header, and references other structures such as the metadata directory, raw data for manifest resources and sometimes an extra native header in the case of mixed mode applications or zapped (ngen'ed) applications. 

.NET directory / CLR 2.0 header
-------------------------------

The .NET data directory can be accessed by the ``IPEImage.DotNetDirectory`` property.

.. code-block:: csharp

    IPEImage peImage = ...

    Console.WriteLine("Managed entrypoint: {0:X8}", peImage.DotNetDirectory.Entrypoint);


Metadata directory 
-----------------------

The metadata data directory is perhaps the most important data directory that is referenced by the .NET directory. It contains the metadata streams, such as the table and the blob stream, which play a key role in the execution of a .NET binary.

To access the metadata directory, access the ``IDotNetDirectory.Metadata`` property, which will provide you an instance of the ``IMetadata`` interface:

.. code-block:: csharp

    IMetadata metadata = peImage.DotNetDirectory.Metadata;

    Console.WriteLine("Metadata file format version: {0}.{1}", metadata.MajorVersion, metadata.MinorVersion);
    Console.WriteLine("Target .NET runtime version: " + metadata.VersionString);


Metadata streams
---------------------

The ``IMetadata`` interface also exposes the ``Streams`` property, a list of ``IMetadataStream`` instances.

.. code-block:: csharp

    foreach (var stream in metadata.Streams)
        Console.WriteLine("Name: " + stream.Name);

Alternatively, it is possible to get a stream by its name using the ``GetStream(string)`` shortcut:

.. code-block:: csharp

    var stringsStream = metadata.GetStream("#Strings");

Or grab the stream by its type:

.. code-block:: csharp

    var stringsStream = metadata.GetStream<StringsStream>;

AsmResolver supports parsing streams using the names in the table below. Any stream with a different name will be converted to a ``CustomMetadataStream``.

+---------------------------+------------------------+
| Name                      | Class                  |
+===========================+========================+
| ``#~`` ``#-`` ``#Schema`` | ``TablesStream``       |
+---------------------------+------------------------+
| ``#Blob``                 | ``BlobStream``         |
+---------------------------+------------------------+
| ``#GUID``                 | ``GuidStream``         |
+---------------------------+------------------------+
| ``#Strings``              | ``StringsStream``      |
+---------------------------+------------------------+
| ``#US``                   | ``UserStringsStream``  |
+---------------------------+------------------------+

Some streams support reading the raw contents using a ``IBinaryStreamReader``. Effectively, every stream that was read from the disk is readable in this way. Below an example of a program that dumps for each readable stream the contents to a file on the disk:

.. code-block:: csharp

    // Iterate over all readable streams.
    foreach (var stream in metadata.Streams.Where(s => s.CanRead))
    {
        // Create a reader that reads the raw contents of the stream.
        var reader = stream.CreateReader();

        // Write the contents to the disk.
        File.WriteAllBytes(stream.Name + ".bin", reader.ReadToEnd());
    }


The ``Streams`` property is mutable. You can add new streams, or remove existing streams:

.. code-block:: csharp

    // Create a new stream with the contents 1, 2, 3, 4.
    var data = new byte[] {1, 2, 3, 4};
    var newStream = new CustomMetadataStream("#Custom", data);

    // Add the stream to the metadata directory.
    metadata.Streams.Add(newStream);

    // Remove it again.
    metadata.Streams.RemoveAt(metadata.Streams.Count - 1);


Blob, Strings, US and GUID streams
----------------------------------

The blob, strings, user-strings and GUID streams are all very similar in the sense that they all provide a storage for data referenced by the tables stream. Each of these streams have a very similar API in AsmResolver.

+------------------------+----------------------+
| Class                  | Method               |
+========================+======================+
| ``BlobStream``         | ``GetBlobByIndex``   |
+------------------------+----------------------+
| ``GuidStream``         | ``GetGuidByIndex``   |
+------------------------+----------------------+
| ``StringsStream``      | ``GetStringByIndex`` |
+------------------------+----------------------+
| ``UserStringsStream``  | ``GetStringByIndex`` |
+------------------------+----------------------+

Example:

.. code-block:: csharp

    var stringsStream = metadata.GetStream<StringsStream>();
    string value = stringsStream.GetStringByIndex(0x1234);

Since blobs in the blob stream have a specific format, just obtaining the `byte[]` of a blob might not be all that useful. Therefore, the ``BlobStream`` has an extra ``GetBlobReaderByIndex`` method, that allows for parsing each blob using an ``IBinaryStreamReader`` object instead:


.. code-block:: csharp

    var blobStream = metadata.GetStream<BlobStream>();
    var reader = blobStream.GetBlobReaderByIndex(0x1234);

    // Use reader to parse the blob signature ...

Tables stream
-------------

The tables stream (``#~``, ``#-`` or ``#Schema``) is the main stream stored in the .NET binary. It provides tables for all members defined in the assembly, as well as all references that the assembly uses. The tables stream is represented by the ``TablesStream`` class and can be obtained in the same way as any other metadata stream:

.. code-block:: csharp

    TablesStream tablesStream = metadata.GetStream<TablesStream>();

Metadata tables are represented by the ``IMetadataTable`` interface. Individal tables can be accessed using the `GetTable` method:

.. code-block:: csharp

    IMetadataTable typeDefTable = tablesStream.GetTable(TableIndex.TypeDef);

Tables can also be obtained by their row type:

.. code-block:: csharp

    MetadataTable<TypeDefinitionRow> typeDefTable = tablesStream.GetTable<TypeDefinitionRow>();

The latter option allows for a more type-safe interaction with the table as well, as each metadata table is associated with its own row structure. Below a table of all row definitions:

+-------------+-----------------------------+--------------------------------+
| Table index | Name (as per specification) | AsmResolver row structure name |
+=============+=============================+================================+
| 0           | Module                      | ``ModuleDefinitionRow``        |
+-------------+-----------------------------+--------------------------------+
| 1           | TypeRef                     | ``TypeDefinitionRow``          |
+-------------+-----------------------------+--------------------------------+
| 2           | TypeDef                     | ``TypeReferenceRow``           |
+-------------+-----------------------------+--------------------------------+
| 3           | FieldPtr                    | ``FieldPointerRow``            |
+-------------+-----------------------------+--------------------------------+
| 4           | Field                       | ``FieldDefinitionRow``         |
+-------------+-----------------------------+--------------------------------+
| 5           | MethodPtr                   | ``MethodPointerRow``           |
+-------------+-----------------------------+--------------------------------+
| 6           | Method                      | ``MethodDefinitionRow``        |
+-------------+-----------------------------+--------------------------------+
| 7           | ParamPtr                    | ``ParameterPointerRow``        |
+-------------+-----------------------------+--------------------------------+
| 8           | Param                       | ``ParameterDefinitionRow``     |
+-------------+-----------------------------+--------------------------------+
| 9           | InterfaceImpl               | ``InterfaceImplementationRow`` |
+-------------+-----------------------------+--------------------------------+
| 10          | MemberRef                   | ``MemberReferenceRow``         |
+-------------+-----------------------------+--------------------------------+
| 11          | Constant                    | ``ConstantRow``                |
+-------------+-----------------------------+--------------------------------+
| 12          | CustomAttribute             | ``CustomAttributeRow``         |
+-------------+-----------------------------+--------------------------------+
| 13          | FieldMarshal                | ``FieldMarshalRow``            |
+-------------+-----------------------------+--------------------------------+
| 14          | DeclSecurity                | ``SecurityDeclarationRow``     |
+-------------+-----------------------------+--------------------------------+
| 15          | ClassLayout                 | ``ClassLayoutRow``             |
+-------------+-----------------------------+--------------------------------+
| 16          | FieldLayout                 | ``FieldLayoutRow``             |
+-------------+-----------------------------+--------------------------------+
| 17          | StandAloneSig               | ``StandAloneSignatureRow``     |
+-------------+-----------------------------+--------------------------------+
| 18          | EventMap                    | ``EventMapRow``                |
+-------------+-----------------------------+--------------------------------+
| 19          | EventPtr                    | ``EventPointerRow``            |
+-------------+-----------------------------+--------------------------------+
| 20          | Event                       | ``EventDefinitionRow``         |
+-------------+-----------------------------+--------------------------------+
| 21          | PropertyMap                 | ``PropertyMapRow``             |
+-------------+-----------------------------+--------------------------------+
| 22          | PropertyPtr                 | ``PropertyPointerRow``         |
+-------------+-----------------------------+--------------------------------+
| 23          | Property                    | ``PropertyDefinitionRow``      |
+-------------+-----------------------------+--------------------------------+
| 24          | MethodSemantics             | ``MethodSemanticsRow``         |
+-------------+-----------------------------+--------------------------------+
| 25          | MethodImpl                  | ``MethodImplementationRow``    |
+-------------+-----------------------------+--------------------------------+
| 26          | ModuleRef                   | ``ModuleReferenceRow``         |
+-------------+-----------------------------+--------------------------------+
| 27          | TypeSpec                    | ``TypeSpecificationRow``       |
+-------------+-----------------------------+--------------------------------+
| 28          | ImplMap                     | ``ImplementatinoMappingRow``   |
+-------------+-----------------------------+--------------------------------+
| 29          | FieldRva                    | ``FieldRvaRow``                |
+-------------+-----------------------------+--------------------------------+
| 30          | EncLog                      | ``EncLogRow``                  |
+-------------+-----------------------------+--------------------------------+
| 31          | EncMap                      | ``EncMapRow``                  |
+-------------+-----------------------------+--------------------------------+
| 32          | Assembly                    | ``AssemblyDefinitionRow``      |
+-------------+-----------------------------+--------------------------------+
| 33          | AssemblyProcessor           | ``AssemblyProcessorRow``       |
+-------------+-----------------------------+--------------------------------+
| 34          | AssemblyOS                  | ``AssemblyOSRow``              |
+-------------+-----------------------------+--------------------------------+
| 35          | AssemblyRef                 | ``AssemblyReferenceRow``       |
+-------------+-----------------------------+--------------------------------+
| 36          | AssemblyRefProcessor        | ``AssemblyRefProcessorRow``    |
+-------------+-----------------------------+--------------------------------+
| 37          | AssemblyRefOS               | ``AssemblyRefOSRow``           |
+-------------+-----------------------------+--------------------------------+
| 38          | File                        | ``FileReferenceRow``           |
+-------------+-----------------------------+--------------------------------+
| 39          | ExportedType                | ``ExportedTypeRow``            |
+-------------+-----------------------------+--------------------------------+
| 40          | ManifestResource            | ``ManifestResourceRow``        |
+-------------+-----------------------------+--------------------------------+
| 41          | NestedClass                 | ``NestedClassRow``             |
+-------------+-----------------------------+--------------------------------+
| 42          | GenericParam                | ``GenericParamRow``            |
+-------------+-----------------------------+--------------------------------+
| 43          | MethodSpec                  | ``MethodSpecificationRow``     |
+-------------+-----------------------------+--------------------------------+
| 44          | GenericParamConstraint      | ``GenericParamConstraintRow``  |
+-------------+-----------------------------+--------------------------------+

Metadata tables are similar to normal ``ICollection<T>`` instances. They provide enumerators, indexers and methods to add or remove rows from the table.

.. code-block:: csharp

    Console.WriteLine("Number of types: " + typeDefTable.Count);

    TypeDefinitionRow firstType = typeDefTable[0];

    foreach (var typeRow in typeDefTable)
    {
        // ...
    }

Using the other metadata streams, it is possible to resolve all columns. Below an example that prints the name and namespace of each type row in the type definition table in a file.

.. code-block:: csharp

    // Load PE image.
    var peImage = PEImage.FromFile(@"C:\file.exe");

    // Obtain relevant streams.
    var metadata = peImage.DotNetDirectory.Metadata;
    var tablesStream = metadata.GetStream<TablesStream>();
    var stringsStream = metadata.GetStream<StringsStream>();
    
    // Go over each type definition in the file.
    var typeDefTable = tablesStream.GetTable<TypeDefinitionRow>();
    foreach (var typeRow in typeDefTable)
    {
        // Resolve name and namespace columns using the #Strings stream.
        string ns = stringsStream.GetStringByIndex(typeRow.Namespace);
        string name = stringsStream.GetStringByIndex(typeRow.Name);

        // Print name and namespace:
        Console.WriteLine(string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}");
    }


Method and FieldRVA
-------------------

Every row structure defined in AsmResolver respects the specification described by the CLR itself. However, there are two exceptions to this rule, and those are the **Method** and **FieldRVA** rows. According to the specification, both of these rows have an **RVA** column that references a segment in the original PE file. Since this second layer of abstraction attempts to abstract away any file offset or virtual address, these columns are replaced with properties called ``Body`` and ``Data`` respectively, both of type ``ISegmentReference`` instead.

``ISegmentReference`` exposes a method ``CreateReader()``, which automatically resolves the RVA that was stored in the row, and creates a new input stream that can be used to parse e.g. method bodies or field data.

**Reading method bodies:**

Reading a managed CIL method body can be done using ``CilRawMethodBody.FromReader`` method:

.. code-block:: csharp

    var methodTable = tablesStream.GetTable<MethodDefinitionRow>();
    var firstMethod = methodTable[0];
    var methodBody = CilRawMethodBody.FromReader(firstMethod.Body.CreateReader());

It is important to note that the user is not bound to use ``CilRawMethodBody``. In the case that the ``Native`` (``0x0001``) flag is set in ``MethodDefinitionRow.ImplAttributes``, the implementation of the method body is not written in CIL, but using native code that uses an instruction set dependent on the platform that this application is targeting. Since the bounds of such a method body is not always well-defined, AsmResolver does not do any parsing on its own. However, using the ``CreateReader()`` method, it is still possible to decode instructions from this method body, using a custom instruction decoder.

**Reading field data:**

Reading field data can be done in a similar fashion as reading method bodies. Again use the ``CreateReader()`` method to gain access to the raw data of the initial value of the field referenced by a **FieldRVA** row.

.. code-block:: csharp

    var fieldRvaTable = tablesStream.GetTable<FieldRvaRow>();
    var firstRva = fieldRvaTable[0];
    var reader = firstRva.Data.CreateReader();

**Creating new segment references:**

Creating new segment references not present in the current PE image yet can for example be done by creating an instance of ``SegmentReference``, which is a wrapper for any ``IReadableSegment`` object.

.. code-block:: csharp

    var myData = new DataSegment(new byte[] {1, 2, 3, 4});
    var fieldRva = new FieldRvaRow(new SegmentReference(myData), 0);
