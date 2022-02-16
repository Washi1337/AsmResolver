namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Provides members defining all possible numerical values for each CIL operation code.
    /// </summary>
    /// <remarks>
    /// See also: <seealso href="https://www.ecma-international.org/wp-content/uploads/ECMA-335_6th_edition_june_2012.pdf"/>
    /// </remarks>
    public enum CilCode : ushort
    {
        /// <summary>
        /// Do nothing (No operation).
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.nop?view=net-6.0"/>
        /// </remarks>
        Nop,

        /// <summary>
        /// Inform a debugger that a breakpoint has been reached.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.break?view=net-6.0"/>
        /// </remarks>
        Break,

        /// <summary>
        /// Load argument 0 onto the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarg_0?view=net-6.0"/>
        /// </remarks>
        Ldarg_0,

        /// <summary>
        /// Load argument 1 onto the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarg_1?view=net-6.0"/>
        /// </remarks>
        Ldarg_1,

        /// <summary>
        /// Load argument 2 onto the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarg_2?view=net-6.0"/>
        /// </remarks>
        Ldarg_2,

        /// <summary>
        /// Load argument 3 onto the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarg_3?view=net-6.0"/>
        /// </remarks>
        Ldarg_3,

        /// <summary>
        /// Load local variable 0 onto stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldloc_0?view=net-6.0"/>
        /// </remarks>
        Ldloc_0,

        /// <summary>
        /// Load local variable 1 onto stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldloc_1?view=net-6.0"/>
        /// </remarks>
        Ldloc_1,

        /// <summary>
        /// Load local variable 2 onto stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldloc_2?view=net-6.0"/>
        /// </remarks>
        Ldloc_2,

        /// <summary>
        /// Load local variable 3 onto stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldloc_3?view=net-6.0"/>
        /// </remarks>
        Ldloc_3,

        /// <summary>
        /// Pop a value from stack into local variable 0.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stloc_0?view=net-6.0"/>
        /// </remarks>
        Stloc_0,

        /// <summary>
        /// Pop a value from stack into local variable 1.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stloc_1?view=net-6.0"/>
        /// </remarks>
        Stloc_1,

        /// <summary>
        /// Pop a value from stack into local variable 2.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stloc_2?view=net-6.0"/>
        /// </remarks>
        Stloc_2,

        /// <summary>
        /// Pop a value from stack into local variable 3.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stloc_3?view=net-6.0"/>
        /// </remarks>
        Stloc_3,

        /// <summary>
        /// Load argument onto the stack, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarg_s?view=net-6.0"/>
        /// </remarks>
        Ldarg_S,

        /// <summary>
        /// Fetch the address of argument, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarga_s?view=net-6.0"/>
        /// </remarks>
        Ldarga_S,

        /// <summary>
        /// Store value to the argument numbered, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.starg_s?view=net-6.0"/>
        /// </remarks>
        Starg_S,

        /// <summary>
        /// Load local variable of index onto stack, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldloc_s?view=net-6.0"/>
        /// </remarks>
        Ldloc_S,

        /// <summary>
        /// Load address of local variable with index, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldloca_s?view=net-6.0"/>
        /// </remarks>
        Ldloca_S,

        /// <summary>
        /// Pop a value from stack into local variable with index, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stloc_s?view=net-6.0"/>
        /// </remarks>
        Stloc_S,

        /// <summary>
        /// Push a null reference on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldnull?view=net-6.0"/>
        /// </remarks>
        Ldnull,

        /// <summary>
        /// Push -1 onto the stack as int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_m1?view=net-6.0"/>
        /// </remarks>
        Ldc_I4_M1,

        /// <summary>
        /// Push 0 onto the stack as int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_0?view=net-6.0"/>
        /// </remarks>
        Ldc_I4_0,

        /// <summary>
        /// Push 1 onto the stack as int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_1?view=net-6.0"/>
        /// </remarks>
        Ldc_I4_1,

        /// <summary>
        /// Push 2 onto the stack as int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_2?view=net-6.0"/>
        /// </remarks>
        Ldc_I4_2,

        /// <summary>
        /// Push 3 onto the stack as int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_3?view=net-6.0"/>
        /// </remarks>
        Ldc_I4_3,

        /// <summary>
        /// Push 4 onto the stack as int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_4?view=net-6.0"/>
        /// </remarks>
        Ldc_I4_4,

        /// <summary>
        /// Push 5 onto the stack as int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_5?view=net-6.0"/>
        /// </remarks>
        Ldc_I4_5,

        /// <summary>
        /// Push 6 onto the stack as int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_6?view=net-6.0"/>
        /// </remarks>
        Ldc_I4_6,

        /// <summary>
        /// Push 7 onto the stack as int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_7?view=net-6.0"/>
        /// </remarks>
        Ldc_I4_7,

        /// <summary>
        /// Push 8 onto the stack as int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_8?view=net-6.0"/>
        /// </remarks>
        Ldc_I4_8,

        /// <summary>
        /// Push num onto the stack as int32, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_s?view=net-6.0"/>
        /// </remarks>
        Ldc_I4_S,

        /// <summary>
        /// Push num of type int32 onto the stack as int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4?view=net-6.0"/>
        /// </remarks>
        Ldc_I4,

        /// <summary>
        /// Push num of type int64 onto the stack as int64.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i8?view=net-6.0"/>
        /// </remarks>
        Ldc_I8,

        /// <summary>
        /// Push num of type float32 onto the stack as F.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_r4?view=net-6.0"/>
        /// </remarks>
        Ldc_R4,

        /// <summary>
        /// Push num of type float64 onto the stack as F.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_r8?view=net-6.0"/>
        /// </remarks>
        Ldc_R8,

        /// <summary>
        /// Duplicate the value on the top of the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.dup?view=net-6.0"/>
        /// </remarks>
        Dup = 0x25,

        /// <summary>
        /// Pop value from the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.pop?view=net-6.0"/>
        /// </remarks>
        Pop,

        /// <summary>
        /// Exit current method and jump to the specified method.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.jmp?view=net-6.0"/>
        /// </remarks>
        Jmp,

        /// <summary>
        /// Call method described by method.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.call?view=net-6.0"/>
        /// </remarks>
        Call,

        /// <summary>
        /// Call method indicated on the stack with arguments described by a calling convention.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.calli?view=net-6.0"/>
        /// </remarks>
        Calli,

        /// <summary>
        /// Return from method, possibly with a value.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ret?view=net-6.0"/>
        /// </remarks>
        Ret,

        /// <summary>
        /// Branch to target, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.br_s?view=net-6.0"/>
        /// </remarks>
        Br_S,

        /// <summary>
        /// Branch to target if value is zero (false), short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.brfalse_s?view=net-6.0"/>
        /// </remarks>
        Brfalse_S,

        /// <summary>
        /// Branch to target if value is non-zero (true), short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.brtrue_s?view=net-6.0"/>
        /// </remarks>
        Brtrue_S,

        /// <summary>
        /// Branch to target if equal, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.beq_s?view=net-6.0"/>
        /// </remarks>
        Beq_S,

        /// <summary>
        /// Branch to target if greater than or equal to, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.bge_s?view=net-6.0"/>
        /// </remarks>
        Bge_S,

        /// <summary>
        /// Branch to target if greater than, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.bgt_s?view=net-6.0"/>
        /// </remarks>
        Bgt_S,

        /// <summary>
        /// Branch to target if less than or equal to, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ble_s?view=net-6.0"/>
        /// </remarks>
        Ble_S,

        /// <summary>
        /// Branch to target if less than, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.blt_s?view=net-6.0"/>
        /// </remarks>
        Blt_S,

        /// <summary>
        /// Branch to target if unequal or unordered, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.bne_un_s?view=net-6.0"/>
        /// </remarks>
        Bne_Un_S,

        /// <summary>
        /// Branch to target if greater than or equal to (unsigned or unordered), short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.bge_un_s?view=net-6.0"/>
        /// </remarks>
        Bge_Un_S,

        /// <summary>
        /// Branch to target if greater than (unsigned or unordered), short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.bgt_un_s?view=net-6.0"/>
        /// </remarks>
        Bgt_Un_S,

        /// <summary>
        /// Branch to target if less than or equal to (unsigned or unordered), short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ble_un_s?view=net-6.0"/>
        /// </remarks>
        Ble_Un_S,

        /// <summary>
        /// Branch to target if less than (unsigned or unordered), short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.blt_un_s?view=net-6.0"/>
        /// </remarks>
        Blt_Un_S,

        /// <summary>
        /// Branch to target.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.br?view=net-6.0"/>
        /// </remarks>
        Br,

        /// <summary>
        /// Branch to target if value is zero (false).
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.brfalse?view=net-6.0"/>
        /// </remarks>
        Brfalse,

        /// <summary>
        /// Branch to target if value is non-zero (true).
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.brtrue?view=net-6.0"/>
        /// </remarks>
        Brtrue,

        /// <summary>
        /// Branch to target if equal.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.beq?view=net-6.0"/>
        /// </remarks>
        Beq,

        /// <summary>
        /// Branch to target if greater than or equal to.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.bge?view=net-6.0"/>
        /// </remarks>
        Bge,

        /// <summary>
        /// Branch to target if greater than.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.bgt?view=net-6.0"/>
        /// </remarks>
        Bgt,

        /// <summary>
        /// Branch to target if less than or equal to.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ble?view=net-6.0"/>
        /// </remarks>
        Ble,

        /// <summary>
        /// Branch to target if less than.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.blt?view=net-6.0"/>
        /// </remarks>
        Blt,

        /// <summary>
        /// Branch to target if unequal or unordered.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.bne_un?view=net-6.0"/>
        /// </remarks>
        Bne_Un,

        /// <summary>
        /// Branch to target if greater than or equal to (unsigned or unordered).
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.bge_un?view=net-6.0"/>
        /// </remarks>
        Bge_Un,

        /// <summary>
        /// Branch to target if greater than (unsigned or unordered).
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.bgt_un?view=net-6.0"/>
        /// </remarks>
        Bgt_Un,

        /// <summary>
        /// Branch to target if less than or equal to (unsigned or unordered).
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ble_un?view=net-6.0"/>
        /// </remarks>
        Ble_Un,

        /// <summary>
        /// Branch to target if less than (unsigned or unordered).
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.blt_un?view=net-6.0"/>
        /// </remarks>
        Blt_Un,

        /// <summary>
        /// Jump to one of n values.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.switch?view=net-6.0"/>
        /// </remarks>
        Switch,

        /// <summary>
        /// Indirect load value of type int8 as int32 on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldind_i1?view=net-6.0"/>
        /// </remarks>
        Ldind_I1,

        /// <summary>
        /// Indirect load value of type unsigned int8 as int32 on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldind_u1?view=net-6.0"/>
        /// </remarks>
        Ldind_U1,

        /// <summary>
        /// Indirect load value of type int16 as int32 on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldind_i2?view=net-6.0"/>
        /// </remarks>
        Ldind_I2,

        /// <summary>
        /// Indirect load value of type unsigned int16 as int32 on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldind_u2?view=net-6.0"/>
        /// </remarks>
        Ldind_U2,

        /// <summary>
        /// Indirect load value of type int32 as int32 on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldind_i4?view=net-6.0"/>
        /// </remarks>
        Ldind_I4,

        /// <summary>
        /// Indirect load value of type unsigned int32 as int32 on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldind_u4?view=net-6.0"/>
        /// </remarks>
        Ldind_U4,

        /// <summary>
        /// Indirect load value of type signed or unsigned int64 as signed int64 on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldind_i8?view=net-6.0"/>
        /// </remarks>
        Ldind_I8,

        /// <summary>
        /// Indirect load value of type native int as native int on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldind_i?view=net-6.0"/>
        /// </remarks>
        Ldind_I,

        /// <summary>
        /// Indirect load value of type float32 as F on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldind_r4?view=net-6.0"/>
        /// </remarks>
        Ldind_R4,

        /// <summary>
        /// Indirect load value of type float64 as F on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldind_r8?view=net-6.0"/>
        /// </remarks>
        Ldind_R8,

        /// <summary>
        /// Indirect load value of type object ref as O on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldind_ref?view=net-6.0"/>
        /// </remarks>
        Ldind_Ref,

        /// <summary>
        /// Store value of type object ref (type O) into memory at address.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stind_ref?view=net-6.0"/>
        /// </remarks>
        Stind_Ref,

        /// <summary>
        /// Store value of type int8 into memory at address.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stind_i1?view=net-6.0"/>
        /// </remarks>
        Stind_I1,

        /// <summary>
        /// Store value of type int16 into memory at address.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stind_i2?view=net-6.0"/>
        /// </remarks>
        Stind_I2,

        /// <summary>
        /// Store value of type int32 into memory at address.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stind_i4?view=net-6.0"/>
        /// </remarks>
        Stind_I4,

        /// <summary>
        /// Store value of type int64 into memory at address.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stind_i8?view=net-6.0"/>
        /// </remarks>
        Stind_I8,

        /// <summary>
        /// Store value of type float32 into memory at address.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stind_r4?view=net-6.0"/>
        /// </remarks>
        Stind_R4,

        /// <summary>
        /// Store value of type float64 into memory at address.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stind_r8?view=net-6.0"/>
        /// </remarks>
        Stind_R8,

        /// <summary>
        /// Add two values, returning a new value.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.add?view=net-6.0"/>
        /// </remarks>
        Add,

        /// <summary>
        /// Subtract value2 from value1, returning a new value.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.sub?view=net-6.0"/>
        /// </remarks>
        Sub,

        /// <summary>
        /// Multiply values.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.mul?view=net-6.0"/>
        /// </remarks>
        Mul,

        /// <summary>
        /// Divide two values to return a quotient or floating-point result.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.div?view=net-6.0"/>
        /// </remarks>
        Div,

        /// <summary>
        /// Divide two values, unsigned, returning a quotient.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.div_un?view=net-6.0"/>
        /// </remarks>
        Div_Un,

        /// <summary>
        /// Remainder when dividing one value by another.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.rem?view=net-6.0"/>
        /// </remarks>
        Rem,

        /// <summary>
        /// Remainder when dividing one unsigned value by another.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.rem_un?view=net-6.0"/>
        /// </remarks>
        Rem_Un,

        /// <summary>
        /// Bitwise AND of two integral values, returns an integral value.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.and?view=net-6.0"/>
        /// </remarks>
        And,

        /// <summary>
        /// Bitwise OR of two integer values, returns an integer.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.or?view=net-6.0"/>
        /// </remarks>
        Or,

        /// <summary>
        /// Bitwise XOR of integer values, returns an integer.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.xor?view=net-6.0"/>
        /// </remarks>
        Xor,

        /// <summary>
        /// Shift an integer left (shifting in zeros), return an integer.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.shl?view=net-6.0"/>
        /// </remarks>
        Shl,

        /// <summary>
        /// Shift an integer right (shift in sign), return an integer.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.shr?view=net-6.0"/>
        /// </remarks>
        Shr,

        /// <summary>
        /// Shift an integer right (shift in zero), return an integer.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.shr_un?view=net-6.0"/>
        /// </remarks>
        Shr_Un,

        /// <summary>
        /// Negate value.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.neg?view=net-6.0"/>
        /// </remarks>
        Neg,

        /// <summary>
        /// Bitwise complement (logical not).
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.not?view=net-6.0"/>
        /// </remarks>
        Not,

        /// <summary>
        /// Convert to int8, pushing int32 on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_i1?view=net-6.0"/>
        /// </remarks>
        Conv_I1,

        /// <summary>
        /// Convert to int16, pushing int32 on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_i2?view=net-6.0"/>
        /// </remarks>
        Conv_I2,

        /// <summary>
        /// Convert to int32, pushing int32 on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_i4?view=net-6.0"/>
        /// </remarks>
        Conv_I4,

        /// <summary>
        /// Convert to int64, pushing int64 on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_i8?view=net-6.0"/>
        /// </remarks>
        Conv_I8,

        /// <summary>
        /// Convert to float32, pushing F on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_r4?view=net-6.0"/>
        /// </remarks>
        Conv_R4,

        /// <summary>
        /// Convert to float64, pushing F on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_r8?view=net-6.0"/>
        /// </remarks>
        Conv_R8,

        /// <summary>
        /// Convert to unsigned int32, pushing int32 on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_u4?view=net-6.0"/>
        /// </remarks>
        Conv_U4,

        /// <summary>
        /// Convert to unsigned int64, pushing int64 on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_u8?view=net-6.0"/>
        /// </remarks>
        Conv_U8,

        /// <summary>
        /// Call a method associated with an object.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.callvirt?view=net-6.0"/>
        /// </remarks>
        Callvirt,

        /// <summary>
        /// Copy a value type from src to dest.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.cpobj?view=net-6.0"/>
        /// </remarks>
        Cpobj,

        /// <summary>
        /// Copy the value stored at address src to the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldobj?view=net-6.0"/>
        /// </remarks>
        Ldobj,

        /// <summary>
        /// Push a string object for the literal string.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldstr?view=net-6.0"/>
        /// </remarks>
        Ldstr,

        /// <summary>
        /// Allocate an uninitialized object or value type and call ctor.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.newobj?view=net-6.0"/>
        /// </remarks>
        Newobj,

        /// <summary>
        /// Cast obj to class.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.castclass?view=net-6.0"/>
        /// </remarks>
        Castclass,

        /// <summary>
        /// Test if obj is an instance of class, returning null or an instance of that class or interface.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.isinst?view=net-6.0"/>
        /// </remarks>
        Isinst,

        /// <summary>
        /// Convert unsigned integer to floating-point, pushing F on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_r_un?view=net-6.0"/>
        /// </remarks>
        Conv_R_Un,

        /// <summary>
        /// Extract a value-type from obj, its boxed representation, and push a controlled-mutability managed pointer to it to the top of the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.unbox?view=net-6.0"/>
        /// </remarks>
        Unbox = 0x79,

        /// <summary>
        /// Throw an exception.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.throw?view=net-6.0"/>
        /// </remarks>
        Throw,

        /// <summary>
        /// Push the value of field of object (or value type) obj, onto the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldfld?view=net-6.0"/>
        /// </remarks>
        Ldfld,

        /// <summary>
        /// Push the address of field of object obj on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldflda?view=net-6.0"/>
        /// </remarks>
        Ldflda,

        /// <summary>
        /// Replace the value of field of the object obj with value.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stfld?view=net-6.0"/>
        /// </remarks>
        Stfld,

        /// <summary>
        /// Push the value of the static field on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldsfld?view=net-6.0"/>
        /// </remarks>
        Ldsfld,

        /// <summary>
        /// Push the address of the static field, field, on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldsflda?view=net-6.0"/>
        /// </remarks>
        Ldsflda,

        /// <summary>
        /// Replace the value of the static field.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stsfld?view=net-6.0"/>
        /// </remarks>
        Stsfld,

        /// <summary>
        /// Store a value at an address.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stobj?view=net-6.0"/>
        /// </remarks>
        Stobj,

        /// <summary>
        /// Convert unsigned to an int8 (on the stack as int32) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_i1_un?view=net-6.0"/>
        /// </remarks>
        Conv_Ovf_I1_Un,

        /// <summary>
        /// Convert unsigned to an int16 (on the stack as int32) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_i2_un?view=net-6.0"/>
        /// </remarks>
        Conv_Ovf_I2_Un,

        /// <summary>
        /// Convert unsigned to an int32 (on the stack as int32) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_i4_un?view=net-6.0"/>
        /// </remarks>
        Conv_Ovf_I4_Un,

        /// <summary>
        /// Convert unsigned to an int64 (on the stack as int64) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_i8_un?view=net-6.0"/>
        /// </remarks>
        Conv_Ovf_I8_Un,

        /// <summary>
        /// Convert unsigned to an unsigned int8 (on the stack as int32) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_u1_un?view=net-6.0"/>
        /// </remarks>
        Conv_Ovf_U1_Un,

        /// <summary>
        /// Convert unsigned to an unsigned int16 (on the stack as int32) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_u2_un?view=net-6.0"/>
        /// </remarks>
        Conv_Ovf_U2_Un,

        /// <summary>
        /// Convert unsigned to an unsigned int32 (on the stack as int32) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_u4_un?view=net-6.0"/>
        /// </remarks>
        Conv_Ovf_U4_Un,

        /// <summary>
        /// Convert unsigned to an unsigned int64 (on the stack as int64) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_u8_un?view=net-6.0"/>
        /// </remarks>
        Conv_Ovf_U8_Un,

        /// <summary>
        /// Convert unsigned to a native int (on the stack as native int) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_i_un?view=net-6.0"/>
        /// </remarks>
        Conv_Ovf_I_Un,

        /// <summary>
        /// Convert unsigned to a native unsigned int (on the stack as native int) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_u_un?view=net-6.0"/>
        /// </remarks>
        Conv_Ovf_U_Un,

        /// <summary>
        /// Convert a boxable value to its boxed form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.box?view=net-6.0"/>
        /// </remarks>
        Box,

        /// <summary>
        /// Create a new array with elements of type etype.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.newarr?view=net-6.0"/>
        /// </remarks>
        Newarr,

        /// <summary>
        /// Push the length (of type native unsigned int) of array on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldlen?view=net-6.0"/>
        /// </remarks>
        Ldlen,

        /// <summary>
        /// Load the address of element at index onto the top of the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelema?view=net-6.0"/>
        /// </remarks>
        Ldelema,

        /// <summary>
        /// Load the element with type int8 at index onto the top of the stack as an int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelem_i1?view=net-6.0"/>
        /// </remarks>
        Ldelem_I1,

        /// <summary>
        /// Load the element with type unsigned int8 at index onto the top of the stack as an int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelem_u1?view=net-6.0"/>
        /// </remarks>
        Ldelem_U1,

        /// <summary>
        /// Load the element with type int16 at index onto the top of the stack as an int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelem_i2?view=net-6.0"/>
        /// </remarks>
        Ldelem_I2,

        /// <summary>
        /// Load the element with type unsigned int16 at index onto the top of the stack as an int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelem_u2?view=net-6.0"/>
        /// </remarks>
        Ldelem_U2,

        /// <summary>
        /// Load the element with type int32 at index onto the top of the stack as an int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelem_i4?view=net-6.0"/>
        /// </remarks>
        Ldelem_I4,

        /// <summary>
        /// Load the element with type unsigned int32 at index onto the top of the stack as an int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelem_u4?view=net-6.0"/>
        /// </remarks>
        Ldelem_U4,

        /// <summary>
        /// Load the element with type signed or unsigned int64 at index onto the top of the stack as a signed int64.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelem_i8?view=net-6.0"/>
        /// </remarks>
        Ldelem_I8,

        /// <summary>
        /// Load the element with type native int at index onto the top of the stack as a native int.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelem_i?view=net-6.0"/>
        /// </remarks>
        Ldelem_I,

        /// <summary>
        /// Load the element with type float32 at index onto the top of the stack as an F.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelem_r4?view=net-6.0"/>
        /// </remarks>
        Ldelem_R4,

        /// <summary>
        /// Load the element with type float64 at index onto the top of the stack as an F.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelem_r8?view=net-6.0"/>
        /// </remarks>
        Ldelem_R8,

        /// <summary>
        /// Load the element at index onto the top of the stack as an O. The type of the O is the same as the element type of the array pushed on the CIL stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelem_ref?view=net-6.0"/>
        /// </remarks>
        Ldelem_Ref,

        /// <summary>
        /// Replace array element at index with the i value on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stelem_i?view=net-6.0"/>
        /// </remarks>
        Stelem_I,

        /// <summary>
        /// Replace array element at index with the int8 value on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stelem_i1?view=net-6.0"/>
        /// </remarks>
        Stelem_I1,

        /// <summary>
        /// Replace array element at index with the int16 value on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stelem_i2?view=net-6.0"/>
        /// </remarks>
        Stelem_I2,

        /// <summary>
        /// Replace array element at index with the int32 value on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stelem_i4?view=net-6.0"/>
        /// </remarks>
        Stelem_I4,

        /// <summary>
        /// Replace array element at index with the int64 value on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stelem_i8?view=net-6.0"/>
        /// </remarks>
        Stelem_I8,

        /// <summary>
        /// Replace array element at index with the float32 value on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stelem_r4?view=net-6.0"/>
        /// </remarks>
        Stelem_R4,

        /// <summary>
        /// Replace array element at index with the float64 value on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stelem_r8?view=net-6.0"/>
        /// </remarks>
        Stelem_R8,

        /// <summary>
        /// Replace array element at index with the ref value on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stelem_ref?view=net-6.0"/>
        /// </remarks>
        Stelem_Ref,

        /// <summary>
        /// Load the element at index onto the top of the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldelem?view=net-6.0"/>
        /// </remarks>
        Ldelem,

        /// <summary>
        /// Replace array element at index with the value on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stelem?view=net-6.0"/>
        /// </remarks>
        Stelem,

        /// <summary>
        /// Extract a value-type from obj, its boxed representation, and copy to the top of the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.unbox_any?view=net-6.0"/>
        /// </remarks>
        Unbox_Any,

        /// <summary>
        /// Convert to an int8 (on the stack as int32) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_i1?view=net-6.0"/>
        /// </remarks>
        Conv_Ovf_I1 = 0xB3,

        /// <summary>
        /// Convert to an unsigned int8 (on the stack as int32) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_u1?view=net-6.0"/>
        /// </remarks>
        Conv_Ovf_U1,

        /// <summary>
        /// Convert to an int16 (on the stack as int32) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_i2?view=net-6.0"/>
        /// </remarks>
        Conv_Ovf_I2,

        /// <summary>
        /// Convert to an unsigned int16 (on the stack as int32) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_u2?view=net-6.0"/>
        /// </remarks>
        Conv_Ovf_U2,

        /// <summary>
        /// Convert to an int32 (on the stack as int32) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_i4?view=net-6.0"/>
        /// </remarks>
        Conv_Ovf_I4,

        /// <summary>
        /// Convert to an unsigned int32 (on the stack as int32) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_u4?view=net-6.0"/>
        /// </remarks>
        Conv_Ovf_U4,

        /// <summary>
        /// Convert to an int64 (on the stack as int64) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_i8?view=net-6.0"/>
        /// </remarks>
        Conv_Ovf_I8,

        /// <summary>
        /// Convert to an unsigned int64 (on the stack as int64) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_u8?view=net-6.0"/>
        /// </remarks>
        Conv_Ovf_U8,

        /// <summary>
        /// Push the address stored in a typed reference.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.refanyval?view=net-6.0"/>
        /// </remarks>
        Refanyval = 0xC2,

        /// <summary>
        /// Throw ArithmeticException if value is not a finite number.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ckfinite?view=net-6.0"/>
        /// </remarks>
        Ckfinite,

        /// <summary>
        /// Push a typed reference to ptr of type class onto the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.mkrefany?view=net-6.0"/>
        /// </remarks>
        Mkrefany = 0xC6,

        /// <summary>
        /// Convert metadata token to its runtime representation.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldtoken?view=net-6.0"/>
        /// </remarks>
        Ldtoken = 0xD0,

        /// <summary>
        /// Convert to unsigned int16, pushing int32 on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_u2?view=net-6.0"/>
        /// </remarks>
        Conv_U2,

        /// <summary>
        /// Convert to unsigned int8, pushing int32 on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_u1?view=net-6.0"/>
        /// </remarks>
        Conv_U1,

        /// <summary>
        /// Convert to native int, pushing native int on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_i?view=net-6.0"/>
        /// </remarks>
        Conv_I,

        /// <summary>
        /// Convert to a native int (on the stack as native int) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_i?view=net-6.0"/>
        /// </remarks>
        Conv_Ovf_I,

        /// <summary>
        /// Convert to a native unsigned int (on the stack as native int) and throw an exception on overflow.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_ovf_u?view=net-6.0"/>
        /// </remarks>
        Conv_Ovf_U,

        /// <summary>
        /// Add signed integer values with overflow check.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.add_ovf?view=net-6.0"/>
        /// </remarks>
        Add_Ovf,

        /// <summary>
        /// Add unsigned integer values with overflow check.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.add_ovf_un?view=net-6.0"/>
        /// </remarks>
        Add_Ovf_Un,

        /// <summary>
        /// Multiply signed integer values. Signed result shall fit in same size.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.mul_ovf?view=net-6.0"/>
        /// </remarks>
        Mul_Ovf,

        /// <summary>
        /// Multiply unsigned integer values. Unsigned result shall fit in same size.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.mul_ovf_un?view=net-6.0"/>
        /// </remarks>
        Mul_Ovf_Un,

        /// <summary>
        /// Subtract native int from a native int. Signed result shall fit in same size.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.sub_ovf?view=net-6.0"/>
        /// </remarks>
        Sub_Ovf,

        /// <summary>
        /// Subtract native unsigned int from a native unsigned int. Unsigned result shall fit in same size.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.sub_ovf_un?view=net-6.0"/>
        /// </remarks>
        Sub_Ovf_Un,

        /// <summary>
        /// End finally clause of an exception block.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.endfinally?view=net-6.0"/>
        /// </remarks>
        Endfinally,

        /// <summary>
        /// Exit a protected region of code.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.leave?view=net-6.0"/>
        /// </remarks>
        Leave,

        /// <summary>
        /// Exit a protected region of code, short form.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.leave_s?view=net-6.0"/>
        /// </remarks>
        Leave_S,

        /// <summary>
        /// Store value of type native int into memory at address.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stind_i?view=net-6.0"/>
        /// </remarks>
        Stind_I,

        /// <summary>
        /// Convert to native unsigned int, pushing native int on stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.conv_u?view=net-6.0"/>
        /// </remarks>
        Conv_U,

        /// <summary>
        /// This prefix opcode is reserved and currently not implemented in the runtime 
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.prefix7?view=net-6.0"/>
        /// </remarks>
        Prefix7 = 0xF8,

        /// <summary>
        /// This prefix opcode is reserved and currently not implemented in the runtime 
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.prefix6?view=net-6.0"/>
        /// </remarks>
        Prefix6,

        /// <summary>
        /// This prefix opcode is reserved and currently not implemented in the runtime 
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.prefix5?view=net-6.0"/>
        /// </remarks>
        Prefix5,

        /// <summary>
        /// This prefix opcode is reserved and currently not implemented in the runtime 
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.prefix4?view=net-6.0"/>
        /// </remarks>
        Prefix4,

        /// <summary>
        /// This prefix opcode is reserved and currently not implemented in the runtime 
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.prefix3?view=net-6.0"/>
        /// </remarks>
        Prefix3,

        /// <summary>
        /// This prefix opcode is reserved and currently not implemented in the runtime 
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.prefix2?view=net-6.0"/>
        /// </remarks>
        Prefix2,

        /// <summary>
        /// This prefix opcode is reserved and currently not implemented in the runtime 
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.prefix1?view=net-6.0"/>
        /// </remarks>
        Prefix1,

        /// <summary>
        /// This prefix opcode is reserved and currently not implemented in the runtime 
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.prefixref?view=net-6.0"/>
        /// </remarks>
        Prefixref,

        /// <summary>
        /// Return argument list handle for the current method.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.arglist?view=net-6.0"/>
        /// </remarks>
        Arglist = 0xFE00,

        /// <summary>
        /// Push 1 (of type int32) if value1 equals value2, else push 0.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ceq?view=net-6.0"/>
        /// </remarks>
        Ceq,

        /// <summary>
        /// Push 1 (of type int32) if value1 greater that value2, else push 0.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.cgt?view=net-6.0"/>
        /// </remarks>
        Cgt,

        /// <summary>
        /// Push 1 (of type int32) if value1 greater that value2, unsigned or unordered, else push 0.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.cgt_un?view=net-6.0"/>
        /// </remarks>
        Cgt_Un,

        /// <summary>
        /// Push 1 (of type int32) if value1 lower than value2, else push 0.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.clt?view=net-6.0"/>
        /// </remarks>
        Clt,

        /// <summary>
        /// Push 1 (of type int32) if value1 lower than value2, unsigned or unordered, else push 0.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.clt_un?view=net-6.0"/>
        /// </remarks>
        Clt_Un,

        /// <summary>
        /// Push a pointer to a method referenced by method, on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldftn?view=net-6.0"/>
        /// </remarks>
        Ldftn,

        /// <summary>
        /// Push address of virtual method on the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldvirtftn?view=net-6.0"/>
        /// </remarks>
        Ldvirtftn,

        /// <summary>
        /// Load argument onto the stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarg?view=net-6.0"/>
        /// </remarks>
        Ldarg = 0xFE09,

        /// <summary>
        /// Fetch the address of the argument indexed.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarga?view=net-6.0"/>
        /// </remarks>
        Ldarga,

        /// <summary>
        /// Store value to the argument.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.starg?view=net-6.0"/>
        /// </remarks>
        Starg,

        /// <summary>
        /// Load local variable of index onto stack.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldloc?view=net-6.0"/>
        /// </remarks>
        Ldloc,

        /// <summary>
        /// Load address of local variable with index index.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldloca?view=net-6.0"/>
        /// </remarks>
        Ldloca,

        /// <summary>
        /// Pop a value from stack into local variable index.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stloc?view=net-6.0"/>
        /// </remarks>
        Stloc,

        /// <summary>
        /// Allocate space from the local memory pool.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.localloc?view=net-6.0"/>
        /// </remarks>
        Localloc,

        /// <summary>
        /// End an exception handling filter clause.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.endfilter?view=net-6.0"/>
        /// </remarks>
        Endfilter = 0xFE11,

        /// <summary>
        /// Subsequent pointer instruction might be unaligned.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.unaligned?view=net-6.0"/>
        /// </remarks>
        Unaligned,

        /// <summary>
        /// Subsequent pointer reference is volatile.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.volatile?view=net-6.0"/>
        /// </remarks>
        Volatile,

        /// <summary>
        /// Subsequent call terminates current method.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.tailcall?view=net-6.0"/>
        /// </remarks>
        Tailcall,

        /// <summary>
        /// Initialize the value at address dest.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.initobj?view=net-6.0"/>
        /// </remarks>
        Initobj,

        /// <summary>
        /// Call a virtual method on a type constrained to be type T.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.constrained?view=net-6.0"/>
        /// </remarks>
        Constrained,

        /// <summary>
        /// Copy data from memory to memory.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.cpblk?view=net-6.0"/>
        /// </remarks>
        Cpblk,

        /// <summary>
        /// Set all bytes in a block of memory to a given byte value.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.initblk?view=net-6.0"/>
        /// </remarks>
        Initblk,

        /// <summary>
        /// Rethrow the current exception.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.rethrow?view=net-6.0"/>
        /// </remarks>
        Rethrow = 0xFE1A,

        /// <summary>
        /// Push the size, in bytes, of a type as an unsigned int32.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.sizeof?view=net-6.0"/>
        /// </remarks>
        Sizeof = 0xFE1C,

        /// <summary>
        /// Push the type token stored in a typed reference.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.refanytype?view=net-6.0"/>
        /// </remarks>
        Refanytype,

        /// <summary>
        /// Specify that the subsequent array address operation performs no type check at runtime, and that it returns a controlled-mutability managed pointer.
        /// </summary>
        /// <remarks>
        /// See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.readonly?view=net-6.0"/>
        /// </remarks>
        Readonly
    }

    /// <summary>
    /// Provides extensions for the <see cref="CilCode"/> enum.
    /// </summary>
    public static class CilCodeExtensions
    {
        /// <summary>
        /// Transforms the raw CIL code to a <see cref="CilOpCode"/>.
        /// </summary>
        /// <param name="code">The code to convert.</param>
        /// <returns>The operation code.</returns>
        public static CilOpCode ToOpCode(this CilCode code)
        {
            if (code < (CilCode) 0x100)
                return CilOpCodes.SingleByteOpCodes[(int) code];
            return CilOpCodes.MultiByteOpCodes[(int) code - 0xFE00];
        }
    }
}
