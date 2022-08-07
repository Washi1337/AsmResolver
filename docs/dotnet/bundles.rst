AppHost / SingleFileHost Bundles
================================

Since the release of .NET Core 3.1, it is possible to deploy .NET assemblies as a single binary. These files are executables that do not contain a traditional .NET metadata header, and run natively on the underlying operating system via a platform-specific application host bootstrapper.

AsmResolver supports extracting the embedded files from these types of binaries. Additionally, given an application host template provided by the .NET SDK, AsmResolver also supports constructing new bundles as well. All relevant code is found in the following namespace:

.. code-block:: csharp

    using AsmResolver.DotNet.Bundles;


Creating Bundles
----------------

.NET bundles are represented using the ``BundleManifest`` class. Creating new bundles can be done using any of the constructors:

.. code-block:: csharp

    var manifest = new BundleManifest(majorVersionNumber: 6);


The major version number refers to the file format that should be used when saving the manifest. Below an overview of the values that are recognized by the CLR:

+----------------------+----------------------------+
| .NET Version Number  | Bundle File Format Version |
+======================+============================+
| .NET Core 3.1        | 1                          |
+----------------------+----------------------------+
| .NET 5.0             | 2                          |
+----------------------+----------------------------+
| .NET 6.0             | 6                          |
+----------------------+----------------------------+

To create a new bundle with a specific bundle identifier, use the overloaded constructor

.. code-block:: csharp

    var manifest = new BundleManifest(6, "MyBundleID");


It is also possible to change the version number as well as the bundle ID later, since these values are exposed as mutable properties ``MajorVersion`` and ``BundleID``

.. code-block:: csharp

    manifest.MajorVersion = 6;
    manifest.BundleID = manifest.GenerateDeterministicBundleID();

.. note::

    If ``BundleID`` is left unset (``null``), it will be automatically assigned a new one using ``GenerateDeterministicBundleID`` upon writing.


Reading Bundles
---------------

Reading and extracting existing bundle manifests from an executable can be done by using one of the ``FromXXX`` methods:

.. code-block:: csharp

    var manifest = BundleManifest.FromFile(@"C:\Path\To\Executable.exe");

.. code-block:: csharp

    byte[] contents = ...
    var manifest = BundleManifest.FromBytes(contents);

.. code-block:: csharp

    IDataSource contents = ...
    var manifest = BundleManifest.FromDataSource(contents);


Similar to the official .NET bundler and extractor, the methods above locate the bundle in the file by looking for a specific signature first. However, official implementations of the application hosting program itself actually do not verify or use this signature in any shape or form. This means that a third party can replace or remove this signature, or write their own implementation of an application host that does not adhere to this standard, and thus throw off static analysis of the file.

AsmResolver does not provide built-in alternative heuristics for finding the right start address of the bundle header. However, it is possible to implement one yourself and provide the resulting start address in one of the overloads of the ``FromXXX`` methods:

.. code-block:: csharp

    byte[] contents = ...
    ulong bundleAddress = ...
    var manifest = BundleManifest.FromBytes(contents, bundleAddress);

.. code-block:: csharp

    IDataSource contents = ...
    ulong bundleAddress = ...
    var manifest = BundleManifest.FromDataSource(contents, bundleAddress);


Writing Bundles
---------------

Constructing new bundled executable files requires a template file that AsmResolver can base the final output on. This is similar how .NET compilers themselves do this as well. By default, the .NET SDK installs template binaries in one of the following directories:

- ``<DOTNET-INSTALLATION-PATH>/sdk/<version>/AppHostTemplate``
- ``<DOTNET-INSTALLATION-PATH>/packs/Microsoft.NETCore.App.Host.<runtime-identifier>/<version>/runtimes/<runtime-identifier>/native``

Using this template file, it is then possible to write a new bundled executable file using ``WriteUsingTemplate``:

.. code-block:: csharp

    BundleManifest manifest = ...
    manifest.WriteUsingTemplate(
        @"C:\Path\To\Output\File.exe",
        new BundlerParameters(
            appHostTemplatePath: @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Host.win-x64\6.0.0\runtimes\win-x64\native\apphost.exe",
            appBinaryPath: @"HelloWorld.dll"));


Typically on Windows, use an ``apphost.exe`` template if you want to construct a native binary that is framework dependent, and ``singlefilehost.exe`` for a fully self-contained binary. On Linux, use the ``apphost`` and ``singlefilehost`` ELF equivalents.

For bundle executable files targeting Windows, it may be required to copy over some values from the original PE file into the final bundle executable file. Usually these values include fields from the PE headers (such as the executable's sub-system target) and Win32 resources (such as application icons and version information). AsmResolver can automatically update these headers by specifying a source image to pull this data from in the ``BundlerParameters``:

.. code-block:: csharp

    BundleManifest manifest = ...
    manifest.WriteUsingTemplate(
        @"C:\Path\To\Output\File.exe",
        new BundlerParameters(
            appHostTemplatePath: @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Host.win-x64\6.0.0\runtimes\win-x64\native\apphost.exe",
            appBinaryPath: @"HelloWorld.dll",
            imagePathToCopyHeadersFrom: @"C:\Path\To\Original\HelloWorld.exe"));

``BundleManifest`` also defines other ```WriteUsingTemplate`` overloads taking ``byte[]``, ``IDataSource`` or ``IPEImage`` instances instead of paths.


Managing Files
--------------

Files in a bundle are represented using the ``BundleFile`` class, and are exposed by the ``BundleManifest.Files`` property. Both the class as well as the list itself is fully mutable, and thus can be used to add, remove or modify files in the bundle.

Creating a new file can be done using the constructors:

.. code-block:: csharp

    var newFile = new BundleFile(
        relativePath: "HelloWorld.dll",
        type: BundleFileType.Assembly,
        contents: System.IO.File.ReadAllBytes(@"C:\Binaries\HelloWorld.dll"));

    manifest.Files.Add(newFile);


It is also possible to iterate over all files and inspect their contents using ``GetData``:

.. code-block:: csharp

    foreach (var file in manifest.Files)
    {
        string path = file.RelativePath;
        byte[] contents = file.GetData();

        Console.WriteLine($"Extracting {path}...");
        System.IO.File.WriteAllBytes(path, contents);
    }


Changing the contents of an existing file can be done using the ``Contents`` property.

.. code-block:: csharp

    BundleFile file = ...
    file.Contents = new DataSegment(new byte[] { 1, 2, 3, 4 });


If the bundle manifest is put into a single-file host template (e.g. ``singlefilehost.exe``), then files can also be compressed or decompressed:

.. code-block:: csharp

    file.Compress();
    // file.Contents now contains the compressed version of the data and file.IsCompressed = true

    file.Decompress();
    // file.Contents now contains the decompressed version of the data and file.IsCompressed = false

