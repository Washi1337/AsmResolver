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

Opening an image can be done through one of the `FromXXX` methods from the ``PEImageBase`` class:

.. code-block:: csharp

    var peImage = PEImageBase.FromFile(@"C:\myfile.exe");

.. code-block:: csharp

    byte[] raw = ...
    var peImage = PEImageBase.FromBytes(raw);

.. code-block:: csharp

    IBinaryStreamReader reader = ...
    var peImage = PEImageBase.FromReader(reader);

