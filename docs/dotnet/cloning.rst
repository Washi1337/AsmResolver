Member Cloning
==============

When processing a .NET module, it often involves injecting additional code. Even though all models representing .NET metadata and CIL code are mutable, it might be very time consuming and error-prone to manually import and inject metadata members and/or CIL code into the target module.

To help developers in injecting existing code into a module, ``AsmResolver.DotNet`` comes with a feature that involves cloning metadata members from one module and copying it over to another. All relevant classes are in the ``AsmResolver.DotNet.Cloning`` namespace:

.. code-block:: csharp

    using AsmResolver.DotNet.Cloning;


The MemberCloner class
----------------------

The ``MemberCloner`` is the root object responsible for cloning members in a .NET module, and importing them into another. 

In the snippet below, we define a new ``MemberCloner`` that is able to clone and import members into the module ``destinationModule:``.

.. code-block:: csharp

    ModuleDefinition destinationModule = ...
    MemberCloner cloner = new MemberCloner(destinationModule);

In the remaining sections of this article, we assume that the ``MemberCloner`` is initialized using the code above.

.. warning::

    The ``MemberCloner`` heavily depends on the ``ReferenceImporter`` class for copying references into the destination module. This class has some limitations, in particular on importing / cloning from modules targeting different framework versions. See :ref:`dotnet-importer-common-caveats` for more information.


Include members to clone
------------------------

The general idea of the ``MemberCloner`` is to first provide all the members to be cloned, and then clone everything all in one go. The reason why it is done like this, is to allow the ``MemberCloner`` to fix up any cross references to members within the to-be-cloned metadata and CIL code.

For the sake of the example, we assume that the following two classes are to be injected in ``destinationModule``:

.. code-block:: csharp

    public class Rectangle
    {
        public Rectangle(Vector2 location, Vector2 size) 
        {
            Location = location;
            Size = size;
        }

        public Vector2 Location { get; set; }
        public Vector2 Size { get; set; }

        public Vector2 GetCenter() => new Vector2(Location.X + Size.X / 2, Location.Y + Size.Y / 2);
    }

    public class Vector2
    {
        public Vector2(int x, int y) 
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }
    }

The first thing we then should do, is find the type definitions that correspond to these classes:

.. code-block:: csharp

    var sourceModule = ModuleDefinition.FromFile(...);
    var rectangleType = sourceModule.TopLevelTypes.First(t => t.Name == "Rectangle");
    var vectorType = sourceModule.TopLevelTypes.First(t => t.Name == "Vector2");

Alternatively, if the source assembly is loaded by the CLR, we also can look up the members by metadata token.

.. code-block:: csharp

    var sourceModule = ModuleDefinition.FromFile(typeof(Rectangle).Assembly.Location);
    var rectangleType = (TypeDefinition) sourceModule.LookupMember(typeof(Rectangle).MetadataToken);
    var vectorType = (TypeDefinition) sourceModule.LookupMember(typeof(Vector2).MetadataToken);


We can then use ``MemberCloner.Include`` to include the types in the cloning procedure:

.. code-block:: csharp

    cloner.Include(rectangleType, recursive: true);
    cloner.Include(vectorType, recursive: true);

The ``recursive`` parameter indicates whether all members and nested types need to be included as well.

``Include`` returns the same ``MemberCloner`` instance. It is therefore also possible to create a long method chain of members to include in the cloning process.

.. code-block:: csharp

    cloner
        .Include(rectangleType, recursive: true)
        .Include(vectorType, recursive: true);

Cloning individual methods, fields, properties and/or events is also supported. This can be done by including the corresponding ``MethodDefinition``, ``FieldDefinition``, ``PropertyDefinition`` and/or ``EventDefinition`` instead.


Cloning the included members 
----------------------------

When all members are included, it is possible to call ``MemberCloner.Clone`` to clone them all in one go. 

.. code-block:: csharp

    var result = cloner.Clone();

The ``MemberCloner`` will automatically resolve any cross references between types, fields and methods that are included in the cloning process. 

For instance, going with the example in the previous section, if both the ``Rectangle`` as well as the ``Vector2`` classes are included, any reference in ``Rectangle`` to ``Vector2`` will be replaced with a reference to the cloned ``Vector2``.  If not all members are included, the ``MemberCloner`` will assume that these are references to external libraries, and will use the ``ReferenceImporter`` to construct references to these members instead.


Injecting the cloned members 
----------------------------

After cloning, we obtain a ``MemberCloneResult``, which contains a register of all members cloned by the member cloner.

- ``OriginalMembers``: The collection containing all original members.
- ``ClonedMembers``: The collection containing all cloned members.
- ``ClonedTopLevelTypes``: A subset of ``ClonedMembers``, containing all cloned top-level types.

Original members can be mapped to their cloned counterpart, using the ``GetClonedMember`` method:

.. code-block:: csharp

    var clonedRectangleType = result.GetClonedMember(rectangleType);

Alternatively, we can get all cloned top-level types.

.. code-block:: csharp

    var clonedTypes = result.ClonedTopLevelTypes;

It is important to note that the ``MemberCloner`` class itself does not inject any of the cloned members. To inject the cloned types, we can for instance add them to the ``ModuleDefinition.TopLevelTypes`` collection:

.. code-block:: csharp

    foreach (var clonedType in clonedTypes)
        destinationModule.TopLevelTypes.Add(clonedType);