Managed Resources
=================

.NET modules may define one or more resource files. Similar to Win32 resources, these are files that contain additional data, such as images, strings or audio files, that are used by the module at run time.

Manifest Resources
------------------

AsmResolver models managed resources using the ``ManifestResource`` class, and they are exposed by the ``ModuleDefinition.Resources`` property. Below an example snippet that prints the names of all resources in a given module:

.. code-block:: csharp

    var module = ModuleDefinition.FromFile(...);
    foreach (var resource in module.Resources)
        Console.WriteLine(resource.Name);


A ``ManifestResource`` can either be embedded in the module itself, or present in an external assembly or file. When it is embedded, the contents of the file can be accessed using the ``EmbeddedDataSegment`` property. This is a mutable property, so it is also possible to assign new data to the resource this way.

.. code-block:: csharp

    ManifestResource resource = ...
    if (resource.IsEmbedded)
    {
        // Get data segment of the resource.
        var oldData = resource.EmbeddedDataSegment;
        
        // Assign new data to the resource.
        var newData = new DataSegment(new byte[] { 1, 2, 3, 4});
        resource.EmbeddedDataSegment = newData;
    }


The ``ManifestResource`` class also defines a convenience ``GetData`` method, for quickly obtaining the data stored in the resource as a ``byte[]``:

.. code-block:: csharp

    ManifestResource resource = ...
    if (resource.IsEmbedded)
    {
        byte[] data = resource.GetData();
        // ...
    }


Alternatively, you can use the ``TryGetReader`` method to immediately instantiate a ``BinaryStreamReader`` for the data. This can be useful if you want to parse the contents of the resource file later.

.. code-block:: csharp

    ManifestResource resource = ...
    if (resource.TryGetReader(out var reader)
    {
        // ...
    }


If the resource is not embedded, the ``Implementation`` property will indicate in which file the resource can be found, and ``Offset`` will indicate where in this file the data starts.

.. code-block:: csharp

    ManifestResource resource = ...
    switch (resource.Implementation)
    {
        case FileReference fileRef:
            // Resource is stored in another file.
            string name = fileRef.Name;
            uint offset = resource.Offset;
            ...
            break;
        
        case AssemblyReference assemblyRef:
            // Resource is stored in another assembly.
            var assembly = assemblyRef.Resolve();
            var actualResource = assembly.ManifestModule.Resources.First(r => r.Name == resource.Name);
            ...
            break;

        case null:
            // Resource is embedded.
            ...
            break
    }


Resource Sets
-------------

Many .NET applications (mainly Windows Forms apps) make use of manifest resources to store *resource sets*. These are resources that have the  ``.resources`` file extension, and combine multiple smaller resources (often localized strings or images) into one manifest resource file.

AsmResolver supports parsing and building new resource sets using the ``ResourceSet`` class. This class is defined in the ``AsmResolver.DotNet.Resources`` namespace:

.. code-block:: csharp

    using AsmResolver.DotNet.Resources;


Creating new Resource Sets
~~~~~~~~~~~~~~~~~~~~~~~~~~

Creating new sets can be done using the constructors of ``ResourceSet``. 

.. code-block:: csharp
    
    var set = new ResourceSet();


By default, the parameterless constructor will create a resource set with a header that references the ``System.Resources.ResourceReader`` and ``System.Resources.RuntimeResourceSet`` types, both from ``mscorlib`` version ``4.0.0.0``. This can be customized if needed, by using another constructor overload that takes a ``ResourceManagerHeader`` instance instead:

.. code-block:: csharp
    
    var set = new ResourceSet(ResourceManagerHeader.Deserializing_v4_0_0_0);


Alternatively, you can change the header using the ``ResourceSet.ManagerHeader`` property:

.. code-block:: csharp
    
    var set = new ResourceSet();
    set.ManagerHeader = ResourceManagerHeader.Deserializing_v4_0_0_0;


Reading existing Resource Sets
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

Reading existing resource sets can be done using the ``ResourceSet.FromReader`` method:

.. code-block:: csharp

    ManifestResource resource = ...
    if (resource.TryGetReader(out var reader)
    {
        var set = ResourceSet.FromReader(reader);
        // ...
    }
    

By default, AsmResolver will read and deserialize entries in a resource set. However, to prevent arbitrary code execution, it will not interpret the data of each entry that is of a non-intrinsic resource type. For these types of entries, AsmResolver will expose the raw data as a ``byte[]`` instead. If you want to change this behavior, you can provide a custom instance of ``IResourceDataSerializer`` or extend the default serializer so that it supports additional resource types.


.. code-block:: csharp

    public class MyResourceDataSerializer : DefaultResourceDataSerializer
    {
        /// <inheritdoc />
        public override object? Deserialize(in BinaryStreamReader reader, ResourceType type)
        {
            // ...
        }
    }

    ManifestResource resource = ...
    if (resource.TryGetReader(out var reader)
    {
        var set = ResourceSet.FromReader(reader, new MyResourceDataSerializer());
        // ...
    }


Accessing Resource Set Entries
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

The ``ResourceSet`` class is a mutable list of ``ResourceSetEntry``, which includes the name, the type of the resource and the deserialized data:

.. code-block:: csharp

    foreach (var entry in set)
    {
        Console.WriteLine("Name: " + entry.Name);
        Console.WriteLine("Type: " + entry.Type.FullName);
        Console.WriteLine("Data: " + entry.Data);
    }


New items can be created using any of the constructors. 

.. code-block:: csharp

    var stringEntry = new ResourceSetEntry("MyString", ResourceTypeCode.String, "Hello, world!");
    set.Add(stringEntry);

    var intEntry = new ResourceSetEntry("MyInt", ResourceTypeCode.Int32, 1234);
    set.Add(intEntry);


AsmResolver also supports reading and adding resource elements that are of a user-defined type:

.. code-block:: csharp

    var pointType = new UserDefinedResourceType(
        "System.Drawing.Point, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");

    var serializedContents = new byte[]
    {
        0x03, 0x06, 0x31, 0x32, 0x2C, 0x20, 0x33, 0x34  // "12, 34"
    };

    var entry = new ResourceSetEntry("MyLocation", type, serializedContents);
    set.Add(entry);


.. note::

    When using user-defined types, some implementations of the CLR will require a special resource reader type (such as ``System.Resources.Extensions.DeserializingResourceReader``) to be referenced in the manager header of the resource set. Therefore, make sure you have the right manager header provided in the ``ResourceSet`` that defines such a compatible reader type.


Writing Resource Sets
~~~~~~~~~~~~~~~~~~~~~

Serializing resource sets can be done using the ``ResourceSet.Write`` method.

.. code-block:: csharp

    using var stream = new MemoryStream();
    var writer = new BinaryStreamWriter(stream);
    set.Write(writer);
    


By default, AsmResolver will serialize entries in a resource set using a default serializer. However, to prevent arbitrary code execution, it will not attempt to serialize objects that are of a non-intrinsic resource type. The default serializer expects a ``byte[]`` for user-defined resource types. If you want to change this behavior, you can provide a custom instance of ``IResourceDataSerializer`` or extend the default serializer so that it supports additional resource types.

.. code-block:: csharp

    public class MyResourceDataSerializer : DefaultResourceDataSerializer
    {
        /// <inheritdoc />
        public override void Serialize(IBinaryStreamWriter writer, ResourceType type, object? value)
        {
            // ...
        }
    }

    using var stream = new MemoryStream();
    var writer = new BinaryStreamWriter(stream);
    set.Write(writer, new MyResourceDataSerializer());