Type Memory Layout
==================

Sometimes it is useful to know details about the memory layout of a type at runtime. Knowing the memory layout of a type can help in various processes, including:

- Getting the size of a type at runtime.

- Calculating field pointer offsets within a type.

AsmResolver provides an API to statically infer information about the memory layout of any given blittable type. It supports structures marked as ``SequentialLayout`` and ``ExplicitLayout``, as well as field alignments and custom field offsets.

To get access to the API, you must include the following namespace:

.. code-block:: csharp
    
    using AsmResolver.DotNet.Memory;

Obtaining the type layout
-------------------------

To get the memory layout of any ``ITypeDescriptor``, use the following extension method:

.. code-block:: csharp

    ITypeDescriptor type = ...    
    TypeMemoryLayout typeLayout = type.GetImpliedMemoryLayout(is32Bit: false);

.. warning::
    
    Only value types that are marked with the ``SequentialLayout`` or ``ExplicitLayout`` structure layout are fully supported. If ``AutoLayout`` was provided, a sequential layout is assumed. This might not be the case for all implementations of the CLR.


.. warning::

    If the type contains a cyclic dependency (e.g. a field with field type equal to its enclosing class), this method will throw an instance of the ``CyclicStructureException`` class.


The size of a type
--------------------------

After the memory layout is inferred, you can query the ``Size`` property to obtain the total size in bytes of the type.

.. code-block:: csharp

    uint size = typeLayout.Size;


Getting field offsets
---------------------

The ``TypeMemoryLayout`` provides an indexer property that takes an instance of ``FieldDefinition``, and returns an instance of ``FieldMemoryLayout``. 

.. code-block:: csharp

    FieldDefinition field = ...
    FieldMemoryLayout fieldLayout = typeLayout[field];

This class contains the offset of the queried field within the type:

.. code-block:: csharp

    uint offset = fieldLayout.Offset;

It also provides another instance of ``TypeMemoryLayout`` to get the layout of the contents of the field:

.. code-block:: csharp

    TypeMemoryLayout contentsLayout = fieldLayout.ContentsLayout;

This can be used to recursively find fields and their offsets.

.. note::
    
    A ``TypeMemoryLayout`` describing the layout of type ``T`` might be different from a ``TypeMemoryLayout`` that was associated to a field, even if this field has field type ``T`` as well. This is due to the fact that the CLR might layout nested fields differently when a structure defines a field with a compound field type.


Getting fields by offset
------------------------

It is also possible to turn an offset (relative to the start of the type) to the field definition that is stored at that offset. This is done by using the ``TryGetFieldAtOffset`` method.

.. code-block:: csharp

    uint offset = ...
    if (typeLayout.TryGetFieldAtOffset(offset, out var fieldLayout))
    {
        // There is a field defined at this offset.
    }

Sometimes, offsets within a structure refer to a field within a nested field. For example, consider the following sample code:

.. code-block:: csharp

    [StructLayout(LayoutKind.Sequential, Size = 17)]
    public struct Struct1
    {
        public int Dummy1;
    }

    [StructLayout(LayoutKind.Sequential, Size = 23, Pack = 2)]
    public struct Struct2
    {
        public Struct1 Nest1;
    }

    [StructLayout(LayoutKind.Sequential, Size = 87, Pack = 64)]
    public struct Struct3
    {
        public Struct1 Nest1;

        public Struct2 Nest2;
    }

To get a collection of fields to access to reach a certain offset within the type, use the ``TryGetFieldPath`` method. This method will return ``true`` if the offset refers to the beginning of a field, and ``false`` otherwise.

.. code-block:: csharp

    var struct3Definition = (TypeDefinition) Module.LookupMember(
        typeof(Struct3).MetadataToken);
    var struct3Layout = struct3Definition.GetImpliedMemoryLayout(false);

    uint offset = 20;
    bool isStartOfField = layout.TryGetFieldPath(offset, out var path);

    // This results in:
    //  - isStartOfField: true.
    //  - path: {Struct3::Nest2, Struct2::Nest1, Struct1::Dummy1}.