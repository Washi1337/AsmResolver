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
| ``FunctionPointerTypeSignature`` | ``method void *(int32, int64)``                                      |
+----------------------------------+----------------------------------------------------------------------+
| ``GenericParameterSignature``    | ``!0``, ``!!0``                                                      |
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


Alternatively, ``CreateTypeReference`` can be used on any ``IResolutionScope``:

.. code-block:: csharp

    var streamTypeSig = corlibScope.CreateTypeReference("System.IO", "Stream");


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


Alternatively, a generic instance can also be generated via the ``MakeGenericType`` fluent syntax method:

.. code-block:: csharp

    var listOfString = corlibScope
        .CreateTypeReference("System.Collections.Generic", "List`1")
        .MakeGenericInstanceType(module.CorLibTypeFactory.String);

    // listOfString now contains a reference to List<string>.


FunctionPointerTypeSignature
----------------------------

Function pointer signatures are strongly-typed pointer types used to describe addresses to functions or methods. In AsmResolver, they are represented using a ``MethodSignature``:

.. code-block:: csharp

    var factory = module.CorLibTypeFactory;
    var signature = MethodSignature.CreateStatic(
        factory.Void,
        factory.Int32,
        factory.Int32);

    var type = new FunctionPointerTypeSignature(signature);

    // type now contains a reference to `method void *(int32, int32)`.


Alternatively, a function pointer signature can also be generated via the ``MakeFunctionPointerType`` fluent syntax method:

.. code-block:: csharp

    var factory = module.CorLibTypeFactory;
    var type = MethodSignature.CreateStatic(
            factory.Void,
            factory.Int32,
            factory.Int32)
        .MakeFunctionPointerType();

    // type now contains a reference to `method void *(int32, int32)`.



Shortcuts
---------

To quickly transform any ``ITypeDescriptor`` into a ``TypeSignature``, it is possible to use the ``.ToTypeSignature()`` method on any ``ITypeDescriptor``. For ``TypeReference`` s, this will also check whether the object is referencing a basic type and return the appropriate ``CorLibTypeSignature`` instead.

.. code-block:: csharp

    var streamTypeRef = new TypeReference(corlibScope, "System.IO", "Stream");
    var streamTypeSig = streamTypeRef.ToTypeSignature();


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



Comparing Type Signatures
-------------------------

Type signatures can be tested for semantic equivalence using the ``SignatureComparer`` class. 
Most use-cases of this class will not require any customization. 
In these cases, the default ``SignatureComparer`` can be used:

.. code-block:: csharp

    var comparer = SignatureComparer.Default;


However, if you wish to configure the comparer (e.g., for relaxing some of the declaring assembly version comparison rules), it is possible to create a new instance instead:

.. code-block:: csharp

    var comparer = new SignatureComparer(SignatureComparisonFlags.AllowNewerVersions);


Once a comparer is obtained, we can test for type equality using any of the overloaded ``Equals`` methods:

.. code-block:: csharp

    TypeSignature type1 = ...;
    TypeSignature type2 = ...;

    if (comparer.Equals(type1, type2)) 
    {
        // type1 and type2 are semantically equivalent.
    }


The ``SignatureComparer`` class implements various instances of the ``IEqualityComparer<T>`` interface, and as such, it can be used as a comparer for dictionaries and related types:

.. code-block:: csharp

    var dictionary = new Dictionary<TypeSignature, TValue>(comparer);


.. note:: 

    The ``SignatureComparer`` class also implements equality comparers for other kinds of metadata, such as field and method descriptors and their signatures.


In some cases, however, exact type equivalence is too strict of a test.
Since .NET facilitates an object oriented environment, many types will inherit or derive from each other, making it difficult to pinpoint exactly which types we would need to compare to test whether two types are compatible with each other.  

Section I.8.7 of the ECMA-335 specification defines a set of rules that dictate whether values of a certain type are compatible with or assignable to variables of another type. 
These rules are implemented in AsmResolver using the ``IsCompatibleWith`` and ``IsAssignableTo`` methods:

.. code-block:: csharp

    if (type1.IsCompatibleWith(type2)) 
    {
        // type1 can be converted to type2.
    }


.. code-block:: csharp

    if (type1.IsAssignableTo(type2)) 
    {
        // Values of type1 can be assigned to variables of type2.
    }
