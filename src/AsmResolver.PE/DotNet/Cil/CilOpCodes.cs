using static AsmResolver.PE.DotNet.Cil.CilOpCode;
using static AsmResolver.PE.DotNet.Cil.CilStackBehaviour;
using static AsmResolver.PE.DotNet.Cil.CilOpCodeType;
using static AsmResolver.PE.DotNet.Cil.CilOperandType;
using static AsmResolver.PE.DotNet.Cil.CilFlowControl;

// Disable missing XML doc warnings.
#pragma warning disable 1591

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Provides members defining the entire CIL instruction set.
    /// </summary>
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
       
        public static readonly CilOpCode Nop = new CilOpCode(
            (((ushort) CilCode.Nop & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Nop >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Break = new CilOpCode(
            (((ushort) CilCode.Break & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Break >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) CilFlowControl.Break << FlowControlOffset));

        public static readonly CilOpCode Ldarg_0 = new CilOpCode(
            (((ushort) CilCode.Ldarg_0 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldarg_0 >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldarg_1 = new CilOpCode(
            (((ushort) CilCode.Ldarg_1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldarg_1 >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldarg_2 = new CilOpCode(
            (((ushort) CilCode.Ldarg_2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldarg_2 >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldarg_3 = new CilOpCode(
            (((ushort) CilCode.Ldarg_3 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldarg_3 >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldloc_0 = new CilOpCode(
            (((ushort) CilCode.Ldloc_0 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldloc_0 >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldloc_1 = new CilOpCode(
            (((ushort) CilCode.Ldloc_1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldloc_1 >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldloc_2 = new CilOpCode(
            (((ushort) CilCode.Ldloc_2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldloc_2 >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldloc_3 = new CilOpCode(
            (((ushort) CilCode.Ldloc_3 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldloc_3 >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stloc_0 = new CilOpCode(
            (((ushort) CilCode.Stloc_0 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stloc_0 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stloc_1 = new CilOpCode(
            (((ushort) CilCode.Stloc_1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stloc_1 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stloc_2 = new CilOpCode(
            (((ushort) CilCode.Stloc_2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stloc_2 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stloc_3 = new CilOpCode(
            (((ushort) CilCode.Stloc_3 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stloc_3 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldarg_S = new CilOpCode(
            (((ushort) CilCode.Ldarg_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldarg_S >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineArgument << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldarga_S = new CilOpCode(
            (((ushort) CilCode.Ldarga_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldarga_S >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineArgument << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Starg_S = new CilOpCode(
            (((ushort) CilCode.Starg_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Starg_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineArgument << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldloc_S = new CilOpCode(
            (((ushort) CilCode.Ldloc_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldloc_S >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineVar << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldloca_S = new CilOpCode(
            (((ushort) CilCode.Ldloca_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldloca_S >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineVar << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stloc_S = new CilOpCode(
            (((ushort) CilCode.Stloc_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stloc_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineVar << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldnull = new CilOpCode(
            (((ushort) CilCode.Ldnull & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldnull >> 15) << TwoBytesOffset)
            | ((ushort) PushRef << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldc_I4_M1 = new CilOpCode(
            (((ushort) CilCode.Ldc_I4_M1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I4_M1 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldc_I4_0 = new CilOpCode(
            (((ushort) CilCode.Ldc_I4_0 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I4_0 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldc_I4_1 = new CilOpCode(
            (((ushort) CilCode.Ldc_I4_1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I4_1 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldc_I4_2 = new CilOpCode(
            (((ushort) CilCode.Ldc_I4_2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I4_2 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldc_I4_3 = new CilOpCode(
            (((ushort) CilCode.Ldc_I4_3 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I4_3 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldc_I4_4 = new CilOpCode(
            (((ushort) CilCode.Ldc_I4_4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I4_4 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldc_I4_5 = new CilOpCode(
            (((ushort) CilCode.Ldc_I4_5 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I4_5 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldc_I4_6 = new CilOpCode(
            (((ushort) CilCode.Ldc_I4_6 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I4_6 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldc_I4_7 = new CilOpCode(
            (((ushort) CilCode.Ldc_I4_7 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I4_7 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldc_I4_8 = new CilOpCode(
            (((ushort) CilCode.Ldc_I4_8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I4_8 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldc_I4_S = new CilOpCode(
            (((ushort) CilCode.Ldc_I4_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I4_S >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineI << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldc_I4 = new CilOpCode(
            (((ushort) CilCode.Ldc_I4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I4 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineI << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldc_I8 = new CilOpCode(
            (((ushort) CilCode.Ldc_I8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_I8 >> 15) << TwoBytesOffset)
            | ((ushort) PushI8 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineI8 << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldc_R4 = new CilOpCode(
            (((ushort) CilCode.Ldc_R4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_R4 >> 15) << TwoBytesOffset)
            | ((ushort) PushR4 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) ShortInlineR << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldc_R8 = new CilOpCode(
            (((ushort) CilCode.Ldc_R8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldc_R8 >> 15) << TwoBytesOffset)
            | ((ushort) PushR8 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineR << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Dup = new CilOpCode(
            (((ushort) CilCode.Dup & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Dup >> 15) << TwoBytesOffset)
            | ((ushort) Push1_Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Pop = new CilOpCode(
            (((ushort) CilCode.Pop & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Pop >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Jmp = new CilOpCode(
            (((ushort) CilCode.Jmp & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Jmp >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineMethod << OperandTypeOffset)
            | ((byte) CilFlowControl.Call << FlowControlOffset));

        public static readonly CilOpCode Call = new CilOpCode(
            (((ushort) CilCode.Call & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Call >> 15) << TwoBytesOffset)
            | ((ushort) VarPush << StackBehaviourPushOffset)
            | ((ushort) VarPop << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineMethod << OperandTypeOffset)
            | ((byte) CilFlowControl.Call << FlowControlOffset));

        public static readonly CilOpCode Calli = new CilOpCode(
            (((ushort) CilCode.Calli & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Calli >> 15) << TwoBytesOffset)
            | ((ushort) VarPush << StackBehaviourPushOffset)
            | ((ushort) VarPop << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineSig << OperandTypeOffset)
            | ((byte) CilFlowControl.Call << FlowControlOffset));

        public static readonly CilOpCode Ret = new CilOpCode(
            (((ushort) CilCode.Ret & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ret >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) VarPop << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Return << FlowControlOffset));

        public static readonly CilOpCode Br_S = new CilOpCode(
            (((ushort) CilCode.Br_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Br_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) Branch << FlowControlOffset));

        public static readonly CilOpCode Brfalse_S = new CilOpCode(
            (((ushort) CilCode.Brfalse_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Brfalse_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Brtrue_S = new CilOpCode(
            (((ushort) CilCode.Brtrue_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Brtrue_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Beq_S = new CilOpCode(
            (((ushort) CilCode.Beq_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Beq_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Bge_S = new CilOpCode(
            (((ushort) CilCode.Bge_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Bge_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Bgt_S = new CilOpCode(
            (((ushort) CilCode.Bgt_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Bgt_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Ble_S = new CilOpCode(
            (((ushort) CilCode.Ble_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ble_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Blt_S = new CilOpCode(
            (((ushort) CilCode.Blt_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Blt_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Bne_Un_S = new CilOpCode(
            (((ushort) CilCode.Bne_Un_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Bne_Un_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Bge_Un_S = new CilOpCode(
            (((ushort) CilCode.Bge_Un_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Bge_Un_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Bgt_Un_S = new CilOpCode(
            (((ushort) CilCode.Bgt_Un_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Bgt_Un_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Ble_Un_S = new CilOpCode(
            (((ushort) CilCode.Ble_Un_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ble_Un_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Blt_Un_S = new CilOpCode(
            (((ushort) CilCode.Blt_Un_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Blt_Un_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Br = new CilOpCode(
            (((ushort) CilCode.Br & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Br >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) Branch << FlowControlOffset));

        public static readonly CilOpCode Brfalse = new CilOpCode(
            (((ushort) CilCode.Brfalse & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Brfalse >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Brtrue = new CilOpCode(
            (((ushort) CilCode.Brtrue & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Brtrue >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Beq = new CilOpCode(
            (((ushort) CilCode.Beq & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Beq >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Bge = new CilOpCode(
            (((ushort) CilCode.Bge & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Bge >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Bgt = new CilOpCode(
            (((ushort) CilCode.Bgt & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Bgt >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Ble = new CilOpCode(
            (((ushort) CilCode.Ble & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ble >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Blt = new CilOpCode(
            (((ushort) CilCode.Blt & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Blt >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Bne_Un = new CilOpCode(
            (((ushort) CilCode.Bne_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Bne_Un >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Bge_Un = new CilOpCode(
            (((ushort) CilCode.Bge_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Bge_Un >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Bgt_Un = new CilOpCode(
            (((ushort) CilCode.Bgt_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Bgt_Un >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Ble_Un = new CilOpCode(
            (((ushort) CilCode.Ble_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ble_Un >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Blt_Un = new CilOpCode(
            (((ushort) CilCode.Blt_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Blt_Un >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Macro << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Switch = new CilOpCode(
            (((ushort) CilCode.Switch & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Switch >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineSwitch << OperandTypeOffset)
            | ((byte) ConditionalBranch << FlowControlOffset));

        public static readonly CilOpCode Ldind_I1 = new CilOpCode(
            (((ushort) CilCode.Ldind_I1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldind_I1 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldind_U1 = new CilOpCode(
            (((ushort) CilCode.Ldind_U1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldind_U1 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldind_I2 = new CilOpCode(
            (((ushort) CilCode.Ldind_I2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldind_I2 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldind_U2 = new CilOpCode(
            (((ushort) CilCode.Ldind_U2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldind_U2 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldind_I4 = new CilOpCode(
            (((ushort) CilCode.Ldind_I4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldind_I4 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldind_U4 = new CilOpCode(
            (((ushort) CilCode.Ldind_U4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldind_U4 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldind_I8 = new CilOpCode(
            (((ushort) CilCode.Ldind_I8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldind_I8 >> 15) << TwoBytesOffset)
            | ((ushort) PushI8 << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldind_I = new CilOpCode(
            (((ushort) CilCode.Ldind_I & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldind_I >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldind_R4 = new CilOpCode(
            (((ushort) CilCode.Ldind_R4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldind_R4 >> 15) << TwoBytesOffset)
            | ((ushort) PushR4 << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldind_R8 = new CilOpCode(
            (((ushort) CilCode.Ldind_R8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldind_R8 >> 15) << TwoBytesOffset)
            | ((ushort) PushR8 << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldind_Ref = new CilOpCode(
            (((ushort) CilCode.Ldind_Ref & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldind_Ref >> 15) << TwoBytesOffset)
            | ((ushort) PushRef << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stind_Ref = new CilOpCode(
            (((ushort) CilCode.Stind_Ref & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stind_Ref >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI_PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stind_I1 = new CilOpCode(
            (((ushort) CilCode.Stind_I1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stind_I1 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI_PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stind_I2 = new CilOpCode(
            (((ushort) CilCode.Stind_I2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stind_I2 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI_PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stind_I4 = new CilOpCode(
            (((ushort) CilCode.Stind_I4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stind_I4 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI_PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stind_I8 = new CilOpCode(
            (((ushort) CilCode.Stind_I8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stind_I8 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI_PopI8 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stind_R4 = new CilOpCode(
            (((ushort) CilCode.Stind_R4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stind_R4 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI_PopR4 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stind_R8 = new CilOpCode(
            (((ushort) CilCode.Stind_R8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stind_R8 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI_PopR8 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Add = new CilOpCode(
            (((ushort) CilCode.Add & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Add >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Sub = new CilOpCode(
            (((ushort) CilCode.Sub & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Sub >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Mul = new CilOpCode(
            (((ushort) CilCode.Mul & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Mul >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Div = new CilOpCode(
            (((ushort) CilCode.Div & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Div >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Div_Un = new CilOpCode(
            (((ushort) CilCode.Div_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Div_Un >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Rem = new CilOpCode(
            (((ushort) CilCode.Rem & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Rem >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Rem_Un = new CilOpCode(
            (((ushort) CilCode.Rem_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Rem_Un >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode And = new CilOpCode(
            (((ushort) CilCode.And & 0xFF) << ValueOffset)
            | (((ushort) CilCode.And >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Or = new CilOpCode(
            (((ushort) CilCode.Or & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Or >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Xor = new CilOpCode(
            (((ushort) CilCode.Xor & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Xor >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Shl = new CilOpCode(
            (((ushort) CilCode.Shl & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Shl >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Shr = new CilOpCode(
            (((ushort) CilCode.Shr & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Shr >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Shr_Un = new CilOpCode(
            (((ushort) CilCode.Shr_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Shr_Un >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Neg = new CilOpCode(
            (((ushort) CilCode.Neg & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Neg >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Not = new CilOpCode(
            (((ushort) CilCode.Not & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Not >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_I1 = new CilOpCode(
            (((ushort) CilCode.Conv_I1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_I1 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_I2 = new CilOpCode(
            (((ushort) CilCode.Conv_I2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_I2 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_I4 = new CilOpCode(
            (((ushort) CilCode.Conv_I4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_I4 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_I8 = new CilOpCode(
            (((ushort) CilCode.Conv_I8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_I8 >> 15) << TwoBytesOffset)
            | ((ushort) PushI8 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_R4 = new CilOpCode(
            (((ushort) CilCode.Conv_R4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_R4 >> 15) << TwoBytesOffset)
            | ((ushort) PushR4 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_R8 = new CilOpCode(
            (((ushort) CilCode.Conv_R8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_R8 >> 15) << TwoBytesOffset)
            | ((ushort) PushR8 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_U4 = new CilOpCode(
            (((ushort) CilCode.Conv_U4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_U4 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_U8 = new CilOpCode(
            (((ushort) CilCode.Conv_U8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_U8 >> 15) << TwoBytesOffset)
            | ((ushort) PushI8 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Callvirt = new CilOpCode(
            (((ushort) CilCode.Callvirt & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Callvirt >> 15) << TwoBytesOffset)
            | ((ushort) VarPush << StackBehaviourPushOffset)
            | ((ushort) VarPop << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineMethod << OperandTypeOffset)
            | ((byte) CilFlowControl.Call << FlowControlOffset));

        public static readonly CilOpCode Cpobj = new CilOpCode(
            (((ushort) CilCode.Cpobj & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Cpobj >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldobj = new CilOpCode(
            (((ushort) CilCode.Ldobj & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldobj >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldstr = new CilOpCode(
            (((ushort) CilCode.Ldstr & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldstr >> 15) << TwoBytesOffset)
            | ((ushort) PushRef << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineString << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Newobj = new CilOpCode(
            (((ushort) CilCode.Newobj & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Newobj >> 15) << TwoBytesOffset)
            | ((ushort) PushRef << StackBehaviourPushOffset)
            | ((ushort) VarPop << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineMethod << OperandTypeOffset)
            | ((byte) CilFlowControl.Call << FlowControlOffset));

        public static readonly CilOpCode Castclass = new CilOpCode(
            (((ushort) CilCode.Castclass & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Castclass >> 15) << TwoBytesOffset)
            | ((ushort) PushRef << StackBehaviourPushOffset)
            | ((ushort) PopRef << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Isinst = new CilOpCode(
            (((ushort) CilCode.Isinst & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Isinst >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_R_Un = new CilOpCode(
            (((ushort) CilCode.Conv_R_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_R_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushR8 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Unbox = new CilOpCode(
            (((ushort) CilCode.Unbox & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Unbox >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Throw = new CilOpCode(
            (((ushort) CilCode.Throw & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Throw >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopRef << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) CilFlowControl.Throw << FlowControlOffset));

        public static readonly CilOpCode Ldfld = new CilOpCode(
            (((ushort) CilCode.Ldfld & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldfld >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) PopRef << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineField << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldflda = new CilOpCode(
            (((ushort) CilCode.Ldflda & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldflda >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineField << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stfld = new CilOpCode(
            (((ushort) CilCode.Stfld & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stfld >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopRef_Pop1 << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineField << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldsfld = new CilOpCode(
            (((ushort) CilCode.Ldsfld & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldsfld >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineField << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldsflda = new CilOpCode(
            (((ushort) CilCode.Ldsflda & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldsflda >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineField << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stsfld = new CilOpCode(
            (((ushort) CilCode.Stsfld & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stsfld >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineField << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stobj = new CilOpCode(
            (((ushort) CilCode.Stobj & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stobj >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_Ovf_I1_Un = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_I1_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_I1_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_Ovf_I2_Un = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_I2_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_I2_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_Ovf_I4_Un = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_I4_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_I4_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_Ovf_I8_Un = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_I8_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_I8_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushI8 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_Ovf_U1_Un = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_U1_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_U1_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_Ovf_U2_Un = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_U2_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_U2_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_Ovf_U4_Un = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_U4_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_U4_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_Ovf_U8_Un = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_U8_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_U8_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushI8 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_Ovf_I_Un = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_I_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_I_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_Ovf_U_Un = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_U_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_U_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Box = new CilOpCode(
            (((ushort) CilCode.Box & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Box >> 15) << TwoBytesOffset)
            | ((ushort) PushRef << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Newarr = new CilOpCode(
            (((ushort) CilCode.Newarr & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Newarr >> 15) << TwoBytesOffset)
            | ((ushort) PushRef << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldlen = new CilOpCode(
            (((ushort) CilCode.Ldlen & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldlen >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldelema = new CilOpCode(
            (((ushort) CilCode.Ldelema & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelema >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldelem_I1 = new CilOpCode(
            (((ushort) CilCode.Ldelem_I1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelem_I1 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldelem_U1 = new CilOpCode(
            (((ushort) CilCode.Ldelem_U1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelem_U1 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldelem_I2 = new CilOpCode(
            (((ushort) CilCode.Ldelem_I2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelem_I2 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldelem_U2 = new CilOpCode(
            (((ushort) CilCode.Ldelem_U2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelem_U2 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldelem_I4 = new CilOpCode(
            (((ushort) CilCode.Ldelem_I4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelem_I4 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldelem_U4 = new CilOpCode(
            (((ushort) CilCode.Ldelem_U4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelem_U4 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldelem_I8 = new CilOpCode(
            (((ushort) CilCode.Ldelem_I8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelem_I8 >> 15) << TwoBytesOffset)
            | ((ushort) PushI8 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldelem_I = new CilOpCode(
            (((ushort) CilCode.Ldelem_I & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelem_I >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldelem_R4 = new CilOpCode(
            (((ushort) CilCode.Ldelem_R4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelem_R4 >> 15) << TwoBytesOffset)
            | ((ushort) PushR4 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldelem_R8 = new CilOpCode(
            (((ushort) CilCode.Ldelem_R8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelem_R8 >> 15) << TwoBytesOffset)
            | ((ushort) PushR8 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldelem_Ref = new CilOpCode(
            (((ushort) CilCode.Ldelem_Ref & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelem_Ref >> 15) << TwoBytesOffset)
            | ((ushort) PushRef << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stelem_I = new CilOpCode(
            (((ushort) CilCode.Stelem_I & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stelem_I >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stelem_I1 = new CilOpCode(
            (((ushort) CilCode.Stelem_I1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stelem_I1 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stelem_I2 = new CilOpCode(
            (((ushort) CilCode.Stelem_I2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stelem_I2 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stelem_I4 = new CilOpCode(
            (((ushort) CilCode.Stelem_I4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stelem_I4 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stelem_I8 = new CilOpCode(
            (((ushort) CilCode.Stelem_I8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stelem_I8 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI_PopI8 << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stelem_R4 = new CilOpCode(
            (((ushort) CilCode.Stelem_R4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stelem_R4 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI_PopR4 << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stelem_R8 = new CilOpCode(
            (((ushort) CilCode.Stelem_R8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stelem_R8 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI_PopR8 << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stelem_Ref = new CilOpCode(
            (((ushort) CilCode.Stelem_Ref & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stelem_Ref >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI_PopRef << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldelem = new CilOpCode(
            (((ushort) CilCode.Ldelem & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldelem >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stelem = new CilOpCode(
            (((ushort) CilCode.Stelem & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stelem >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopRef_PopI_Pop1 << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Unbox_Any = new CilOpCode(
            (((ushort) CilCode.Unbox_Any & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Unbox_Any >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) PopRef << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_Ovf_I1 = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_I1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_I1 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_Ovf_U1 = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_U1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_U1 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_Ovf_I2 = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_I2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_I2 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_Ovf_U2 = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_U2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_U2 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_Ovf_I4 = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_I4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_I4 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_Ovf_U4 = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_U4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_U4 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_Ovf_I8 = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_I8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_I8 >> 15) << TwoBytesOffset)
            | ((ushort) PushI8 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_Ovf_U8 = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_U8 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_U8 >> 15) << TwoBytesOffset)
            | ((ushort) PushI8 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Refanyval = new CilOpCode(
            (((ushort) CilCode.Refanyval & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Refanyval >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ckfinite = new CilOpCode(
            (((ushort) CilCode.Ckfinite & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ckfinite >> 15) << TwoBytesOffset)
            | ((ushort) PushR8 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Mkrefany = new CilOpCode(
            (((ushort) CilCode.Mkrefany & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Mkrefany >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldtoken = new CilOpCode(
            (((ushort) CilCode.Ldtoken & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldtoken >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineTok << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_U2 = new CilOpCode(
            (((ushort) CilCode.Conv_U2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_U2 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_U1 = new CilOpCode(
            (((ushort) CilCode.Conv_U1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_U1 >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_I = new CilOpCode(
            (((ushort) CilCode.Conv_I & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_I >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_Ovf_I = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_I & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_I >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_Ovf_U = new CilOpCode(
            (((ushort) CilCode.Conv_Ovf_U & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_Ovf_U >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Add_Ovf = new CilOpCode(
            (((ushort) CilCode.Add_Ovf & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Add_Ovf >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Add_Ovf_Un = new CilOpCode(
            (((ushort) CilCode.Add_Ovf_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Add_Ovf_Un >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Mul_Ovf = new CilOpCode(
            (((ushort) CilCode.Mul_Ovf & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Mul_Ovf >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Mul_Ovf_Un = new CilOpCode(
            (((ushort) CilCode.Mul_Ovf_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Mul_Ovf_Un >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Sub_Ovf = new CilOpCode(
            (((ushort) CilCode.Sub_Ovf & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Sub_Ovf >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Sub_Ovf_Un = new CilOpCode(
            (((ushort) CilCode.Sub_Ovf_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Sub_Ovf_Un >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Endfinally = new CilOpCode(
            (((ushort) CilCode.Endfinally & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Endfinally >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Return << FlowControlOffset));

        public static readonly CilOpCode Leave = new CilOpCode(
            (((ushort) CilCode.Leave & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Leave >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopAll << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineBrTarget << OperandTypeOffset)
            | ((byte) Branch << FlowControlOffset));

        public static readonly CilOpCode Leave_S = new CilOpCode(
            (((ushort) CilCode.Leave_S & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Leave_S >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopAll << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) ShortInlineBrTarget << OperandTypeOffset)
            | ((byte) Branch << FlowControlOffset));

        public static readonly CilOpCode Stind_I = new CilOpCode(
            (((ushort) CilCode.Stind_I & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stind_I >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI_PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Conv_U = new CilOpCode(
            (((ushort) CilCode.Conv_U & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Conv_U >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Prefix7 = new CilOpCode(
            (((ushort) CilCode.Prefix7 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Prefix7 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Internal << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

        public static readonly CilOpCode Prefix6 = new CilOpCode(
            (((ushort) CilCode.Prefix6 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Prefix6 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Internal << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

        public static readonly CilOpCode Prefix5 = new CilOpCode(
            (((ushort) CilCode.Prefix5 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Prefix5 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Internal << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

        public static readonly CilOpCode Prefix4 = new CilOpCode(
            (((ushort) CilCode.Prefix4 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Prefix4 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Internal << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

        public static readonly CilOpCode Prefix3 = new CilOpCode(
            (((ushort) CilCode.Prefix3 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Prefix3 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Internal << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

        public static readonly CilOpCode Prefix2 = new CilOpCode(
            (((ushort) CilCode.Prefix2 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Prefix2 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Internal << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

        public static readonly CilOpCode Prefix1 = new CilOpCode(
            (((ushort) CilCode.Prefix1 & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Prefix1 >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Internal << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

        public static readonly CilOpCode Prefixref = new CilOpCode(
            (((ushort) CilCode.Prefixref & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Prefixref >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Internal << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

        public static readonly CilOpCode Arglist = new CilOpCode(
            (((ushort) CilCode.Arglist & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Arglist >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ceq = new CilOpCode(
            (((ushort) CilCode.Ceq & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ceq >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Cgt = new CilOpCode(
            (((ushort) CilCode.Cgt & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Cgt >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Cgt_Un = new CilOpCode(
            (((ushort) CilCode.Cgt_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Cgt_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Clt = new CilOpCode(
            (((ushort) CilCode.Clt & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Clt >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Clt_Un = new CilOpCode(
            (((ushort) CilCode.Clt_Un & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Clt_Un >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1_Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldftn = new CilOpCode(
            (((ushort) CilCode.Ldftn & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldftn >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineMethod << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldvirtftn = new CilOpCode(
            (((ushort) CilCode.Ldvirtftn & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldvirtftn >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopRef << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineMethod << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldarg = new CilOpCode(
            (((ushort) CilCode.Ldarg & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldarg >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineArgument << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldarga = new CilOpCode(
            (((ushort) CilCode.Ldarga & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldarga >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineArgument << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Starg = new CilOpCode(
            (((ushort) CilCode.Starg & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Starg >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineArgument << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldloc = new CilOpCode(
            (((ushort) CilCode.Ldloc & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldloc >> 15) << TwoBytesOffset)
            | ((ushort) Push1 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineVar << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Ldloca = new CilOpCode(
            (((ushort) CilCode.Ldloca & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Ldloca >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineVar << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Stloc = new CilOpCode(
            (((ushort) CilCode.Stloc & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Stloc >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineVar << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Localloc = new CilOpCode(
            (((ushort) CilCode.Localloc & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Localloc >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Endfilter = new CilOpCode(
            (((ushort) CilCode.Endfilter & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Endfilter >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Return << FlowControlOffset));

        public static readonly CilOpCode Unaligned = new CilOpCode(
            (((ushort) CilCode.Unaligned & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Unaligned >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Prefix << OpCodeTypeOffset)
            | ((byte) ShortInlineI << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

        public static readonly CilOpCode Volatile = new CilOpCode(
            (((ushort) CilCode.Volatile & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Volatile >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Prefix << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

        public static readonly CilOpCode Tailcall = new CilOpCode(
            (((ushort) CilCode.Tailcall & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Tailcall >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Prefix << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

        public static readonly CilOpCode Initobj = new CilOpCode(
            (((ushort) CilCode.Initobj & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Initobj >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Constrained = new CilOpCode(
            (((ushort) CilCode.Constrained & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Constrained >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Prefix << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Meta << FlowControlOffset));

        public static readonly CilOpCode Cpblk = new CilOpCode(
            (((ushort) CilCode.Cpblk & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Cpblk >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI_PopI_PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Initblk = new CilOpCode(
            (((ushort) CilCode.Initblk & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Initblk >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) PopI_PopI_PopI << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Rethrow = new CilOpCode(
            (((ushort) CilCode.Rethrow & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Rethrow >> 15) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) ObjModel << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) CilFlowControl.Throw << FlowControlOffset));

        public static readonly CilOpCode Sizeof = new CilOpCode(
            (((ushort) CilCode.Sizeof & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Sizeof >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineType << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Refanytype = new CilOpCode(
            (((ushort) CilCode.Refanytype & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Refanytype >> 15) << TwoBytesOffset)
            | ((ushort) PushI << StackBehaviourPushOffset)
            | ((ushort) Pop1 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

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