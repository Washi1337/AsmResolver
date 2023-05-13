# CIL Method Bodies

The relevant models in this document can be found in the following
namespaces:

``` csharp
using AsmResolver.PE.DotNet.Cil;    // Raw models for the CIL language.
using AsmResolver.DotNet.Code.Cil;  // High level and easy to use models related to CIL.
```

## The CilMethodBody class

The `MethodDefinition` class defines a property called `CilMethodBody`,
which exposes the managed implementation of the method, written in the
Common Intermediate Language, or CIL for short.

Each `CilMethodBody` is assigned to exactly one `MethodDefinition`. Upon
instantiation of such a method body, it is therefore required to specify
the owner of the body:

``` csharp
MethodDefinition method = ...

CilMethodBody body = new CilMethodBody(method);
method.CilMethodBody = body;
```

The `CilMethodBody` class consists of the following basic building
blocks:

-   `Instructions`: The instructions to be executed.
-   `LocalVariables`: The local variables defined by the method body.
-   `ExceptionHandlers`: A collection of regions protected by an
    exception handler.

## Basic structure of CIL instructions

Instructions that are assembled into the method body are automatically
disassembled and put in a mutable collection of `CilInstruction`,
accessible by the `Instructions` property.

``` csharp
var instructions = body.Instructions;
```

The `CilInstruction` class defines three basic properties:

-   `Offset`: The offset of the instruction, relative to the start of
    the code stream.
-   `OpCode`: The operation the instruction performs.
-   `Operand`: The operand of the instruction.

By default, depending on the value of `OpCode.OperandType`, `Operand`
contains (and always should contain) one of the following:

|OpCode.OperandType                  |Type of Operand                         |                
|------------------------------------|----------------------------------------|                                        
|`CilOperandType.InlineNone`         |N/A (is always `null`)                  |                        
|`CilOperandType.ShortInlineI`       |`sbyte`                                 |        
|`CilOperandType.InlineI`            |`int`                                   |        
|`CilOperandType.InlineI8`           |`long`                                  |        
|`CilOperandType.ShortInlineR`       |`float`                                 |        
|`CilOperandType.InlineR`            |`double`                                |            
|`CilOperandType.InlineString`       |`string` or `MetadataToken`             |                            
|`CilOperandType.InlineBrTarget`     |`ICilLabel` or `int`                    |                        
|`CilOperandType.ShortInlineBrTarget`|`ICilLabel` or `sbyte`                  |                        
|`CilOperandType.InlineSwitch`       |`IList<ICilLabel>`                      |                    
|`CilOperandType.ShortInlineVar`     |`CilLocalVariable` or `byte`            |                                
|`CilOperandType.InlineVar`          |`CilLocalVariable` or `ushort`          |                                
|`CilOperandType.ShortInlineArgument`|`Parameter` or `byte`                   |                        
|`CilOperandType.InlineArgument`     |`Parameter` or `ushort`                 |                        
|`CilOperandType.InlineField`        |`IFieldDescriptor` or `MetadataToken`   |                                        
|`CilOperandType.InlineMethod`       |`IMethodDescriptor` or `MetadataToken`  |                                        
|`CilOperandType.InlineSig`          |`StandAloneSignature` or `MetadataToken`|                                            
|`CilOperandType.InlineTok`          |`IMetadataMember` or `MetadataToken`    |                                        
|`CilOperandType.InlineType`         |`ITypeDefOrRef` or `MetadataToken`      |                                        


> [!WARNING]
> Providing an incorrect operand type will result in the CIL assembler to
> fail assembling the method body upon writing the module to the disk.

Creating a new instruction can be done using one of the constructors,
together with the `CilOpCodes` static class:

``` csharp
body.Instructions.AddRange(new[]
{
    new CilInstruction(CilOpCodes.Ldstr, "Hello, World!"),
    new CilInstruction(CilOpCodes.Ret),
});
```

However, the preferred way of adding instructions to add or insert new
instructions is to use one of the `Add` or `Insert` overloads that
directly take an opcode and operand. This is because it avoids an
allocation of an array, and the overloads perform immediate validation
on the created instruction.

