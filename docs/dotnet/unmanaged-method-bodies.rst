Native Method Bodies
====================

Method bodies in .NET binaries are not limited to CIL as the implementation language. Mixed-mode applications can contain methods implemented using unmanaged code that runs directly on the underlying processor. Languages might include x86 or ARM, and are always platform-specific.

AsmResolver supports creating new method bodies that are implemented this way. The relevant models in this document can be found in the following namespaces:

.. code-block:: csharp

    using AsmResolver.DotNet.Code.Native;


Prerequisites
-------------

Before you can start adding native method bodies to a .NET module, a few prerequisites have to be met. Failing to do so will make the CLR not run your mixed mode application, and might throw runtime or image format exceptions, even if everything else conforms to the right format. This section will go over these requirements briefly.

Allowing native code in modules
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

To make the CLR treat the output file as a mixed mode application, the ``ILOnly`` flag needs to be unset:

.. code-block:: csharp

    ModuleDefinition module = ...
    module.Attributes &= ~DotNetDirectoryFlags.ILOnly;

Furthermore, make sure the right architecture is specified. For example, for an x86 64-bit binary, use the following:

.. code-block:: csharp

    module.PEKind = OptionalHeaderMagic.Pe32Plus;
    module.MachineType = MachineType.Amd64;
    module.Attributes &= ~DotNetDirectoryFlags.Bit32Required;

For 32-bit x86 binaries, use the following:

.. code-block:: csharp

    module.PEKind = OptionalHeaderMagic.Pe32;
    module.MachineType = MachineType.I386;
    module.Attributes |= DotNetDirectoryFlags.Bit32Required;


Flags for native methods
~~~~~~~~~~~~~~~~~~~~~~~~

As per ECMA-335 specification, a method definition can only represent a native function via Platform Invoke (P/Invoke). While P/Invoke is usually used for importing functions from external libraries (such as `kernel32.dll`), it is also needed for implementing native methods that are defined within the current .NET module itself. Therefore, to be able to assign a valid native body to a method, the right flags need to be set in both the ``Attributes`` and ``ImplAttributes`` property of a ``MethodDefinition``:

.. code-block:: csharp

    MethodDefinition method = ...

    method.Attributes |= MethodAttributes.PInvokeImpl;
    method.ImpleAttributes |= MethodImplAttributes.Native | MethodImplAttributes.Unmanaged | MethodImplAttributes.PreserveSig;


The NativeMethodBody class
--------------------------

The ``MethodDefinition`` class defines a property called ``NativeMethodBody``, which exposes the unmanaged implementation of the method.

Each ``NativeMethodBody`` is assigned to exactly one ``MethodDefinition``. Upon instantiation of such a method body, it is therefore required to specify the owner of the body:

.. code-block:: csharp

    MethodDefinition method = ...

    var body = new NativeMethodBody(method);
    method.NativeMethodBody = body;


The ``NativeMethodBody`` class consists of the following basic building blocks:

- ``Code``: The raw code stream to be executed.
- ``AddressFixups``:  A collection of fixups that need to be applied within the code upon writing the code to the disk.

In the following sections, we will briefly go over each of them.

Writing native code
-------------------

The contents of a native method body can be set through the ``Code`` property. This is a ``byte[]`` that represents the raw code stream to be executed. Below an example of a simple method body written in x86 64-bit assembly code, that returns the constant ``1337``:

.. code-block:: csharp

    body.Code = new byte[]
    {
        0xb8, 0x39, 0x05, 0x00, 0x00, // mov rax, 1337
        0xc3                          // ret
    };


.. note::

    Since native method bodies are platform dependent, AsmResolver does not provide a standard way to encode these instructions. To construct the byte array that you need for a particular implementation of a method body, consider using a third-party assembler or assembler library.


Symbols and Address Fixups
--------------------------

In a lot of cases, native method bodies that references symbols (such as imported functions) require direct addresses to be referenced within its instructions. Since the addresses of these symbols are not known yet upon creating a ``NativeMethodBody``, it is not possible to encode such an operand directly in the ``Code`` byte array. To support these kinds of references regardless, AsmResolver can be instructed to apply address fixups just before writing the body to the disk. These instructions are essentially small pieces of information that tell AsmResolver that at a particular offset the bytes should be replaced with a reference to a symbol in the final PE. This can be applied to any object that implements ``ISymbol``. In the following, two of the most commonly used symbols will be discussed.


Imported Symbols
~~~~~~~~~~~~~~~~

In the PE file format, symbols from external modules are often imported by placing an entry into the imports directory. This is essentially a table of names that the Windows PE loader will go through, look up the actual address of each name, and put it in the import address table. Typically, when a piece of code is meant to make a call to an external function, the code will make an indirect call to an entry stored in this table. In x86 64-bit, using nasm syntax, a call to the ``puts`` function might look like the following snippet:

