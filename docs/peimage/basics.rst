Basic I/O
=========

Every PE image interaction is done through classes defined by the ``AsmResolver.PE`` namespace:

.. code-block:: csharp

    using AsmResolver.PE;

Creating a new PE image
-----------------------

Creating a new image can be done by instantiating a ``PEImage`` class:

.. code-block:: csharp

    var peImage = new PEImage();


Opening a PE image
------------------

Opening an image can be done through one of the `FromXXX` methods from the ``PEImage`` class:

.. code-block:: csharp

    IPEImage peImage = PEImage.FromFile(@"C:\myfile.exe");

.. code-block:: csharp

    PEFile peFile = ...
    IPEImage peImage = PEImage.FromFile(peFile);

.. code-block:: csharp

    byte[] raw = ...
    IPEImage peImage = PEImage.FromBytes(raw);

.. code-block:: csharp

    IBinaryStreamReader reader = ...
    IPEImage peImage = PEImage.FromReader(reader);


Writing a PE image
-------------------

Building an image back to a PE file can be done by using one of the classes that implement the ``IPEFileBuilder`` interface. 

.. note::
    
    Currently AsmResolver only provides a builder for .NET images.

Building a .NET image can be done through the ``AsmResolver.PE.DotNet.Builder.ManagedPEBuilder`` class:

.. code-block:: csharp

    var builder = new ManagedPEBuilder();
    var newPEFile = builder.ConstructPEFile(image);

Once a ``PEFile`` instance has been generated from the image, you can use it to write the executable to an output stream (such as a file on the disk or a memory stream).

.. code-block:: csharp

    using (var stream = File.Create(@"C:\mynewfile.exe"))
    {
        var writer = new BinaryStreamWriter(stream);
        newPEFile.Write(writer);
    }