``` csharp
var instructions = body.Instructions;
instructions.Add(CilOpCodes.Ldstr, "Hello, World!");
instructions.Add(CilOpCodes.Ret);
```

## Pushing 32-bit integer constants onto the stack

In CIL, pushing integer constants onto the stack is done using one of
the `ldc.i4` instruction variants.

The recommended way to create such an instruction is not to use the
constructor, but instead use the `CilInstruction.CreateLdcI4(int)`
method instead. This automatically selects the smallest possible opcode
possible and sets the operand accordingly:

``` csharp
CilInstruction push1 = CilInstruction.CreateLdcI4(1);            // Returns "ldc.i4.1" macro
CilInstruction pushShort = CilInstruction.CreateLdcI4(123);      // Returns "ldc.i4.s 123" macro
CilInstruction pushLarge = CilInstruction.CreateLdcI4(12345678); // Returns "ldc.i4 12345678"
```

If we want to get the pushed value, we can use the
`CilInstruction.GetLdcI4Constant()` method. This method works on any of
the `ldc.i4` variants, including all the macro opcodes that do not
explicitly define an operand such as `ldc.i4.1`.

## Branching Instructions

Branch instructions are instructions that (might) transfer control to
another part of the method body. To reference the instruction to jump to
(the branch target), `ICilLabel` is used. The easiest way to create such
a label is to use the `CreateLabel()` function on the instruction to
reference:

``` csharp
CilInstruction targetInstruction = ...
ICilLabel label = targetInstruction.CreateLabel();

instructions.Add(CilOpCodes.Br, label);
```

Alternatively, when using the `Add` or `Insert` overloads, it is
possible to use the return value of these overloads.

``` csharp
var instructions = body.Instructions;
var label = new CilInstructionLabel();

instructions.Add(CilOpCodes.Br, label);
/* ... */
label.Instruction = instruction.Add(CilOpCodes.Ret);
```

The `switch` operation uses a `IList<ICilLabel>` instead.

> [!NOTE]
> When a branching instruction contains a `null` label or a label that
> references an instruction that is not present in the method body,
> AsmResolver will by default report an exception upon serializing the
> code stream. This can be disabled by setting `VerifyLabelsOnBuild` to
> `false`.

## Finding instructions by offset

Instructions stored in a method body are indexed not by offset, but by
order of occurrence. If it is required to find an instruction by offset,
it is possible to use the `Instructions.GetByOffset(int)` method, which
performs a binary search (O(log(n))) and is faster than a linear search
(O(n)) such as a for loop or using a construction like
`.First(i => i.Offset == offset)` provided by `System.Linq`.

For `GetByOffset` to work, it is required that all offsets in the
instruction collection are up to date. Recalculating all offsets within
an instruction collection can be done through
`Instructions.CalculateOffsets()`.

``` csharp
// Calculate all offsets once ...
body.Instructions.CalculateOffsets();

// Look up multiple times.
var instruction1 = body.Instructions.GetByOffset(0x0012);
var instruction2 = body.Instructions.GetByOffset(0x0020);

// Find the index of an instruction.
int index = body.Instructions.GetIndexByOffset(0x0012);
instruction1 = body.Instructions[index];
```

## Referencing members

As specified by the table above, operations such as a `call` require a
member as operand.

It is important that the member referenced in the operand of such an
instruction is imported in the module. This can be done using the
`ReferenceImporter` class.

Below an example on how to use the `ReferenceImporter` to emit a call to
`Console::WriteLine(string)` using reflection:

``` csharp
var importer = new ReferenceImporter(targetModule);
var writeLine = importer.ImportMethod(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) } );

body.Instructions.Add(new CilInstruction(CilOpCodes.Ldstr, "Hello, world!"));
body.Instructions.Add(new CilInstruction(CilOpCodes.Call, writeLine));
```

More information on the capabilities and limitations of the
`ReferenceImporter` can be found in
[Reference Importing](importing.md).

## Expanding and optimising macros

CIL defines a couple of macro operations that do the same as their full
counterpart, but require less space to be encoded. For example, the
`ldc.i4.1` instruction is a macro for `ldc.i4 1`, and requires 1 byte
instead of 5 bytes to do the same thing.

