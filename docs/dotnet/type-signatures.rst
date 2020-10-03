Type Signatures
===============

Type signatures represent references to types within a blob signature. They are not directly associated with a metadata token, but can reference types defined in one of the metadata tables.

All relevant classes in this document can be found in the following namespaces:

.. code-block:: csharp

    using AsmResolver.DotNet.Signatures;
    using AsmResolver.DotNet.Signatures.Types;


Overview of all Type Signatures 
-------------------------------

Basic leaf type signatures: 

+----------------------------------+----------------------------------------------------------------------+
| Type signature name              | Example                                                              |
+==================================+======================================================================+
| ``CorLibTypeSignature``          | ``int32`` (``System.Int32``)                                         |
+----------------------------------+----------------------------------------------------------------------+
| ``TypeDefOrRefSignature``        | ``System.IO.Stream``, ``System.Drawing.Point``                       |
+----------------------------------+----------------------------------------------------------------------+
| ``GenericInstanceTypeSignature`` | ``System.Collections.Generic.IList`1<System.Int32>``                 |
+----------------------------------+----------------------------------------------------------------------+
| ``SentinelTypeSignature``        | (Used as a delimeter for vararg method signatures)                   |
+----------------------------------+----------------------------------------------------------------------+

Decorator type signatures:

+----------------------------------+----------------------------------------------------------------------+
| Type signature name              | Example                                                              |
+==================================+======================================================================+
| ``SzArrayTypeSignature``         | ``System.Int32[]``                                                   |
+----------------------------------+----------------------------------------------------------------------+
| ``ArrayTypeSignature``           | ``System.Int32[0.., 0..]``                                           |
+----------------------------------+----------------------------------------------------------------------+
| ``ByReferenceTypeSignature``     | ``System.Int32&``                                                    |
+----------------------------------+----------------------------------------------------------------------+
| ``PointerTypeSignature``         | ``System.Int32*``                                                    |
+----------------------------------+----------------------------------------------------------------------+
| ``GenericParameterSignature``    | ``!0``, ``!!0``                                                      |
+----------------------------------+----------------------------------------------------------------------+
| ``CustomModifierTypeSignature``  | ``System.Int32 modreq (System.Runtime.CompilerServices.IsVolatile)`` |
+----------------------------------+----------------------------------------------------------------------+
| ``BoxedTypeSignature``           | (Boxes a value type signature)                                       |
+----------------------------------+----------------------------------------------------------------------+
| ``PinnedTypeSignature``          | (Pins the value of a local variable in memory)                       |
+----------------------------------+----------------------------------------------------------------------+


Basic Element Types
-------------------

The CLR defines a set of primitive types (such as ``System.Int32``, ``System.Object``) as basic element types that can be referenced using a single byte, rather than the fully qualified name. These are represented using the ``CorLibTypeSignature`` class.

Every ``ModuleDefinition`` defines a property called ``CorLibTypeFactory``, which exposes reusable instances of all corlib type signatures:

.. code-block:: csharp

    ModuleDefinition module = ...
    TypeSignature int32Type = module.CorLibTypeFactory.Int32;

Corlib type signatures can also be looked up by their element type, by their full name, or by converting a type reference to a corlib type signature.

.. code-block:: csharp

    int32Type = module.CorLibTypeFactory.FromElementType(ElementType.I4);
    int32Type = module.CorLibTypeFactory.FromName("System", "Int32");

    var int32TypeRef = new TypeReference(corlibScope, "System", "Int32");
    int32Type = module.CorLibTypeFactory.FromType(int32TypeRef);

If an invalid element type, name or type descriptor is passed on, these methods return ``null``.

TypeDefOrRefSignature
---------------------

The ``TypeDefOrRefSignature`` class is used to reference types in either the ``TypeDef`` or ``TypeRef`` (and sometimes ``TypeSpec``) metadata table. 

.. code-block:: csharp

    TypeReference streamTypeRef = new TypeReference(corlibScope, "System.IO", "Stream");
    TypeSignature streamTypeSig = new TypeDefOrRefSignature(streamTypeRef);

g
.. warning::

    While it is technically possible to reference a basic type such as ``System.Int32`` as a ``TypeDefOrRefSignature``, it renders the .NET module invalid by most implementations of the CLR. Always use the ``CorLibTypeSignature`` to reference basic types within your blob signatures.


GenericInstanceTypeSignature
----------------------------

The ``GenericInstanceTypeSignature`` class is used to instantiate generic types with type arguments:

.. code-block:: csharp

    var listTypeRef = new TypeReference(corlibScope, "System.Collections.Generic", "List`1");
    
    var listOfString = new GenericInstanceTypeSignature(listTypeRef, 
        isValueType: false, 
        typeArguments: new[] { module.CorLibTypeFactory.String });

    // listOfString now contains a reference to List<string>.


