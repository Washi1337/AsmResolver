Methods
===============

Non-Generic Methods on Generic Types
------------------------------------------

This section covers referencing methods such as ``System.Collections.Generic.List`1<System.Int32>.Add``. They can be referenced with the ``MemberReference`` class.

.. code-block:: csharp

    var corlibScope = moduleDefinition.CorLibTypeFactory.CorLibScope;

    var listTypeReference = new TypeReference(corlibScope, "System.Collections.Generic", "List`1");
    
    var listOfInt32 = listTypeReference.MakeGenericInstanceType(moduleDefinition.CorLibTypeFactory.Int32);

    var addMethodDefinition = listTypeReference.Resolve().Methods.Single(m => m.Name == "Add" && m.Parameters.Count == 1);

    var reference = new MemberReference(listOfInt32.ToTypeDefOrRef(), addMethodDefinition.Name, addMethodDefinition.Signature);

Generic Methods on Non-Generic Types
------------------------------------------

This section covers referencing methods such as ``System.Array.Empty<System.Int32>``. They can be referenced with the ``MethodSpecification`` class via the ``MakeGenericInstanceMethod`` extension method on ``IMethodDefOrRef``.

.. code-block:: csharp

    var corlibScope = moduleDefinition.CorLibTypeFactory.CorLibScope;

    var arrayTypeReference = new TypeReference(corlibScope, "System", "Array");

    var emptyMethodDefinition = arrayTypeReference.Resolve().Methods.Single(m => m.Name == "Empty" && m.Parameters.Count == 0);

    var reference = emptyMethodDefinition.MakeGenericInstanceMethod(moduleDefinition.CorLibTypeFactory.Int32);