AsmResolver is able to expand macros to their larger sized counterparts
and back using the `Instructions.ExpandMacros()` and
`Instructions.OptimizeMacros()`.

``` csharp
var instruction = new CilInstruction(CilOpCodes.Ldc_I4, 1);
body.Instructions.Add(instruction);

body.Instructions.OptimizeMacros();

// instruction is now optimized to "ldc.i4.1".
```

``` csharp
var instruction = new CilInstruction(CilOpCodes.Ldc_I4_1);
body.Instructions.Add(instruction);

body.Instructions.ExpandMacros();

// instruction is now expanded to "ldc.i4 1".
```

## Pretty printing CIL instructions

Instructions can be formatted using e.g. an instance of the
`CilInstructionFormatter`:

``` csharp
var formatter = new CilInstructionFormatter();
foreach (CilInstruction instruction in body.Instructions)
    Console.WriteLine(formatter.FormatInstruction(instruction));
```

## Patching CIL instructions

Instructions can be added or removed using the `Add`, `Insert`, `Remove`
and `RemoveAt` methods:

``` csharp
body.Instructions.Add(CilOpCodes.Ldstr, "Hello, world!");
body.Instructions.Insert(i, CilOpCodes.Ldc_I4, 1234);
body.Instructions.RemoveAt(i);
```

\... or by using the indexer to replace existing instructions:

``` csharp
body.Instructions[i] = new CilInstruction(CilOpCodes.Ret);
```

Removing or replacing instructions may not always be favourable. The
original `CilInstruction` object might be used as a reference for a
branch target or exception handler boundary. Removing or replacing these
`CilInstruction` objects would therefore break these kinds of
references, rendering the body invalid. Rather than updating all
references manually, it may therefore be wiser to reuse the
`CilInstruction` object and simply modify the `OpCode` and `Operand`
properties instead:

``` csharp
body.Instructions[i].OpCode = CilOpCodes.Ldc_I4;
body.Instructions[i].Operand = 1234;
```

AsmResolver provides a helper function `ReplaceWith` that shortens the
code into a single line:

``` csharp
body.Instructions[i].ReplaceWith(CilOpCodes.Ldc_I4, 1234);
```

Since it is very common to replace instructions with a
[nop]{.title-ref}, AsmResolver also defines a special `ReplaceWithNop`
helper function:

``` csharp
body.Instructions[i].ReplaceWithNop();
```

## Exception handlers

Exception handlers are regions in the method body that are protected
from exceptions. In AsmResolver, they are represented by the
`CilExceptionHandler` class, and define the following properties:

-   `HandlerType`: The type of handler.
-   `TryStart`: The label indicating the start of the protected region.
-   `TryEnd`: The label indicating the end of the protected region. This
    label is exclusive, i.e. it marks the first instruction that is not
    included in the region.
-   `HandlerStart`: The label indicating the start of the handler
    region.
-   `HandlerEnd`: The label indicating the end of the handler region.
    This label is exclusive, i.e. it marks the first instruction that is
    not included in the region.
-   `FilterStart`: The label indicating the start of the filter
    expression, if available.
-   `ExceptionType`: The type of exceptions that are caught by the
    handler.

Depending on the value of `HandlerType`, either `FilterStart` or
`ExceptionType`, or neither has a value.

> [!NOTE]
> Similar to branch instructions, when an exception handler contains a
> `null` label or a label that references an instruction that is not
> present in the method body, AsmResolver will report an exception upon
> serializing the code stream. This can be disabled by setting
> `VerifyLabelsOnBuild` to `false`.

## Maximum stack depth

CIL method bodies work with a stack, and the stack has a pre-defined
size. This pre-defined size is defined by the `MaxStack` property.

The max stack can be computed by using the `ComputeMaxStack` method. By
default, AsmResolver automatically calculates the maximum stack depth of
a method body upon writing the module to the disk. If you want to
override this behaviour, set `ComputeMaxStackOnBuild` to `false`.

> [!NOTE]
> If a `StackImbalanceException` is thrown upon writing the module to the
> disk, or upon calling `ComputeMaxStack`, it means that not all execution
> paths in the provided CIL code push or pop the expected amount of
> values. It is a good indication that the provided CIL code is invalid.
