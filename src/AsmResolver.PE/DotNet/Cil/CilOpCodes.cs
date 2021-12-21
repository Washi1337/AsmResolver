using static AsmResolver.PE.DotNet.Cil.CilOpCode;
using static AsmResolver.PE.DotNet.Cil.CilStackBehaviour;
using static AsmResolver.PE.DotNet.Cil.CilOpCodeType;
using static AsmResolver.PE.DotNet.Cil.CilOperandType;
using static AsmResolver.PE.DotNet.Cil.CilFlowControl;

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Provides members defining the entire CIL instruction set.
    /// </summary>
    /// <remarks>
    /// See also: <seealso href="https://www.ecma-international.org/wp-content/uploads/ECMA-335_6th_edition_june_2012.pdf"/>
    /// </remarks>
    public static class CilOpCodes
    {
        /// <summary>
        /// Gets a sorted list of all single-byte operation codes.
        /// </summary>
        public static readonly CilOpCode[] SingleByteOpCodes = new CilOpCode[256];
        
        /// <summary>
        /// Gets a sorted list of all multi-byte operation codes.
        /// </summary>
        public static readonly CilOpCode[] MultiByteOpCodes = new CilOpCode[256];

        /// <summary>
        /// Do nothing (No operation).
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.nop?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Nop = new CilOpCode(
            (((ushort) CilCode.Nop & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Nop >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Inform a debugger that a breakpoint has been reached.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.break?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Break = new CilOpCode(
            (((ushort) CilCode.Break & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Break >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) CilFlowControl.Break << FlowControlOffset));

        /// <summary>
        /// Load argument 0 onto the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarg_0?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldarg_0 = new CilOpCode(
            (((ushort) CilCode.Ldarg_0 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldarg_0 >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load argument 1 onto the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarg_1?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldarg_1 = new CilOpCode(
            (((ushort) CilCode.Ldarg_1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldarg_1 >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load argument 2 onto the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarg_2?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldarg_2 = new CilOpCode(
            (((ushort) CilCode.Ldarg_2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldarg_2 >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load argument 3 onto the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarg_3?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldarg_3 = new CilOpCode(
            (((ushort) CilCode.Ldarg_3 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldarg_3 >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load local variable 0 onto stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldloc_0?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldloc_0 = new CilOpCode(
            (((ushort) CilCode.Ldloc_0 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldloc_0 >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load local variable 1 onto stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldloc_1?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldloc_1 = new CilOpCode(
            (((ushort) CilCode.Ldloc_1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldloc_1 >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load local variable 2 onto stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldloc_2?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldloc_2 = new CilOpCode(
            (((ushort) CilCode.Ldloc_2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldloc_2 >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load local variable 3 onto stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldloc_3?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldloc_3 = new CilOpCode(
            (((ushort) CilCode.Ldloc_3 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldloc_3 >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Pop a value from stack into local variable 0.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stloc_0?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stloc_0 = new CilOpCode(
            (((ushort) CilCode.Stloc_0 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stloc_0 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Pop a value from stack into local variable 1.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stloc_1?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stloc_1 = new CilOpCode(
            (((ushort) CilCode.Stloc_1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stloc_1 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Pop a value from stack into local variable 2.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stloc_2?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stloc_2 = new CilOpCode(
            (((ushort) CilCode.Stloc_2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stloc_2 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Pop a value from stack into local variable 3.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stloc_3?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stloc_3 = new CilOpCode(
            (((ushort) CilCode.Stloc_3 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stloc_3 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load argument onto the stack, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarg_s?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldarg_S = new CilOpCode(
            (((ushort) CilCode.Ldarg_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldarg_S >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineArgument << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Fetch the address of argument, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarga_s?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldarga_S = new CilOpCode(
            (((ushort) CilCode.Ldarga_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldarga_S >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineArgument << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Store value to the argument numbered, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.starg_s?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Starg_S = new CilOpCode(
            (((ushort) CilCode.Starg_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Starg_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineArgument << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load local variable of index onto stack, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldloc_s?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldloc_S = new CilOpCode(
            (((ushort) CilCode.Ldloc_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldloc_S >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineVar << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load address of local variable with index, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldloca_s?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldloca_S = new CilOpCode(
            (((ushort) CilCode.Ldloca_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldloca_S >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineVar << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Pop a value from stack into local variable with index, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stloc_s?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stloc_S = new CilOpCode(
            (((ushort) CilCode.Stloc_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stloc_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineVar << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push a null reference on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldnull?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldnull = new CilOpCode(
            (((ushort) CilCode.Ldnull & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldnull >> 15) << TwoBytesOffset)
            | ((ushort) PushRef << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push -1 onto the stack as int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_m1?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldc_I4_M1 = new CilOpCode(
            (((ushort) CilCode.Ldc_I4_M1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I4_M1 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push 0 onto the stack as int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_0?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldc_I4_0 = new CilOpCode(
            (((ushort) CilCode.Ldc_I4_0 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I4_0 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push 1 onto the stack as int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_1?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldc_I4_1 = new CilOpCode(
            (((ushort) CilCode.Ldc_I4_1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I4_1 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push 2 onto the stack as int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_2?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldc_I4_2 = new CilOpCode(
            (((ushort) CilCode.Ldc_I4_2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I4_2 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push 3 onto the stack as int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_3?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldc_I4_3 = new CilOpCode(
            (((ushort) CilCode.Ldc_I4_3 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I4_3 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push 4 onto the stack as int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_4?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldc_I4_4 = new CilOpCode(
            (((ushort) CilCode.Ldc_I4_4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I4_4 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push 5 onto the stack as int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_5?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldc_I4_5 = new CilOpCode(
            (((ushort) CilCode.Ldc_I4_5 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I4_5 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push 6 onto the stack as int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_6?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldc_I4_6 = new CilOpCode(
            (((ushort) CilCode.Ldc_I4_6 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I4_6 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push 7 onto the stack as int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_7?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldc_I4_7 = new CilOpCode(
            (((ushort) CilCode.Ldc_I4_7 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I4_7 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push 8 onto the stack as int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_8?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldc_I4_8 = new CilOpCode(
            (((ushort) CilCode.Ldc_I4_8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I4_8 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push num onto the stack as int32, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_s?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldc_I4_S = new CilOpCode(
            (((ushort) CilCode.Ldc_I4_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I4_S >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineI << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push num of type int32 onto the stack as int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldc_I4 = new CilOpCode(
            (((ushort) CilCode.Ldc_I4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I4 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineI << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push num of type int64 onto the stack as int64.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i8?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldc_I8 = new CilOpCode(
            (((ushort) CilCode.Ldc_I8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I8 >> 15) << TwoBytesOffset)
            | ((ushort) PushI8 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineI8 << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push num of type float32 onto the stack as F.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_r4?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldc_R4 = new CilOpCode(
            (((ushort) CilCode.Ldc_R4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_R4 >> 15) << TwoBytesOffset)
            | ((ushort) PushR4 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) ShortInlineR << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push num of type float64 onto the stack as F.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_r8?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldc_R8 = new CilOpCode(
            (((ushort) CilCode.Ldc_R8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_R8 >> 15) << TwoBytesOffset)
            | ((ushort) PushR8 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineR << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Duplicate the value on the top of the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.dup?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Dup = new CilOpCode(
            (((ushort) CilCode.Dup & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Dup >> 15) << TwoBytesOffset)
            | ((ushort) Push1_Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Pop value from the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.pop?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Pop = new CilOpCode(
            (((ushort) CilCode.Pop & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Pop >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Exit current method and jump to the specified method.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.jmp?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Jmp = new CilOpCode(
            (((ushort) CilCode.Jmp & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Jmp >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineMethod << OperandTypeOffset)
            | ((byte) CilFlowControl.Call << FlowControlOffset));

        /// <summary>
        /// Call method described by method.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.call?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Call = new CilOpCode(
            (((ushort) CilCode.Call & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Call >> 15) << TwoBytesOffset)
            | ((ushort) VarPush << StackBehaviourPushOffset)
            | ((ushort) VarPop << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineMethod << OperandTypeOffset)
            | ((byte) CilFlowControl.Call << FlowControlOffset));

        /// <summary>
        /// Call method indicated on the stack with arguments described by a calling convention.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.calli?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Calli = new CilOpCode(
            (((ushort) CilCode.Calli & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Calli >> 15) << TwoBytesOffset)
            | ((ushort) VarPush << StackBehaviourPushOffset)
            | ((ushort) VarPop << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineSig << OperandTypeOffset)
            | ((byte) CilFlowControl.Call << FlowControlOffset));

        /// <summary>
        /// Return from method, possibly with a value.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ret?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ret = new CilOpCode(
            (((ushort) CilCode.Ret & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ret >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) VarPop << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Return << FlowControlOffset));

        /// <summary>
        /// Branch to target, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.br_s?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Br_S = new CilOpCode(
            (((ushort) CilCode.Br_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Br_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) Branch << FlowControlOffset));

        /// <summary>
        /// Branch to target if value is zero (false), short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.brfalse_s?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Brfalse_S = new CilOpCode(
            (((ushort) CilCode.Brfalse_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Brfalse_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Branch to target if value is non-zero (true), short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.brtrue_s?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Brtrue_S = new CilOpCode(
            (((ushort) CilCode.Brtrue_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Brtrue_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Branch to target if equal, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.beq_s?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Beq_S = new CilOpCode(
            (((ushort) CilCode.Beq_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Beq_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Branch to target if greater than or equal to, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.bge_s?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Bge_S = new CilOpCode(
            (((ushort) CilCode.Bge_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Bge_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Branch to target if greater than, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.bgt_s?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Bgt_S = new CilOpCode(
            (((ushort) CilCode.Bgt_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Bgt_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Branch to target if less than or equal to, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ble_s?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ble_S = new CilOpCode(
            (((ushort) CilCode.Ble_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ble_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Branch to target if less than, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.blt_s?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Blt_S = new CilOpCode(
            (((ushort) CilCode.Blt_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Blt_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Branch to target if unequal or unordered, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.bne_un_s?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Bne_Un_S = new CilOpCode(
            (((ushort) CilCode.Bne_Un_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Bne_Un_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Branch to target if greater than or equal to (unsigned or unordered), short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.bge_un_s?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Bge_Un_S = new CilOpCode(
            (((ushort) CilCode.Bge_Un_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Bge_Un_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Branch to target if greater than (unsigned or unordered), short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.bgt_un_s?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Bgt_Un_S = new CilOpCode(
            (((ushort) CilCode.Bgt_Un_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Bgt_Un_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Branch to target if less than or equal to (unsigned or unordered), short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ble_un_s?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ble_Un_S = new CilOpCode(
            (((ushort) CilCode.Ble_Un_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ble_Un_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Branch to target if less than (unsigned or unordered), short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.blt_un_s?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Blt_Un_S = new CilOpCode(
            (((ushort) CilCode.Blt_Un_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Blt_Un_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Branch to target.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.br?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Br = new CilOpCode(
            (((ushort) CilCode.Br & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Br >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) Branch << FlowControlOffset));

        /// <summary>
        /// Branch to target if value is zero (false).
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.brfalse?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Brfalse = new CilOpCode(
            (((ushort) CilCode.Brfalse & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Brfalse >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Branch to target if value is non-zero (true).
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.brtrue?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Brtrue = new CilOpCode(
            (((ushort) CilCode.Brtrue & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Brtrue >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Branch to target if equal.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.beq?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Beq = new CilOpCode(
            (((ushort) CilCode.Beq & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Beq >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Branch to target if greater than or equal to.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.bge?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Bge = new CilOpCode(
            (((ushort) CilCode.Bge & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Bge >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Branch to target if greater than.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.bgt?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Bgt = new CilOpCode(
            (((ushort) CilCode.Bgt & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Bgt >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Branch to target if less than or equal to.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ble?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ble = new CilOpCode(
            (((ushort) CilCode.Ble & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ble >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Branch to target if less than.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.blt?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Blt = new CilOpCode(
            (((ushort) CilCode.Blt & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Blt >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Branch to target if unequal or unordered.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.bne_un?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Bne_Un = new CilOpCode(
            (((ushort) CilCode.Bne_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Bne_Un >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Branch to target if greater than or equal to (unsigned or unordered).
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.bge_un?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Bge_Un = new CilOpCode(
            (((ushort) CilCode.Bge_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Bge_Un >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Branch to target if greater than (unsigned or unordered).
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.bgt_un?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Bgt_Un = new CilOpCode(
            (((ushort) CilCode.Bgt_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Bgt_Un >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Branch to target if less than or equal to (unsigned or unordered).
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ble_un?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ble_Un = new CilOpCode(
            (((ushort) CilCode.Ble_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ble_Un >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Branch to target if less than (unsigned or unordered).
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.blt_un?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Blt_Un = new CilOpCode(
            (((ushort) CilCode.Blt_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Blt_Un >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Jump to one of n values.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.switch?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Switch = new CilOpCode(
            (((ushort) CilCode.Switch & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Switch >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineSwitch << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        /// <summary>
        /// Indirect load value of type int8 as int32 on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldind_i1?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldind_I1 = new CilOpCode(
            (((ushort) CilCode.Ldind_I1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldind_I1 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Indirect load value of type unsigned int8 as int32 on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldind_u1?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldind_U1 = new CilOpCode(
            (((ushort) CilCode.Ldind_U1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldind_U1 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Indirect load value of type int16 as int32 on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldind_i2?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldind_I2 = new CilOpCode(
            (((ushort) CilCode.Ldind_I2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldind_I2 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Indirect load value of type unsigned int16 as int32 on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldind_u2?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldind_U2 = new CilOpCode(
            (((ushort) CilCode.Ldind_U2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldind_U2 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Indirect load value of type int32 as int32 on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldind_i4?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldind_I4 = new CilOpCode(
            (((ushort) CilCode.Ldind_I4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldind_I4 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Indirect load value of type unsigned int32 as int32 on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldind_u4?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldind_U4 = new CilOpCode(
            (((ushort) CilCode.Ldind_U4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldind_U4 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Indirect load value of type signed or unsigned int64 as signed int64 on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldind_i8?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldind_I8 = new CilOpCode(
            (((ushort) CilCode.Ldind_I8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldind_I8 >> 15) << TwoBytesOffset)
            | ((ushort) PushI8 << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Indirect load value of type native int as native int on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldind_i?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldind_I = new CilOpCode(
            (((ushort) CilCode.Ldind_I & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldind_I >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Indirect load value of type float32 as F on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldind_r4?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldind_R4 = new CilOpCode(
            (((ushort) CilCode.Ldind_R4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldind_R4 >> 15) << TwoBytesOffset)
            | ((ushort) PushR4 << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Indirect load value of type float64 as F on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldind_r8?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldind_R8 = new CilOpCode(
            (((ushort) CilCode.Ldind_R8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldind_R8 >> 15) << TwoBytesOffset)
            | ((ushort) PushR8 << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Indirect load value of type object ref as O on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldind_ref?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldind_Ref = new CilOpCode(
            (((ushort) CilCode.Ldind_Ref & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldind_Ref >> 15) << TwoBytesOffset)
            | ((ushort) PushRef << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Store value of type object ref (type O) into memory at address.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stind_ref?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stind_Ref = new CilOpCode(
            (((ushort) CilCode.Stind_Ref & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stind_Ref >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI_PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Store value of type int8 into memory at address.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stind_i1?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stind_I1 = new CilOpCode(
            (((ushort) CilCode.Stind_I1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stind_I1 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI_PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Store value of type int16 into memory at address.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stind_i2?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stind_I2 = new CilOpCode(
            (((ushort) CilCode.Stind_I2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stind_I2 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI_PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Store value of type int32 into memory at address.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stind_i4?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stind_I4 = new CilOpCode(
            (((ushort) CilCode.Stind_I4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stind_I4 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI_PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Store value of type int64 into memory at address.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stind_i8?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stind_I8 = new CilOpCode(
            (((ushort) CilCode.Stind_I8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stind_I8 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI_PopI8 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Store value of type float32 into memory at address.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stind_r4?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stind_R4 = new CilOpCode(
            (((ushort) CilCode.Stind_R4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stind_R4 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI_PopR4 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Store value of type float64 into memory at address.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stind_r8?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stind_R8 = new CilOpCode(
            (((ushort) CilCode.Stind_R8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stind_R8 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI_PopR8 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Add two values, returning a new value.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.add?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Add = new CilOpCode(
            (((ushort) CilCode.Add & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Add >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Subtract value2 from value1, returning a new value.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.sub?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Sub = new CilOpCode(
            (((ushort) CilCode.Sub & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Sub >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Multiply values.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.mul?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Mul = new CilOpCode(
            (((ushort) CilCode.Mul & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Mul >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Divide two values to return a quotient or floating-point result.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.div?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Div = new CilOpCode(
            (((ushort) CilCode.Div & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Div >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Divide two values, unsigned, returning a quotient.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.div_un?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Div_Un = new CilOpCode(
            (((ushort) CilCode.Div_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Div_Un >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Remainder when dividing one value by another.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.rem?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Rem = new CilOpCode(
            (((ushort) CilCode.Rem & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Rem >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Remainder when dividing one unsigned value by another.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.rem_un?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Rem_Un = new CilOpCode(
            (((ushort) CilCode.Rem_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Rem_Un >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Bitwise AND of two integral values, returns an integral value.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.and?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode And = new CilOpCode(
            (((ushort) CilCode.And & 0xFF) << ValueOffset)
            | (((ushort) CilCode.And >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Bitwise OR of two integer values, returns an integer.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.or?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Or = new CilOpCode(
            (((ushort) CilCode.Or & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Or >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Bitwise XOR of integer values, returns an integer.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.xor?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Xor = new CilOpCode(
            (((ushort) CilCode.Xor & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Xor >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Shift an integer left (shifting in zeros), return an integer.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.shl?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Shl = new CilOpCode(
            (((ushort) CilCode.Shl & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Shl >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Shift an integer right (shift in sign), return an integer.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.shr?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Shr = new CilOpCode(
            (((ushort) CilCode.Shr & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Shr >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Shift an integer right (shift in zero), return an integer.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.shr_un?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Shr_Un = new CilOpCode(
            (((ushort) CilCode.Shr_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Shr_Un >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Negate value.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.neg?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Neg = new CilOpCode(
            (((ushort) CilCode.Neg & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Neg >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Bitwise complement (logical not).
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.not?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Not = new CilOpCode(
            (((ushort) CilCode.Not & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Not >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert to int8, pushing int32 on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_i1?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_I1 = new CilOpCode(
            (((ushort) CilCode.Conv_I1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_I1 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert to int16, pushing int32 on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_i2?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_I2 = new CilOpCode(
            (((ushort) CilCode.Conv_I2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_I2 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert to int32, pushing int32 on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_i4?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_I4 = new CilOpCode(
            (((ushort) CilCode.Conv_I4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_I4 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert to int64, pushing int64 on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_i8?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_I8 = new CilOpCode(
            (((ushort) CilCode.Conv_I8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_I8 >> 15) << TwoBytesOffset)
            | ((ushort) PushI8 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert to float32, pushing F on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_r4?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_R4 = new CilOpCode(
            (((ushort) CilCode.Conv_R4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_R4 >> 15) << TwoBytesOffset)
            | ((ushort) PushR4 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert to float64, pushing F on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_r8?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_R8 = new CilOpCode(
            (((ushort) CilCode.Conv_R8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_R8 >> 15) << TwoBytesOffset)
            | ((ushort) PushR8 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert to unsigned int32, pushing int32 on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_u4?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_U4 = new CilOpCode(
            (((ushort) CilCode.Conv_U4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_U4 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert to unsigned int64, pushing int64 on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_u8?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_U8 = new CilOpCode(
            (((ushort) CilCode.Conv_U8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_U8 >> 15) << TwoBytesOffset)
            | ((ushort) PushI8 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Call a method associated with an object.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.callvirt?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Callvirt = new CilOpCode(
            (((ushort) CilCode.Callvirt & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Callvirt >> 15) << TwoBytesOffset)
            | ((ushort) VarPush << StackBehaviourPushOffset)
            | ((ushort) VarPop << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineMethod << OperandTypeOffset)
            | ((byte) CilFlowControl.Call << FlowControlOffset));

        /// <summary>
        /// Copy a value type from src to dest.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.cpobj?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Cpobj = new CilOpCode(
            (((ushort) CilCode.Cpobj & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Cpobj >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Copy the value stored at address src to the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldobj?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldobj = new CilOpCode(
            (((ushort) CilCode.Ldobj & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldobj >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push a string object for the literal string.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldstr?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldstr = new CilOpCode(
            (((ushort) CilCode.Ldstr & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldstr >> 15) << TwoBytesOffset)
            | ((ushort) PushRef << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineString << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Allocate an uninitialized object or value type and call ctor.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.newobj?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Newobj = new CilOpCode(
            (((ushort) CilCode.Newobj & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Newobj >> 15) << TwoBytesOffset)
            | ((ushort) PushRef << StackBehaviourPushOffset)
            | ((ushort) VarPop << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineMethod << OperandTypeOffset)
            | ((byte) CilFlowControl.Call << FlowControlOffset));

        /// <summary>
        /// Cast obj to class.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.castclass?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Castclass = new CilOpCode(
            (((ushort) CilCode.Castclass & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Castclass >> 15) << TwoBytesOffset)
            | ((ushort) PushRef << StackBehaviourPushOffset)
            | ((ushort) PopRef << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Test if obj is an instance of class, returning null or an instance of that class or interface.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.isinst?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Isinst = new CilOpCode(
            (((ushort) CilCode.Isinst & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Isinst >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert unsigned integer to floating-point, pushing F on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_r_un?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_R_Un = new CilOpCode(
            (((ushort) CilCode.Conv_R_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_R_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushR8 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Extract a value-type from obj, its boxed representation, and push a controlled-mutability managed pointer to it to the top of the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.unbox?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Unbox = new CilOpCode(
            (((ushort) CilCode.Unbox & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Unbox >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Throw an exception.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.throw?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Throw = new CilOpCode(
            (((ushort) CilCode.Throw & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Throw >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopRef << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) CilFlowControl.Throw << FlowControlOffset));

        /// <summary>
        /// Push the value of field of object (or value type) obj, onto the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldfld?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldfld = new CilOpCode(
            (((ushort) CilCode.Ldfld & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldfld >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) PopRef << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineField << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push the address of field of object obj on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldflda?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldflda = new CilOpCode(
            (((ushort) CilCode.Ldflda & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldflda >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineField << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Replace the value of field of the object obj with value.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stfld?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stfld = new CilOpCode(
            (((ushort) CilCode.Stfld & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stfld >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopRef_Pop1 << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineField << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push the value of the static field on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldsfld?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldsfld = new CilOpCode(
            (((ushort) CilCode.Ldsfld & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldsfld >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineField << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push the address of the static field, field, on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldsflda?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldsflda = new CilOpCode(
            (((ushort) CilCode.Ldsflda & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldsflda >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineField << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Replace the value of the static field.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stsfld?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stsfld = new CilOpCode(
            (((ushort) CilCode.Stsfld & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stsfld >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineField << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Store a value at an address.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stobj?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stobj = new CilOpCode(
            (((ushort) CilCode.Stobj & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stobj >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert unsigned to an int8 (on the stack as int32) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_i1_un?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_Ovf_I1_Un = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_I1_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_I1_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert unsigned to an int16 (on the stack as int32) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_i2_un?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_Ovf_I2_Un = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_I2_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_I2_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert unsigned to an int32 (on the stack as int32) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_i4_un?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_Ovf_I4_Un = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_I4_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_I4_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert unsigned to an int64 (on the stack as int64) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_i8_un?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_Ovf_I8_Un = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_I8_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_I8_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushI8 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert unsigned to an unsigned int8 (on the stack as int32) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_u1_un?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_Ovf_U1_Un = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_U1_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_U1_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert unsigned to an unsigned int16 (on the stack as int32) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_u2_un?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_Ovf_U2_Un = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_U2_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_U2_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert unsigned to an unsigned int32 (on the stack as int32) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_u4_un?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_Ovf_U4_Un = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_U4_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_U4_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert unsigned to an unsigned int64 (on the stack as int64) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_u8_un?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_Ovf_U8_Un = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_U8_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_U8_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushI8 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert unsigned to a native int (on the stack as native int) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_i_un?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_Ovf_I_Un = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_I_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_I_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert unsigned to a native unsigned int (on the stack as native int) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_u_un?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_Ovf_U_Un = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_U_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_U_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert a boxable value to its boxed form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.box?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Box = new CilOpCode(
            (((ushort) CilCode.Box & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Box >> 15) << TwoBytesOffset)
            | ((ushort) PushRef << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Create a new array with elements of type etype.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.newarr?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Newarr = new CilOpCode(
            (((ushort) CilCode.Newarr & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Newarr >> 15) << TwoBytesOffset)
            | ((ushort) PushRef << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push the length (of type native unsigned int) of array on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldlen?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldlen = new CilOpCode(
            (((ushort) CilCode.Ldlen & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldlen >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load the address of element at index onto the top of the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelema?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldelema = new CilOpCode(
            (((ushort) CilCode.Ldelema & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelema >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load the element with type int8 at index onto the top of the stack as an int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelem_i1?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldelem_I1 = new CilOpCode(
            (((ushort) CilCode.Ldelem_I1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelem_I1 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load the element with type unsigned int8 at index onto the top of the stack as an int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelem_u1?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldelem_U1 = new CilOpCode(
            (((ushort) CilCode.Ldelem_U1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelem_U1 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load the element with type int16 at index onto the top of the stack as an int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelem_i2?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldelem_I2 = new CilOpCode(
            (((ushort) CilCode.Ldelem_I2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelem_I2 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load the element with type unsigned int16 at index onto the top of the stack as an int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelem_u2?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldelem_U2 = new CilOpCode(
            (((ushort) CilCode.Ldelem_U2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelem_U2 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load the element with type int32 at index onto the top of the stack as an int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelem_i4?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldelem_I4 = new CilOpCode(
            (((ushort) CilCode.Ldelem_I4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelem_I4 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load the element with type unsigned int32 at index onto the top of the stack as an int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelem_u4?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldelem_U4 = new CilOpCode(
            (((ushort) CilCode.Ldelem_U4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelem_U4 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load the element with type signed or unsigned int64 at index onto the top of the stack as a signed int64.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelem_i8?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldelem_I8 = new CilOpCode(
            (((ushort) CilCode.Ldelem_I8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelem_I8 >> 15) << TwoBytesOffset)
            | ((ushort) PushI8 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load the element with type native int at index onto the top of the stack as a native int.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelem_i?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldelem_I = new CilOpCode(
            (((ushort) CilCode.Ldelem_I & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelem_I >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load the element with type float32 at index onto the top of the stack as an F.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelem_r4?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldelem_R4 = new CilOpCode(
            (((ushort) CilCode.Ldelem_R4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelem_R4 >> 15) << TwoBytesOffset)
            | ((ushort) PushR4 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load the element with type float64 at index onto the top of the stack as an F.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelem_r8?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldelem_R8 = new CilOpCode(
            (((ushort) CilCode.Ldelem_R8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelem_R8 >> 15) << TwoBytesOffset)
            | ((ushort) PushR8 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load the element at index onto the top of the stack as an O. The type of the O is the same as the element type of the array pushed on the CIL stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelem_ref?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldelem_Ref = new CilOpCode(
            (((ushort) CilCode.Ldelem_Ref & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelem_Ref >> 15) << TwoBytesOffset)
            | ((ushort) PushRef << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Replace array element at index with the i value on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stelem_i?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stelem_I = new CilOpCode(
            (((ushort) CilCode.Stelem_I & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stelem_I >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Replace array element at index with the int8 value on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stelem_i1?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stelem_I1 = new CilOpCode(
            (((ushort) CilCode.Stelem_I1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stelem_I1 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Replace array element at index with the int16 value on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stelem_i2?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stelem_I2 = new CilOpCode(
            (((ushort) CilCode.Stelem_I2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stelem_I2 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Replace array element at index with the int32 value on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stelem_i4?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stelem_I4 = new CilOpCode(
            (((ushort) CilCode.Stelem_I4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stelem_I4 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Replace array element at index with the int64 value on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stelem_i8?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stelem_I8 = new CilOpCode(
            (((ushort) CilCode.Stelem_I8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stelem_I8 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI_PopI8 << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Replace array element at index with the float32 value on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stelem_r4?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stelem_R4 = new CilOpCode(
            (((ushort) CilCode.Stelem_R4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stelem_R4 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI_PopR4 << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Replace array element at index with the float64 value on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stelem_r8?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stelem_R8 = new CilOpCode(
            (((ushort) CilCode.Stelem_R8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stelem_R8 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI_PopR8 << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Replace array element at index with the ref value on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stelem_ref?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stelem_Ref = new CilOpCode(
            (((ushort) CilCode.Stelem_Ref & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stelem_Ref >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI_PopRef << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load the element at index onto the top of the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelem?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldelem = new CilOpCode(
            (((ushort) CilCode.Ldelem & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelem >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Replace array element at index with the value on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stelem?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stelem = new CilOpCode(
            (((ushort) CilCode.Stelem & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stelem >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI_Pop1 << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Extract a value-type from obj, its boxed representation, and copy to the top of the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.unbox_any?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Unbox_Any = new CilOpCode(
            (((ushort) CilCode.Unbox_Any & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Unbox_Any >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) PopRef << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert to an int8 (on the stack as int32) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_i1?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_Ovf_I1 = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_I1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_I1 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert to an unsigned int8 (on the stack as int32) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_u1?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_Ovf_U1 = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_U1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_U1 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert to an int16 (on the stack as int32) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_i2?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_Ovf_I2 = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_I2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_I2 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert to an unsigned int16 (on the stack as int32) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_u2?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_Ovf_U2 = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_U2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_U2 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert to an int32 (on the stack as int32) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_i4?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_Ovf_I4 = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_I4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_I4 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert to an unsigned int32 (on the stack as int32) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_u4?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_Ovf_U4 = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_U4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_U4 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert to an int64 (on the stack as int64) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_i8?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_Ovf_I8 = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_I8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_I8 >> 15) << TwoBytesOffset)
            | ((ushort) PushI8 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert to an unsigned int64 (on the stack as int64) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_u8?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_Ovf_U8 = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_U8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_U8 >> 15) << TwoBytesOffset)
            | ((ushort) PushI8 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push the address stored in a typed reference.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.refanyval?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Refanyval = new CilOpCode(
            (((ushort) CilCode.Refanyval & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Refanyval >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Throw ArithmeticException if value is not a finite number.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ckfinite?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ckfinite = new CilOpCode(
            (((ushort) CilCode.Ckfinite & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ckfinite >> 15) << TwoBytesOffset)
            | ((ushort) PushR8 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push a typed reference to ptr of type class onto the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.mkrefany?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Mkrefany = new CilOpCode(
            (((ushort) CilCode.Mkrefany & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Mkrefany >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert metadata token to its runtime representation.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldtoken?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldtoken = new CilOpCode(
            (((ushort) CilCode.Ldtoken & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldtoken >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineTok << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert to unsigned int16, pushing int32 on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_u2?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_U2 = new CilOpCode(
            (((ushort) CilCode.Conv_U2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_U2 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert to unsigned int8, pushing int32 on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_u1?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_U1 = new CilOpCode(
            (((ushort) CilCode.Conv_U1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_U1 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert to native int, pushing native int on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_i?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_I = new CilOpCode(
            (((ushort) CilCode.Conv_I & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_I >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert to a native int (on the stack as native int) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_i?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_Ovf_I = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_I & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_I >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert to a native unsigned int (on the stack as native int) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_u?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_Ovf_U = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_U & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_U >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Add signed integer values with overflow check.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.add_ovf?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Add_Ovf = new CilOpCode(
            (((ushort) CilCode.Add_Ovf & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Add_Ovf >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Add unsigned integer values with overflow check.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.add_ovf_un?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Add_Ovf_Un = new CilOpCode(
            (((ushort) CilCode.Add_Ovf_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Add_Ovf_Un >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Multiply signed integer values. Signed result shall fit in same size.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.mul_ovf?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Mul_Ovf = new CilOpCode(
            (((ushort) CilCode.Mul_Ovf & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Mul_Ovf >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Multiply unsigned integer values. Unsigned result shall fit in same size.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.mul_ovf_un?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Mul_Ovf_Un = new CilOpCode(
            (((ushort) CilCode.Mul_Ovf_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Mul_Ovf_Un >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Subtract native int from a native int. Signed result shall fit in same size.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.sub_ovf?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Sub_Ovf = new CilOpCode(
            (((ushort) CilCode.Sub_Ovf & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Sub_Ovf >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Subtract native unsigned int from a native unsigned int. Unsigned result shall fit in same size.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.sub_ovf_un?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Sub_Ovf_Un = new CilOpCode(
            (((ushort) CilCode.Sub_Ovf_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Sub_Ovf_Un >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// End finally clause of an exception block.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.endfinally?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Endfinally = new CilOpCode(
            (((ushort) CilCode.Endfinally & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Endfinally >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Return << FlowControlOffset));

        /// <summary>
        /// Exit a protected region of code.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.leave?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Leave = new CilOpCode(
            (((ushort) CilCode.Leave & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Leave >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopAll << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) Branch << FlowControlOffset));

        /// <summary>
        /// Exit a protected region of code, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.leave_s?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Leave_S = new CilOpCode(
            (((ushort) CilCode.Leave_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Leave_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopAll << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) Branch << FlowControlOffset));

        /// <summary>
        /// Store value of type native int into memory at address.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stind_i?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stind_I = new CilOpCode(
            (((ushort) CilCode.Stind_I & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stind_I >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI_PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Convert to native unsigned int, pushing native int on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_u?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Conv_U = new CilOpCode(
            (((ushort) CilCode.Conv_U & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_U >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// This prefix opcode is reserved and currently not implemented in the runtime 
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.prefix7?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Prefix7 = new CilOpCode(
            (((ushort) CilCode.Prefix7 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Prefix7 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Internal << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

        /// <summary>
        /// This prefix opcode is reserved and currently not implemented in the runtime 
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.prefix6?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Prefix6 = new CilOpCode(
            (((ushort) CilCode.Prefix6 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Prefix6 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Internal << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

        /// <summary>
        /// This prefix opcode is reserved and currently not implemented in the runtime 
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.prefix5?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Prefix5 = new CilOpCode(
            (((ushort) CilCode.Prefix5 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Prefix5 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Internal << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

        /// <summary>
        /// This prefix opcode is reserved and currently not implemented in the runtime 
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.prefix4?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Prefix4 = new CilOpCode(
            (((ushort) CilCode.Prefix4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Prefix4 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Internal << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

        /// <summary>
        /// This prefix opcode is reserved and currently not implemented in the runtime 
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.prefix3?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Prefix3 = new CilOpCode(
            (((ushort) CilCode.Prefix3 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Prefix3 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Internal << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

        /// <summary>
        /// This prefix opcode is reserved and currently not implemented in the runtime 
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.prefix2?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Prefix2 = new CilOpCode(
            (((ushort) CilCode.Prefix2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Prefix2 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Internal << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

        /// <summary>
        /// This prefix opcode is reserved and currently not implemented in the runtime 
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.prefix1?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Prefix1 = new CilOpCode(
            (((ushort) CilCode.Prefix1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Prefix1 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Internal << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

        /// <summary>
        /// This prefix opcode is reserved and currently not implemented in the runtime 
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.prefixref?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Prefixref = new CilOpCode(
            (((ushort) CilCode.Prefixref & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Prefixref >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Internal << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

        /// <summary>
        /// Return argument list handle for the current method.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.arglist?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Arglist = new CilOpCode(
            (((ushort) CilCode.Arglist & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Arglist >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push 1 (of type int32) if value1 equals value2, else push 0.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ceq?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ceq = new CilOpCode(
            (((ushort) CilCode.Ceq & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ceq >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push 1 (of type int32) if value1 greater that value2, else push 0.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.cgt?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Cgt = new CilOpCode(
            (((ushort) CilCode.Cgt & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Cgt >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push 1 (of type int32) if value1 greater that value2, unsigned or unordered, else push 0.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.cgt_un?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Cgt_Un = new CilOpCode(
            (((ushort) CilCode.Cgt_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Cgt_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push 1 (of type int32) if value1 lower than value2, else push 0.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.clt?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Clt = new CilOpCode(
            (((ushort) CilCode.Clt & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Clt >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push 1 (of type int32) if value1 lower than value2, unsigned or unordered, else push 0.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.clt_un?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Clt_Un = new CilOpCode(
            (((ushort) CilCode.Clt_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Clt_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push a pointer to a method referenced by method, on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldftn?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldftn = new CilOpCode(
            (((ushort) CilCode.Ldftn & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldftn >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineMethod << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push address of virtual method on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldvirtftn?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldvirtftn = new CilOpCode(
            (((ushort) CilCode.Ldvirtftn & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldvirtftn >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineMethod << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load argument onto the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarg?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldarg = new CilOpCode(
            (((ushort) CilCode.Ldarg & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldarg >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineArgument << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Fetch the address of the argument indexed.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarga?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldarga = new CilOpCode(
            (((ushort) CilCode.Ldarga & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldarga >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineArgument << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Store value to the argument.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.starg?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Starg = new CilOpCode(
            (((ushort) CilCode.Starg & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Starg >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineArgument << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load local variable of index onto stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldloc?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldloc = new CilOpCode(
            (((ushort) CilCode.Ldloc & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldloc >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineVar << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Load address of local variable with index index.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldloca?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Ldloca = new CilOpCode(
            (((ushort) CilCode.Ldloca & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldloca >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineVar << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Pop a value from stack into local variable index.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stloc?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Stloc = new CilOpCode(
            (((ushort) CilCode.Stloc & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stloc >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineVar << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Allocate space from the local memory pool.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.localloc?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Localloc = new CilOpCode(
            (((ushort) CilCode.Localloc & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Localloc >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// End an exception handling filter clause.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.endfilter?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Endfilter = new CilOpCode(
            (((ushort) CilCode.Endfilter & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Endfilter >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Return << FlowControlOffset));

        /// <summary>
        /// Subsequent pointer instruction might be unaligned.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.unaligned?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Unaligned = new CilOpCode(
            (((ushort) CilCode.Unaligned & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Unaligned >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Prefix << OpCodeTypeOffset)
            | ((byte) ShortInlineI << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

        /// <summary>
        /// Subsequent pointer reference is volatile.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.volatile?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Volatile = new CilOpCode(
            (((ushort) CilCode.Volatile & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Volatile >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Prefix << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

        /// <summary>
        /// Subsequent call terminates current method.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.tailcall?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Tailcall = new CilOpCode(
            (((ushort) CilCode.Tailcall & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Tailcall >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Prefix << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

        /// <summary>
        /// Initialize the value at address dest.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.initobj?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Initobj = new CilOpCode(
            (((ushort) CilCode.Initobj & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Initobj >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Call a virtual method on a type constrained to be type T.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.constrained?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Constrained = new CilOpCode(
            (((ushort) CilCode.Constrained & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Constrained >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Prefix << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

        /// <summary>
        /// Copy data from memory to memory.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.cpblk?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Cpblk = new CilOpCode(
            (((ushort) CilCode.Cpblk & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Cpblk >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI_PopI_PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Set all bytes in a block of memory to a given byte value.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.initblk?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Initblk = new CilOpCode(
            (((ushort) CilCode.Initblk & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Initblk >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI_PopI_PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Rethrow the current exception.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.rethrow?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Rethrow = new CilOpCode(
            (((ushort) CilCode.Rethrow & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Rethrow >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) CilFlowControl.Throw << FlowControlOffset));

        /// <summary>
        /// Push the size, in bytes, of a type as an unsigned int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.sizeof?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Sizeof = new CilOpCode(
            (((ushort) CilCode.Sizeof & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Sizeof >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Push the type token stored in a typed reference.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.refanytype?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Refanytype = new CilOpCode(
            (((ushort) CilCode.Refanytype & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Refanytype >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        /// <summary>
        /// Specify that the subsequent array address operation performs no type check at runtime, and that it returns a controlled-mutability managed pointer.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.readonly?view=net-6.0"/>
        /// </remarks>
        public static readonly CilOpCode Readonly = new CilOpCode(
            (((ushort) CilCode.Readonly & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Readonly >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Prefix << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

    }
}
