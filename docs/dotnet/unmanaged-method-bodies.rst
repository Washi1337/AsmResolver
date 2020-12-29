Native Method Bodies
====================

Method bodies in .NET binaries are not limited to CIL as the implementation language. Mixed-mode applications can contain methods implemented using unmanaged code that runs directly on the underlying processor. Languages might include x86 or ARM, and are always platform-specific.

AsmResolver supports creating new method bodies that are implemented this way. The relevant models in this document can be found in the following namespaces:

.. code-block:: csharp

    using AsmResolver.DotNet.Code.Native;


Allowing native code in modules
-------------------------------

Before you can start adding native method bodies to a .NET module, it is required to change a couple of flags in the headers of the executable. In particular, to make the CLR run a mixed mode application, the ``ILOnly`` flag needs to be unset:

.. code-block:: csharp 

    ModuleDefinition module = ...
    module.Attributes &= DotNetDirectoryFlags.ILOnly;

Failing to do so will make the CLR not run your mixed mode application, and might throw runtime or image format exceptions, even if everything else conforms to the right format.

Furthermore, make sure the right architecture is specified. For example, for an x86 64-bit binary, use the folloing:

.. code-block:: csharp

    module.PEKind = OptionalHeaderMagic.Pe32Plus;
    module.MachineType = MachineType.Amd64;
    module.Attributes &= ~DotNetDirectoryFlags.Bit32Required;

For 32-bit x86 binaries, use the following:

.. code-block:: csharp

    module.PEKind = OptionalHeaderMagic.Pe32;
    module.MachineType = MachineType.I386;
    module.Attributes |= DotNetDirectoryFlags.Bit32Required;


The NativeMethodBody class
--------------------------

The ``MethodDefinition`` class defines a property called ``NativeMethodBody``, which exposes the unmanaged implementation of the method.

Each ``NativeMethodBody`` is assigned to exactly one ``MethodDefinition``. Upon instantiation of such a method body, it is therefore required to specify the owner of the body:

.. code-block:: csharp

    MethodDefinition method = ...

    NativeMethodBody body = new NativeMethodBody(method);
    method.NativeMethodBody = body;


The ``NativeMethodBody`` class consists of the following basic building blocks:

- ``Code``: The raw code stream to be executed.
- ``AddressFixups``:  A collection of fixups that need to be applied within the code upon writing the code to the disk.

In the following sections, we will briefly go over each of them.

Writing native code
-------------------

The contents of a native method body can be set through the ``Code`` property. This is a ``byte[]`` that represents the raw code stream to be executed. Below an example of a simple method body written in x86 64-bit assembly code, that returns the constant ``0x1337``:

.. code-block:: csharp

    body.Code = new byte[]
    {
        0xb8, 0x39, 0x05, 0x00, 0x00, // mov rax, 1337
        0xc3                          // ret
    };


.. note::
    
    Since native method bodies are platform dependent, AsmResolver does not provide a standard way to encode these instructions. To construct the byte array that you need for a particular implementation of a method body, consider using a third-party assembler or assembler library.


References to external symbols
------------------------------

In a lot of cases, methods require making calls to functions defined in external libraries and native method bodies are no exception. In the PE file format, these kinds of symbols are often put into the imports directory. This is essentially a table of names that the Windows PE loader will go through, look up the actual address of each name, and put it in the import address table. Typically, when a piece of code is meant to make a call to an external function, the code will make an indirect call to an entry stored in this table. In x86 64-bit, using nasm syntax, a call to the ``puts`` function might look like the following snippet: 

.. code-block:: csharp

    ...
    lea rcx, [rel message]
    call qword [rel puts]
    ...

Since the import directory is not constructed yet when we are operating on the abstraction level of a ``ModuleDefinition``, the address of the import address entry is still unknown. Therefore, it is not possible to encode an operand like the one in the call instruction of the above example.

To support these kinds of references in native method bodies regardless, it is possible to instruct AsmResolver to apply address fixups just before writing the body to the disk. These are essentially small pieces of information that tell AsmResolver that at a particular offset the bytes should be replaced with a reference to a symbol in the final PE.

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
    

Notice how the operand of the call instruction is left at zero (`0x00`) bytes. To let AsmResolver know that these 4 bytes are to be replaced by an address to an entry in the import address table, we first create a new instance of ``ImportedSymbol``, representing the ``puts`` symbol:

.. code-block:: csharp

    var ucrtbased = new ImportedModule("ucrtbased.dll");
    var puts = new ImportedSymbol(0x4fc, "puts");
    ucrtbased.Symbols.Add(puts);
    

We can then add it as a fixup to the method body:

.. code-block:: csharp

    body.AddressFixups.Add(new AddressFixup(
        0xD, AddressFixupType.Relative32BitAddress, puts
    ));


The type of fixup that is required will depend on the architecture and instruction that is used. Below an overview:

+--------------------------+-----------------------------------------------------------------------+---------------------------------+
| Fixup type               | Description                                                           | Example instructions            |
+==========================+=======================================================================+=================================+
| ``Absolute32BitAddress`` | The operand is an absolute virtual address                            | ``call dword [address]``        |
+--------------------------+-----------------------------------------------------------------------+---------------------------------+
| ``Relative32BitAddress`` | The operand is an address relative to the current instruction pointer | ``call qword [rip+offset]``     |
+--------------------------+-----------------------------------------------------------------------+---------------------------------+
