Reference Importing
===================

.NET modules use entries in the TypeRef or MemberRef tables to reference types or members from external assemblies. Importing references into the current module therefore form a key role when creating new- or modifying existing .NET modules. When a member is not imported into the current module, a ``MemberNotImportedException`` will be thrown when you are trying to create a PE image or write the module to the disk.

AsmResolver provides the ``ReferenceImporter`` class that does most of the heavy lifting for you.

All samples in this document assume there is an instance of ``ReferenceImporter`` created using the following code:

.. code-block:: csharp

    var importer = new ReferenceImporter(module);


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

Some key points:

- The reference importer does no caching. Each call to any of the import methods will result in a new instance of the imported member.

- If the member that is passsed onto the reference importer is a definition the current module or was already imported into the module, the same instance of this member definition or reference will be returned.


Below an example of how to import a type definition called ``SomeType``:

.. code-block:: csharp

    ModuleDefinition externalModule = ModuleDefinition.FromFile(...);
    TypeDefinition typeToImport = externalModule.TopLevelTypes.First(t => t.Name == "SomeType");

    ITypeDefOrRef importedType = importer.ImportType(typeToImport);



Importing type signatures
-------------------------

Type signatures can also be imported using the ``ReferenceImporter`` class, but these should be imported using the ``ImportTypeSignature`` method instead.

Some key points:

- The reference importer does no caching. Each call to any of the import methods will result in a new instance of the imported member.

- If the type signature passed on was already an imported type signature, the same instance will be returned.

- If a corlib type signature is imported, the appropriate type from the ``CorLibTypeFactory`` of the target module will be selected, regardless of whether CorLib versions are compatible with each other.


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

Some key points:

- The reference importer does no caching. Each call to any of the import methods will result in a new instance of the imported member.

- There is limited support for importing compound types. Types that can be imported through reflection include:

    - Pointer types
    
    - By reference types 

    - Array types: If an array contains only one dimension, a ``SzArrayTypeSignature`` is returned. Otherwise a ``ArrayTypeSignature`` is created.

    - Generic parameters

    - Generic type instantiations

- Instantiations of generic methods are supported.