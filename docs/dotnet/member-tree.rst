The Member Tree
===============

Assemblies and modules
----------------------

The root of every .NET assembly is represented by the ``AssemblyDefinition`` class. This class exposes basic information such as name, version and public key token, but also a collection of all modules that are defined in the assembly. Modules are represented by the ``ModuleDefinition`` class.

Below an example that enumerates all modules defined in an assembly.

.. code-block:: csharp

    var assembly = AssemblyDefinition.FromFile(...);
    foreach (var module in assembly.Modules)
        Console.WriteLine(module.Name);

Most .NET assemblies only have one module. This main module is also known as the manifest module, and can be accessed directly through the ``AssemblyDefinition.ManifestModule`` property.


Obtaining types in a module
---------------------------

Types are represented by the ``TypeDefinition`` class. To get the types defined in a module, use the ``ModuleDefinition.TopLevelTypes`` property. A top level types is any non-nested type. Nested types are exposed through the ``TypeDefinition.NestedTypes``. Alternatively, to get all types, including nested types, it is possible to call the ``ModuleDefinition.GetAllTypes`` method instead.

Below, an example program that iterates through all types recursively and prints them:

.. code-block:: csharp

    public const int IndentationWidth = 3;

    private static void Main(string[] args)
    {
        var module = ModuleDefinition.FromFile(...);
        DumpTypes(module.TopLevelTypes);
    }

    private static void DumpTypes(IEnumerable<TypeDefinition> types, int indentationLevel = 0)
    {
        string indentation = new string(' ', indentationLevel * IndentationWidth);
        foreach (var type in types)
        {
            // Print the name of the current type.
            Console.WriteLine("{0}- {1} : {2:X8}", indentation, type.Name, type.MetadataToken.ToInt32());

            // Dump any nested types.
            DumpTypes(type.NestedTypes, indentationLevel + 1);
        }
    }

.. _dotnet-obtaining-methods-and-fields:

Obtaining methods and fields
----------------------------

The ``TypeDefinition`` class exposes collections of methods and fields that the type defines:

.. code-block:: csharp

    foreach (var method in type.Methods)
        Console.WriteLine("{0} : {1:X8}", method.Name, method.MetadataToken.ToInt32());


.. code-block:: csharp

    foreach (var field in type.Fields)
        Console.WriteLine("{0} : {1:X8}", field.Name, field.MetadataToken.ToInt32());

Methods and fields have a ``Signature`` property, that contain the return and parameter types, or the field type respectively.

.. code-block:: csharp

    MethodDefinition method = ...
    Console.WriteLine("Return type: " + method.Signature.ReturnType);
    Console.WriteLine("Parameter types: " + string.Join(", ", method.Signature.ParameterTypes));


.. code-block:: csharp

    FieldDefinition field = ...
    Console.WriteLine("Field type: " + field.Signature.FieldType);


However, for reading parameters from a method definition, it is preferred to use the ``Parameters`` property instead of the ``ParameterTypes`` property stored in the signature. This is because the ``Parameters`` property automatically binds the types to the parameter definitions that are associated to these parameter types. This provides additional information, such as the name of the parameter:

.. code-block:: csharp

    foreach (var parameter in method.Parameters)
        Console.WriteLine($"{parameter.Name} : {parameter.ParameterType}");


Obtaining properties and events
-------------------------------

Obtaining properties and events is similar to obtaining methods and fields; ``TypeDefinition`` exposes them in a list as well:

.. code-block:: csharp

    foreach (var @event in type.Events)
        Console.WriteLine("{0} : {1:X8}", @event.Name, @event.MetadataToken.ToInt32());

.. code-block:: csharp

    foreach (var property in type.Properties)
        Console.WriteLine("{0} : {1:X8}", property.Name, property.MetadataToken.ToInt32());


Properties and events have methods associated to them. These are accessible through the ``Semantics`` property:

.. code-block:: csharp

    foreach (MethodSemantics semantic in property.Semantics)
    {
        Console.WriteLine("{0} {1} : {2:X8}", semantic.Attributes, semantic.Method.Name,
            semantic.MetadataToken.ToInt32());
    }
