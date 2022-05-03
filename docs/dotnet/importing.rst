.. _dotnet-reference-importing:

Reference Importing
===================

.NET modules use entries in the TypeRef or MemberRef tables to reference types or members from external assemblies. Importing references into the current module therefore form a key role when creating new- or modifying existing .NET modules. When a member is not imported into the current module, a ``MemberNotImportedException`` will be thrown when you are trying to create a PE image or write the module to the disk.

AsmResolver provides the ``ReferenceImporter`` class that does most of the heavy lifting. Obtaining an instance of ``ReferenceImporter`` can be done in two ways.

Either instantiate one yourself:

.. code-block:: csharp

    ModuleDefinition module = ...
    var importer = new ReferenceImporter(module);

Or obtain the default instance that comes with every ``ModuleDefinition`` object. This avoids allocating new reference importers every time.

.. code-block:: csharp
    
    ModuleDefinition module = ...
    var importer = module.DefaultImporter;


The example snippets that will follow in this articule assume that there is such a ``ReferenceImporter`` object instantiated using either of these two methods, and is stored in an ``importer`` variable.


Importing metadata members
--------------------------

Metadata members from external modules can be imported using the ``ReferenceImporter`` class using one of the following members:

+---------------------------+------------------------+----------------------+
| Member type to import     | Method to use          | Result type          |
+===========================+========================+======================+
| ``IResolutionScope``      | ``ImportScope``        | ``IResolutionScope`` |
+---------------------------+------------------------+----------------------+
| ``AssemblyReference``     | ``ImportScope``        | ``IResolutionScope`` |
+---------------------------+------------------------+----------------------+
| ``AssemblyDefinition``    | ``ImportScope``        | ``IResolutionScope`` |
+---------------------------+------------------------+----------------------+
| ``ModuleReference``       | ``ImportScope``        | ``IResolutionScope`` |
+---------------------------+------------------------+----------------------+
| ``ITypeDefOrRef``         | ``ImportType``         | ``ITypeDefOrRef``    |
+---------------------------+------------------------+----------------------+
| ``TypeDefinition``        | ``ImportType``         | ``ITypeDefOrRef``    |
+---------------------------+------------------------+----------------------+
| ``TypeReference``         | ``ImportType``         | ``ITypeDefOrRef``    |
+---------------------------+------------------------+----------------------+
| ``TypeSpecification``     | ``ImportType``         | ``ITypeDefOrRef``    |
+---------------------------+------------------------+----------------------+
| ``IMethodDefOrRef``       | ``ImportMethod``       | ``IMethodDefOrRef``  |
+---------------------------+------------------------+----------------------+
| ``MethodDefinition``      | ``ImportMethod``       | ``IMethodDefOrRef``  |
+---------------------------+------------------------+----------------------+
| ``MethodSpecification``   | ``ImportMethod``       | ``IMethodDefOrRef``  |
+---------------------------+------------------------+----------------------+
| ``IFieldDescriptor``      | ``ImportField``        | ``IFieldDescriptor`` |
+---------------------------+------------------------+----------------------+
| ``FieldDefinition``       | ``ImportField``        | ``IFieldDescriptor`` |
+---------------------------+------------------------+----------------------+

Below an example of how to import a type definition called ``SomeType``:

.. code-block:: csharp

    ModuleDefinition externalModule = ModuleDefinition.FromFile(...);
    TypeDefinition typeToImport = externalModule.TopLevelTypes.First(t => t.Name == "SomeType");

    ITypeDefOrRef importedType = importer.ImportType(typeToImport);


Importing type signatures
-------------------------

Type signatures can also be imported using the ``ReferenceImporter`` class, but these should be imported using the ``ImportTypeSignature`` method instead.

.. note:: 

    If a corlib type signature is imported, the appropriate type from the ``CorLibTypeFactory`` of the target module will be selected, regardless of whether CorLib versions are compatible with each other.


Importing using reflection
--------------------------

Types and members can also be imported by passing on an instance of various ``System.Reflection`` classes.

+---------------------------+------------------------+----------------------+
| Member type to import     | Method to use          | Result type          |
+===========================+========================+======================+
| ``Type``                  | ``ImportType``         | ``ITypeDefOrRef``    |
+---------------------------+------------------------+----------------------+
| ``Type``                  | ``ImportTypeSignature``| ``TypeSignature``    |
+---------------------------+------------------------+----------------------+
| ``MethodBase``            | ``ImportMethod``       | ``IMethodDefOrRef``  |
+---------------------------+------------------------+----------------------+
| ``MethodInfo``            | ``ImportMethod``       | ``IMethodDefOrRef``  |
+---------------------------+------------------------+----------------------+
| ``ConstructorInfo``       | ``ImportMethod``       | ``IMethodDefOrRef``  |
+---------------------------+------------------------+----------------------+
| ``FieldInfo``             | ``ImportScope``        | ``MemberReference``  |
+---------------------------+------------------------+----------------------+


There is limited support for importing compound types. Types that can be imported through reflection include:

- Pointer types.
- By-reference types.
- Array types:
    - If an array contains only one dimension, a ``SzArrayTypeSignature`` is returned. Otherwise a ``ArrayTypeSignature`` is created.
- Generic parameters.
- Generic type instantiations.

Instantiations of generic methods are supported.


.. _dotnet-importer-common-caveats:

Common Caveats using the Importer 
---------------------------------

Caching and reuse of instances
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

The default implementation of ``ReferenceImporter`` does not maintain a cache. Each call to any of the import methods will result in a new instance of the imported member.  The exception to this rule is when the member passed onto the importer is defined in the module the importer is targeting itself, or was already a reference imported by an importer into the target module. In both of these cases, the same instance of this member definition or reference will be returned instead.

Importing cross-framework versions
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

The ``ReferenceImporter`` does not support importing across different versions of the target framework. Members are being imported as-is, and are not automatically adjusted to conform with other versions of a library. 

As a result, trying to import from for example a library part of the .NET Framework into a module targeting .NET Core or vice versa has a high chance of producing an invalid .NET binary that cannot be executed by the runtime. For example, attempting to import a reference to ``[System.Runtime] System.DateTime`` into a module targeting .NET Framework will result in a new reference targeting a .NET Core library (``System.Runtime``) as opposed to the appropriate .NET Framework library (``mscorlib``). 

This is a common mistake when trying to import using metadata provided by ``System.Reflection``. For example, if the host application that uses AsmResolver targets .NET Core but the input file is targeting .NET Framework, then you will run in the exact issue described in the above.

.. code-block:: csharp

    var reference = importer.ImportType(typeof(DateTime));

    // `reference` will target `[mscorlib] System.DateTime` when running on .NET Framework, and `[System.Runtime] System.DateTime` when running on .NET Core.


Therefore, always make sure you are importing from a .NET module that is compatible with the target .NET module.