.. code-block:: csharp

    ...
    lea rcx, [rel message]
    call qword [rel puts]
    ...

Consider the following example x86 64-bit code, that is printing the text ``Hello from the unmanaged world!`` to the standard output stream using the ``puts`` function.

.. code-block:: csharp

    body.Code = new byte[]
    {
        /* 00: */ 0x48, 0x83, 0xEC, 0x28,                     // sub rsp, 0x28

        /* 04: */ 0x48, 0x8D, 0x0D, 0x10, 0x00, 0x00, 0x00,   // lea rcx, [rel message]
        /* 0B: */ 0xFF, 0x15, 0x00, 0x00, 0x00, 0x00,         // call [rel puts]

        /* 11: */ 0xB8, 0x37, 0x13, 0x00, 0x00,               // mov eax, 0x1337

        /* 16: */ 0x48, 0x83, 0xC4, 0x28,                     // add rsp, 0x28
        /* 1A: */ 0xC3,                                       // ret

        // message:
        0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x20, 0x66,   // "Hello f"
        0x72, 0x6f, 0x6d, 0x20, 0x74, 0x68, 0x65,   // "rom the"
        0x20, 0x75, 0x6e, 0x6d, 0x61, 0x6e, 0x61,   // " unmana"
        0x67, 0x65, 0x64, 0x20, 0x77, 0x6f, 0x72,   // "ged wor"
        0x6c, 0x64, 0x21, 0x00                      // "ld!"
    };


Notice how the operand of the ``call`` instruction is left at zero (``0x00``) bytes. To let AsmResolver know that these 4 bytes are to be replaced by an address to an entry in the import address table, we first create a new instance of ``ImportedSymbol``, representing the ``puts`` symbol:

.. code-block:: csharp

    var ucrtbased = new ImportedModule("ucrtbased.dll");
    var puts = new ImportedSymbol(0x4fc, "puts");
    ucrtbased.Symbols.Add(puts);


We can then add it as a fixup to the method body:

.. code-block:: csharp

    body.AddressFixups.Add(new AddressFixup(
        0xD, AddressFixupType.Relative32BitAddress, puts
    ));


Local Symbols
~~~~~~~~~~~~~

If a native body is supposed to process or return some data that is defined within the body itself, the ``NativeLocalSymbol`` class can be used.

Consider the following example x86 32-bit snippet, that returns the virtual address of a string.

.. code-block:: csharp

    0xB8, 0x00, 0x00, 0x00, 0x00 // mov eax, message
    0xc3,                        // ret

    // message (unicode):
    0x48, 0x00, 0x65, 0x00, 0x6c, 0x00, 0x6c, 0x00, 0x6f, 0x00, 0x2c, 0x00, 0x20, 0x00, // "Hello, "
    0x77, 0x00, 0x6f, 0x00, 0x72, 0x00, 0x6c, 0x00, 0x64, 0x00, 0x21, 0x00, 0x00, 0x00  // "world!."


Notice how the operand of the ``mov`` instruction is left at zero (``0x00``) bytes. To let AsmResolver know that these 4 bytes are to be replaced by the actual virtual address to ``message``, we can define a local symbol and register an address fixup in the following manner:

.. code-block:: csharp

    var message = new NativeLocalSymbol(body, offset: 0x6);
    body.AddressFixups.Add(new AddressFixup(
        0x1, AddressFixupType.Absolute32BitAddress, message
    ));


.. warning::

    The ``NativeLocalSymbol`` can only be used within the code of the native method body itself. This is due to the fact that these types of symbols are not processed further after serializing a ``NativeMethodBody`` to a ``CodeSegment`` by the default method body serializer.


Fixup Types
~~~~~~~~~~~

The type of fixup that is required will depend on the architecture and instruction that is used. Below an overview of all fixups that AsmResolver is able to apply:

+--------------------------+-----------------------------------------------------------------------+---------------------------------+
| Fixup type               | Description                                                           | Example instructions            |
+==========================+=======================================================================+=================================+
| ``Absolute32BitAddress`` | The operand is a 32-bit absolute virtual address                      | ``call dword [address]``        |
+--------------------------+-----------------------------------------------------------------------+---------------------------------+
| ``Absolute64BitAddress`` | The operand is a 64-bit absolute virtual address                      | ``mov rax, address``            |
+--------------------------+-----------------------------------------------------------------------+---------------------------------+
| ``Relative32BitAddress`` | The operand is an address relative to the current instruction pointer | ``call qword [rip+offset]``     |
+--------------------------+-----------------------------------------------------------------------+---------------------------------+
