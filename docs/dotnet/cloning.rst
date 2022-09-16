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
    var cloner = new MemberCloner(destinationModule);

In the remaining sections of this article, we assume that the ``MemberCloner`` is initialized using the code above.


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


Custom reference importers
--------------------------

The ``MemberCloner`` heavily depends on the ``CloneContextAwareReferenceImporter`` class for copying references into the destination module. This class is derived from ``ReferenceImporter``, which has some limitations. In particular, limitations arise when cloning from modules targeting different framework versions, or when trying to reference members that may already exist in the target module (e.g., when dealing with ``NullableAttribute`` annotated metadata).

To account for situations like these, the cloner allows for specifying custom reference importer instances. By deriving from the ``CloneContextAwareReferenceImporter`` class, and overriding methods such as ``ImportMethod``, we can reroute specific member references to the appropriate metadata if needed. Below is an example of a basic implementation of an importer that attempts to map method references from the ``System.Runtime.CompilerServices`` namespace to definitions that are already present in the target module.

.. code-block:: csharp

    public class MyImporter : CloneContextAwareReferenceImporter
    {
        private static readonly SignatureComparer Comparer = new();

        public MyImporter(MemberCloneContext context)
            : base(context)
        {
        }

        public override IMethodDefOrRef ImportMethod(IMethodDefOrRef method)
        {
            // Check if the method is from a type defined in the System.Runtime.CompilerServices namespace.
            if (method.DeclaringType is { Namespace.Value: "System.Runtime.CompilerServices" } type)
            {
                // We might already have a type and method defined in the target module (e.g., NullableAttribute::.ctor(int32)).
                // Try find it in the target module.

                var existingMethod = this.Context.Module
                    .TopLevelTypes.FirstOrDefault(t => t.IsTypeOf(type.Namespace, type.Name))?
                    .Methods.FirstOrDefault(m => method.Name == m.Name && Comparer.Equals(m.Signature, method.Signature));

                // If we found a matching definition, then return it instead of importing the reference.
                if (existingMethod is not null)
                    return existingMethod;
            }

            return base.ImportMethod(method);
        }
    }


We can then pass a custom importer factory to our member cloner constructor as follows:

.. code-block:: csharp

    var cloner = new MemberCloner(destinationModule, context => new MyImporter(context));

All references to methods defined in the ``NSystem.Runtime.CompilerServices`` namespace will then be mapped to the appropriate method definitions if they exist in the target module.

See :ref:`dotnet-importer-common-caveats` for more information on reference importing and its caveats.


Post processing of cloned members
---------------------------------

In some cases, cloned members may need to be post-processed before they are injected into the target module. The ``MemberCloner`` class can be initialized with an instance of a ``IMemberClonerListener``, that gets notified by the cloner object every time a definition was cloned.

Below an example that appends the string ``_Cloned`` to the  name for every cloned type.

.. code-block:: csharp

    public class MyListener : MemberClonerListener
    {
        public override void OnClonedType(TypeDefinition original, TypeDefinition cloned)
        {
            cloned.Name = $"{original.Name}_Cloned";
            base.OnClonedType(original, cloned);
        }
    }

We can then initialize our cloner with an instance of our listener class:

.. code-block:: csharp

    var cloner = new MemberCloner(destinationModule, new MyListener());


Alternatively, we can also override the more generic ``OnClonedMember`` instead, which gets fired for every member definition that was cloned.

.. code-block:: csharp

    public class MyListener : MemberClonerListener
    {
        public override void OnClonedMember(IMemberDefinition original, IMemberDefinition cloned)
        {
            /* ... Do post processing here ... */
            base.OnClonedMember(original, cloned);
        }
    }

As a shortcut, this can also be done by passing in a delegate or lambda instead to the ``MemberCloner`` constructor.

.. code-block:: csharp

    var cloner = new MemberCloner(destinationModule, (original, cloned) => {
        /* ... Do post processing here ... */
    });


Injecting the cloned members
----------------------------

The ``Clone`` method returns a  ``MemberCloneResult``, which contains a register of all members cloned by the member cloner.

- ``OriginalMembers``: The collection containing all original members.
- ``ClonedMembers``: The collection containing all cloned members.
- ``ClonedTopLevelTypes``: A subset of ``ClonedMembers``, containing all cloned top-level types.

Original members can be mapped to their cloned counterpart, using the ``GetClonedMember`` method:

.. code-block:: csharp

    var clonedRectangleType = result.GetClonedMember(rectangleType);

Alternatively, we can get all cloned top-level types.

.. code-block:: csharp

    var clonedTypes = result.ClonedTopLevelTypes;

It is important to note that the ``MemberCloner`` class itself does not inject any of the cloned members by itself. To inject the cloned types, we can for instance add them to the ``ModuleDefinition.TopLevelTypes`` collection:

.. code-block:: csharp

    foreach (var clonedType in clonedTypes)
        destinationModule.TopLevelTypes.Add(clonedType);


However, since injecting the cloned top level types is a very common use-case for the cloner, AsmResolver defines the ``InjectTypeClonerListener`` class that implements a cloner listener that injects all top level types automatically into the destination module. In such a case, the code can be reduced to the following:

.. code-block:: csharp

    new MemberCloner(destinationModule, new InjectTypeClonerListener(destinationModule))
        .Include(rectangleType)
        .Include(vectorType)
        .Clone();

    // `destinationModule` now contains copies of `rectangleType` and `vectorType`.
