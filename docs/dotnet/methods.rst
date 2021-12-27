Methods
===============

Non-Generic Methods on Generic Types
------------------------------------------

This section covers referencing methods such as ``System.Collections.Generic.List`1<System.Int32>.Add``. They can be referenced with the ``MemberReference`` class.

.. code-block:: csharp

    var listTypeRef = new TypeReference(corlibScope, "System.Collections.Generic", "List`1");
    
    var listOfInt32 = new GenericInstanceTypeSignature(listTypeRef, 
        isValueType: false, 
        typeArguments: new[] { module.CorLibTypeFactory.Int32 });

    var addMethodDefinition = listTypeRef.Resolve().Methods.Single(m => m.Name == "Add" && m.Parameters.Count == 1);

    var reference = new MemberReference(listOfInt32.ToTypeDefOrRef(), addMethodDefinition.Name, addMethodDefinition.Signature);

Generic Methods on Non-Generic Types
------------------------------------------

This section covers referencing methods such as ``System.Array.Empty<System.Int32>``. They can be referenced with the ``MethodSpecification`` class.

.. code-block:: csharp

    var arrayRef = new TypeReference(corlibScope, "System", "Array");

    var emptyMethodDefinition = arrayRef.Resolve().Methods.Single(m => m.Name == "Empty" && m.Parameters.Count == 0);

    var genericInstanceMethodSignature = new GenericInstanceMethodSignature(module.CorLibTypeFactory.Int32);

    var reference = new MethodSpecification(emptyMethodDefinition, genericInstanceMethodSignature);
