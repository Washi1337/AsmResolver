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

The above will create a module that references mscorlib.dll 4.0.0.0 (.NET Framework 4.0). If another version of the Common Object Runtime Library is desired, we can use one of the overloads of the constructor, and use a custom ``AssemblyReference``, or one of the pre-defined assembly references in the ``KnownCorLibs`` class to target another version of the library.

.. code-block:: csharp 

    var module = new ModuleDefinition("MyModule.exe", KnownCorLibs.SystemRuntime_v4_2_2_0);


Opening a .NET module
---------------------

Opening a .NET module can be done through one of the `FromXXX` methods from the ``ModuleDefinition`` class:

.. code-block:: csharp

    byte[] raw = ...
    ModuleDefinition module = ModuleDefinition.FromBytes(raw);
    
.. code-block:: csharp

    ModuleDefinition module = ModuleDefinition.FromFile(@"C:\myfile.exe");

.. code-block:: csharp

    PEFile peFile = ...
    ModuleDefinition module = ModuleDefinition.FromFile(peFile);

.. code-block:: csharp

    BinaryStreamReader reader = ...
    ModuleDefinition module = ModuleDefinition.FromReader(reader);

.. code-block:: csharp

    IPEImage peImage = ...
    ModuleDefinition module = ModuleDefinition.FromImage(peImage);


If you want to read large files (+100MB), consider using memory mapped I/O instead:

.. code-block:: csharp

    using var service = new MemoryMappedFileService();
    ModuleDefinition module = ModuleDefinition.FromFile(service.OpenFile(@"C:\myfile.exe"));


On Windows, if a module is loaded and mapped in memory (e.g. as a dependency defined in Metadata or by the means of ``System.Reflection``), it is possible to load the module from memory by transforming the module into a ``HINSTANCE`` (a.k.a. module base address), and then providing it to AsmResolver:

.. code-block:: csharp

    Module module = ...;
    IntPtr hInstance = Marshal.GetHINSTANCE(module);
    ModuleDefinition module = ModuleDefinition.FromModuleBaseAddress(hInstance);
    

Writing a .NET module
---------------------

Writing a .NET module can be done through one of the `Write` method overloads.

.. code-block:: csharp

    module.Write(@"C:\myfile.patched.exe");

.. code-block:: csharp

    Stream stream = ...;
    module.Write(stream);

For more advanced options to write .NET modules, see Advanced PE Image building.


Creating a new .NET assembly
----------------------------

AsmResolver also supports creating entire (multi-module) .NET assemblies instead.

.. code-block:: csharp

    var assembly = new AssemblyDefinition("MyAssembly", new Version(1, 0, 0, 0));


Opening a .NET assembly
-----------------------

Opening (multi-module) .NET assemblies can be done in a very similar fashion as reading a single module:

.. code-block:: csharp

    byte[] raw = ...
    AssemblyDefinition assembly = AssemblyDefinition.FromBytes(raw);

.. code-block:: csharp

    AssemblyDefinition assembly = AssemblyDefinition.FromFile(@"C:\myfile.exe");

.. code-block:: csharp

    IPEFile peFile = ...
    AssemblyDefinition assembly = AssemblyDefinition.FromFile(peFile);

.. code-block:: csharp

    BinaryStreamReader reader = ...
    AssemblyDefinition assembly = AssemblyDefinition.FromReader(reader);

.. code-block:: csharp

    IPEImage peImage = ...
    AssemblyDefinition assembly = AssemblyDefinition.FromImage(peImage);

    
If you want to read large files (+100MB), consider using memory mapped I/O instead:

.. code-block:: csharp

    using var service = new MemoryMappedFileService();
    AssemblyDefinition assembly = AssemblyDefinition.FromFile(service.OpenFile(@"C:\myfile.exe"));


Writing a .NET assembly
-----------------------

Writing a .NET assembly can be done through one of the `Write` method overloads.

.. code-block:: csharp

    assembly.Write(@"C:\myfile.patched.exe");

For more advanced options to write .NET modules, see Advanced PE Image building.