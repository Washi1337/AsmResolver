Basic I/O
=========

Every .NET image interaction is done through classes defined by the ``AsmResolver.DotNet`` namespace:

.. code-block:: csharp

    using AsmResolver.DotNet;

Creating a new .NET module
--------------------------

Creating a new image can be done by instantiating a ``ModuleDefinition`` class:

.. code-block:: csharp

    var module = new ModuleDefinition("MyModule.exe");


Opening a .NET module
---------------------

Opening a .NET module can be done through one of the `FromXXX` methods from the ``ModuleDefinition`` class:

.. code-block:: csharp

    ModuleDefinition module = ModuleDefinition.FromFile(@"C:\myfile.exe");

.. code-block:: csharp

    PEFile peFile = ...
    ModuleDefinition module = ModuleDefinition.FromFile(peFile);

.. code-block:: csharp

    byte[] raw = ...
    ModuleDefinition module = ModuleDefinition.FromBytes(raw);

.. code-block:: csharp

    IBinaryStreamReader reader = ...
    ModuleDefinition module = ModuleDefinition.FromReader(reader);

.. code-block:: csharp

    IPEImage peImage = ...
    ModuleDefinition module = ModuleDefinition.FromImage(peImage);

.. code-block:: csharp

    IMetadata metadata = ...
    ModuleDefinition module = ModuleDefinition.FromMetadata(metadata);


Creating a new .NET assembly
----------------------------

AsmResolver also supports creating entire (multi-module) .NET assemblies instead.

.. code-block:: csharp

    var assembly = new AssemblyDefinition("MyAssembly", new Version(1, 0, 0, 0));


Opening a .NET assembly
-----------------------

Opening (multi-module) .NET assemblies can be done in a very similar fashion as reading a single module:

.. code-block:: csharp

    AssemblyDefinition assembly = AssemblyDefinition.FromFile(@"C:\myfile.exe");

.. code-block:: csharp

    PEFile peFile = ...
    AssemblyDefinition assembly = AssemblyDefinition.FromFile(peFile);

.. code-block:: csharp

    byte[] raw = ...
    AssemblyDefinition assembly = AssemblyDefinition.FromBytes(raw);

.. code-block:: csharp

    IBinaryStreamReader reader = ...
    AssemblyDefinition assembly = AssemblyDefinition.FromReader(reader);

.. code-block:: csharp

    IPEImage peImage = ...
    AssemblyDefinition assembly = AssemblyDefinition.FromImage(peImage);

.. code-block:: csharp

    IMetadata metadata = ...
    AssemblyDefinition assembly = AssemblyDefinition.FromMetadata(metadata);

