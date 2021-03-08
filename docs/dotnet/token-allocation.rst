Metadata Token Allocation
=========================

A lot of models in a .NET module are assigned a unique metadata token. This token can be accessed through the ``IMetadataMember.MetadataToken`` property. The exception to this rule is newly created metadata members. These newly created members are assigned the zero token (a token with RID = 0). Upon building a module, these tokens will be replaced with their actual tokens.

Custom Token Allocation
-----------------------

Some usecase of AsmResolver will depend on the knowledge of tokens of newly created members prior to serializing the module. Therefore, AsmResolver provides the ``TokenAllocator`` class, which allows for assigning new tokens to members pre-emptively. If a module is then written to the disk with the ``MetadataFlags.PreserveTableIndices`` flags set (see Advanced PE Image Building for more information on how that is done), this token will be preserved in the final image.

The token allocator for a particular module can be accessed through the ``ModuleDefinition.TokenAllocator`` property:

.. code-block:: csharp

    var allocator = module.TokenAllocator;

Using the allocator, it is possible to assign metadata tokens to newly created members. This is done using the ``AssignNextAvailableToken`` method:

.. code-block:: csharp

    var field = new FieldDefinition(...);
    someType.Fields.Add(field);

    allocator.AssignNextAvailableToken(field);

    // field.MetadataToken is now non-zero.

.. warning::

    Only members with a zero Metadata Token can be assigned a new metadata token. If a metadata member with a non-zero MetadataToken was passed as an argument, this method will throw an ``ArgumentException``.