Shortcuts
---------

To quickly transform any ``ITypeDescriptor`` into a ``TypeSignature``, it is possible to use the ``.ToTypeSignature()`` method on any ``ITypeDescriptor``. For ``TypeReference`` s, this will also check whether the object is referencing a basic type and return the appropriate ``CorLibTypeSignature`` instead.

.. code-block:: csharp

    TypeReference streamTypeRef = new TypeReference(corlibScope, "System.IO", "Stream");
    TypeSignature streamTypeSig = streamTypeRef.ToTypeSignature();


Likewise, a ``TypeSignature`` can also be converted back to a ``ITypeDefOrRef``, which can be referenced using a metadata token, using the ``TypeSignature.ToTypeDefOrRef()`` method.

Decorating type signatures
--------------------------

Type signatures can be annotated with extra properties, such as an array or pointer specifier.

Below an example of how to create a type signature referencing ``System.Int32[]``:

.. code-block:: csharp

    var arrayTypeSig = new SzArrayTypeSignature(module.CorLibTypeFactory.Int32);

Traversing type signature annotations can be done by accessing the ``BaseType`` property of ``TypeSignature``.

.. code-block:: csharp

    var arrayElementType = arrayTypeSig.BaseType; // returns System.Int32

Adding decorations to types can also be done through shortcut methods that follow the ``MakeXXX`` naming scheme:

.. code-block:: csharp

    var arrayTypeSig = module.CorLibTypeFactory.Int32.MakeSzArrayType();

Below an overview of all factory shortcut methods:

+-------------------------------------------------------------------+------------------------------------------------------------------------------------------------------------------+
| Factory method                                                    | Description                                                                                                      |
+===================================================================+==================================================================================================================+
| ``MakeArrayType(int dimensionCount)``                             | Wraps the type in a new ``ArrayTypeSignature`` with ``dimensionCount`` zero based dimensions with no upperbound. |
+-------------------------------------------------------------------+------------------------------------------------------------------------------------------------------------------+
| ``MakeArrayType(ArrayDimension[] dimensinos)``                    | Wraps the type in a new ``ArrayTypeSignature`` with ``dimensions`` set as dimensions                             |
+-------------------------------------------------------------------+------------------------------------------------------------------------------------------------------------------+
| ``MakeByReferenceType()``                                         | Wraps the type in a new ``ByReferenceTypeSignature``                                                             |
+-------------------------------------------------------------------+------------------------------------------------------------------------------------------------------------------+
| ``MakeModifierType(ITypeDefOrRef modifierType, bool isRequired)`` | Wraps the type in a new ``CustomModifierTypeSignature`` with the specified modifier type.                        |
+-------------------------------------------------------------------+------------------------------------------------------------------------------------------------------------------+
| ``MakePinnedType()``                                              | Wraps the type in a new ``PinnedTypeSignature``                                                                  |
+-------------------------------------------------------------------+------------------------------------------------------------------------------------------------------------------+
| ``MakePointerType()``                                             | Wraps the type in a new ``PointerTypeSignature``                                                                 |
+-------------------------------------------------------------------+------------------------------------------------------------------------------------------------------------------+
| ``MakeSzArrayType()``                                             | Wraps the type in a new ``SzArrayTypeSignature``                                                                 |
+-------------------------------------------------------------------+------------------------------------------------------------------------------------------------------------------+
| ``MakeGenericInstanceType(TypeSignature[] typeArguments)``        | Wraps the type in a new ``GenericInstanceTypeSignature`` with the provided type arguments.                       |
+-------------------------------------------------------------------+------------------------------------------------------------------------------------------------------------------+
