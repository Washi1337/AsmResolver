using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cil
{
    /// <summary>
    /// Represents a single instruction found in a CIL method body. This includes offset, operation code and operand. 
    /// </summary>
    public sealed class CilInstruction
    {
        private static readonly ISet<CilCode> LdcI4Codes = new HashSet<CilCode>(new[]
        {
            CilCode.Ldc_I4,
            CilCode.Ldc_I4_0,
            CilCode.Ldc_I4_1,
            CilCode.Ldc_I4_2,
            CilCode.Ldc_I4_3,
            CilCode.Ldc_I4_4,
            CilCode.Ldc_I4_5,
            CilCode.Ldc_I4_6,
            CilCode.Ldc_I4_7,
            CilCode.Ldc_I4_8,
            CilCode.Ldc_I4_S,
            CilCode.Ldc_I4_M1,
        });
        
        public static CilInstruction Create(CilOpCode code)
        {
            if (code.OperandType != CilOperandType.InlineNone)
                throw new ArgumentException("Opcode requires an operand.", "code");
            return new CilInstruction(0, code, null);
        }

        public static CilInstruction Create(CilOpCode code, CilInstruction instruction)
        {
            if (code.OperandType != CilOperandType.ShortInlineBrTarget && code.OperandType != CilOperandType.InlineBrTarget)
                throw new ArgumentException("Opcode does not accept an instruction operand.", "code");
            return new CilInstruction(0, code, instruction);
        }

        public static CilInstruction Create(CilOpCode code, IMemberReference operand)
        {
            switch (code.OperandType)
            {
                case CilOperandType.InlineField:
                case CilOperandType.InlineMethod:
                case CilOperandType.InlineTok:
                case CilOperandType.InlineType:
                    return new CilInstruction(0, code, operand);
            }
            throw new ArgumentException("Opcode does not accept a member operand operand.", "code");
        }

        public static CilInstruction Create(CilOpCode code, sbyte operand)
        {
            if (code.OperandType != CilOperandType.ShortInlineI)
                throw new ArgumentException("Opcode does not accept an int8 operand.", "code");
            return new CilInstruction(0, code, operand);
        }

        public static CilInstruction Create(CilOpCode code, int operand)
        {
            if (code.OperandType != CilOperandType.InlineI)
                throw new ArgumentException("Opcode does not accept an int32 operand.", "code");
            return new CilInstruction(0, code, operand);
        }

        public static CilInstruction Create(CilOpCode code, long operand)
        {
            if (code.OperandType != CilOperandType.InlineI8)
                throw new ArgumentException("Opcode does not accept an int64 operand.", "code");
            return new CilInstruction(0, code, operand);
        }

        public static CilInstruction Create(CilOpCode code, float operand)
        {
            if (code.OperandType != CilOperandType.ShortInlineR)
                throw new ArgumentException("Opcode does not accept a float32 operand.", "code");
            return new CilInstruction(0, code, operand);
        }

        public static CilInstruction Create(CilOpCode code, double operand)
        {
            if (code.OperandType != CilOperandType.InlineR)
                throw new ArgumentException("Opcode does not accept a float64 operand.", "code");
            return new CilInstruction(0, code, operand);
        }

        public static CilInstruction Create(CilOpCode code, string operand)
        {
            if (code.OperandType != CilOperandType.InlineString)
                throw new ArgumentException("Opcode does not accept a string operand.", "code");
            return new CilInstruction(0, code, operand);
        }

        public static CilInstruction Create(CilOpCode code, IEnumerable<CilInstruction> operand)
        {
            if (code.OperandType != CilOperandType.InlineSwitch)
                throw new ArgumentException("Opcode does not accept a collection of instructions as operand.", "code");
            return new CilInstruction(0, code, new List<CilInstruction>(operand));
        }

        public static CilInstruction Create(CilOpCode code, VariableSignature operand)
        {
            if (code.OperandType != CilOperandType.InlineVar && code.OperandType != CilOperandType.ShortInlineVar)
                throw new ArgumentException("Opcode does not accept a local variable operand.", "code");
            return new CilInstruction(0, code, operand);
        }

        public static CilInstruction Create(CilOpCode code, ParameterSignature operand)
        {
            if (code.OperandType != CilOperandType.InlineArgument && code.OperandType != CilOperandType.ShortInlineArgument)
                throw new ArgumentException("Opcode does not accept a parameter operand.", "code");
            return new CilInstruction(0, code, operand);
        }

        public static CilInstruction Create(CilOpCode code, StandAloneSignature operand)
        {
            if (code.OperandType != CilOperandType.InlineSig)
                throw new ArgumentException("Opcode does not accept a signature operand.", "code");
            return new CilInstruction(0, code, operand);
        }

        public CilInstruction(int offset, CilOpCode opCode, object operand)
        {
            Offset = offset;
            OpCode = opCode;
            Operand = operand;
        }

        /// <summary>
        /// Gets or sets the offset of the CIL instruction inside the method body.
        /// </summary>
        public int Offset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the operation code that is used upon evaluation of the instruction. 
        /// </summary>
        public CilOpCode OpCode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the operand that is used by the operation upon evaluation of the instruction. 
        /// </summary>
        public object Operand
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the amount of bytes the instruction uses to encode itself.
        /// </summary>
        public int Size => OpCode.Size + GetOperandSize();

        /// <summary>
        /// Gets a value indicating whether the instruction is an ldc.i4 variant. That is, it loads an int32 onto the
        /// stack.
        /// </summary>
        public bool IsLdcI4 => LdcI4Codes.Contains(OpCode.Code);

        /// <summary>
        /// When the instruction is an ldc.i4 variant ,gets the integer value that is pushed onto the stack.
        /// </summary>
        /// <returns>The integer that is pushed onto the stack.</returns>
        /// <exception cref="ArgumentException">Occurs when the instruction is not an ldc.i4 instruction.</exception>
        /// <remarks>
        /// This includes extracting the integer value that is pushed by single byte macros of the ldc.i4 instruction.
        /// </remarks>
        public int GetLdcValue()
        {
            switch (OpCode.Code)
            {
                case CilCode.Ldc_I4_M1:
                    return -1;
                case CilCode.Ldc_I4_0:
                    return 0;
                case CilCode.Ldc_I4_1:
                    return 1;
                case CilCode.Ldc_I4_2:
                    return 2;
                case CilCode.Ldc_I4_3:
                    return 3;
                case CilCode.Ldc_I4_4:
                    return 4;
                case CilCode.Ldc_I4_5:
                    return 5;
                case CilCode.Ldc_I4_6:
                    return 6;
                case CilCode.Ldc_I4_7:
                    return 7;
                case CilCode.Ldc_I4_8:
                    return 8;
                case CilCode.Ldc_I4:
                case CilCode.Ldc_I4_S:
                    return Convert.ToInt32(Operand);
                default:
                    throw new ArgumentException("Instruction is not an ldc.i4 variant.");
            }
        }

        /// <summary>
        /// Calculates the size in bytes of the operand.
        /// </summary>
        /// <returns>The amount of bytes to encode the operand.</returns>
        /// <exception cref="NotSupportedException">Occurs when an invalid or unsupported instruction has been provided.</exception>
        public int GetOperandSize()
        {
            switch (OpCode.OperandType)
            {
                case CilOperandType.InlineNone:
                    return 0;
                case CilOperandType.ShortInlineArgument:
                case CilOperandType.ShortInlineVar:
                case CilOperandType.ShortInlineI:
                case CilOperandType.ShortInlineBrTarget:
                    return 1;
                case CilOperandType.InlineVar:
                case CilOperandType.InlineArgument:
                    return 2;
                case CilOperandType.ShortInlineR:
                case CilOperandType.InlineI:
                case CilOperandType.InlineField:
                case CilOperandType.InlineMethod:
                case CilOperandType.InlineSig:
                case CilOperandType.InlineTok:
                case CilOperandType.InlineType:
                case CilOperandType.InlineString:
                case CilOperandType.InlineBrTarget:
                    return 4;

                case CilOperandType.InlineR:
                case CilOperandType.InlineI8:
                    return 8;

                case CilOperandType.InlineSwitch:
                    var array = Operand as IList;
                    if (array == null)
                        return 4;
                    return 4 * (array.Count + 1);
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// Attempts to stringify the operand.
        /// </summary>
        /// <returns>The stringified operand.</returns>
        /// <exception cref="NotSupportedException">Occurs when an invalid or unsupported operand type is provided.</exception>
        public string OperandToString()
        {
            if (Operand == null)
                return string.Empty;

            switch (OpCode.OperandType)
            {
                case CilOperandType.InlineNone:
                    return string.Empty;

                case CilOperandType.InlineArgument:
                case CilOperandType.ShortInlineArgument:
                case CilOperandType.InlineVar:
                case CilOperandType.ShortInlineVar:
                    // TODO: return index

                case CilOperandType.InlineR:
                case CilOperandType.ShortInlineR:
                case CilOperandType.InlineI:
                case CilOperandType.InlineI8:
                case CilOperandType.ShortInlineI:
                    return Convert.ToString(Operand, CultureInfo.InvariantCulture);

                case CilOperandType.InlineField:
                case CilOperandType.InlineMethod:
                case CilOperandType.InlineSig:
                case CilOperandType.InlineTok:
                case CilOperandType.InlineType:
                    return Operand is IMetadataMember member
                        ? member.ToString()
                        : "TOKEN<0x" + ((MetadataToken) Operand) + ">";

                case CilOperandType.InlineString:
                    return Operand is string
                        ? $"\"{Operand}\""
                        : "TOKEN<0x" + ((MetadataToken) Operand) + ">";
                case CilOperandType.InlineSwitch:
                    return string.Join(", ",
                        ((IList<CilInstruction>) Operand).Select(x => "IL_" + x.Offset.ToString("X4")));

                case CilOperandType.InlineBrTarget:
                case CilOperandType.ShortInlineBrTarget:
                    return "IL_" + ((CilInstruction) Operand).Offset.ToString("X4");
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// Determines the amount of values popped from the stack by this instruction.
        /// </summary>
        /// <param name="parent">The parent method body the instruction is residing in.</param>
        /// <returns>A non-negative number representing the amount of values popped from the stack.</returns>
        public int GetStackPopCount(CilMethodBody parent)
        {
            MethodSignature signature = null;
            var member = Operand as ICallableMemberReference;
            var standalone = Operand as StandAloneSignature;
            if (member != null)
                signature = member.Signature as MethodSignature;
            else if (standalone != null)
                signature = standalone.Signature as MethodSignature;

            switch (OpCode.StackBehaviourPop)
            {
                case CilStackBehaviour.Pop1:
                case CilStackBehaviour.Popi:
                case CilStackBehaviour.Popref:
                    return 1;
                
                case CilStackBehaviour.Pop1_pop1:
                case CilStackBehaviour.Popi_pop1:
                case CilStackBehaviour.Popi_popi:
                case CilStackBehaviour.Popi_popi8:
                case CilStackBehaviour.Popi_popr4:
                case CilStackBehaviour.Popi_popr8:
                case CilStackBehaviour.Popref_pop1:
                case CilStackBehaviour.Popref_popi:
                    return 2;
                
                case CilStackBehaviour.Popi_popi_popi:
                case CilStackBehaviour.Popref_popi_pop1:
                case CilStackBehaviour.Popref_popi_popi:
                case CilStackBehaviour.Popref_popi_popi8:
                case CilStackBehaviour.Popref_popi_popr4:
                case CilStackBehaviour.Popref_popi_popr8:
                case CilStackBehaviour.Popref_popi_popref:
                    return 3;
                
                case CilStackBehaviour.Varpop:
                    if (signature == null)
                    {
                        if (OpCode.Code == CilCode.Ret)
                            return parent.Method.Signature.ReturnType.IsTypeOf("System", "Void") ? 0 : 1;
                    }
                    else
                    {
                        int count = signature.Parameters.Count;
                        if (signature.HasThis && OpCode.Code != CilCode.Newobj)
                            count++;
                        return count;
                    }

                    break;
            }

            return 0;
        }

        /// <summary>
        /// Determines the amount of values pushed onto the stack by this instruction.
        /// </summary>
        /// <param name="parent">The parent method body the instruction is residing in.</param>
        /// <returns>A non-negative number representing the amount of values pushed onto the stack.</returns>
        public int GetStackPushCount(CilMethodBody parent)
        {
            MethodSignature signature = null;
            var member = Operand as ICallableMemberReference;
            var standalone = Operand as StandAloneSignature;
            if (member != null)
                signature = member.Signature as MethodSignature;
            else if (standalone != null)
                signature = standalone.Signature as MethodSignature;
            
            switch (OpCode.StackBehaviourPush)
            {
                case CilStackBehaviour.Push1:
                case CilStackBehaviour.Pushi:
                case CilStackBehaviour.Pushi8:
                case CilStackBehaviour.Pushr4:
                case CilStackBehaviour.Pushr8:
                case CilStackBehaviour.Pushref:
                    return 1;
                
                case CilStackBehaviour.Push1_push1:
                    return 2;
                
                case CilStackBehaviour.Varpush:
                    if (signature != null
                        && (!signature.ReturnType.IsTypeOf("System", "Void") || OpCode.Code == CilCode.Newobj))
                        return 1;
                    break;
            }

            return 0;
        }
        
        /// <summary>
        /// Determines the stack delta of the instruction. That is, the amount of values pushed onto the stack
        /// subtracted from the amount of values popped from the stack.
        /// </summary>
        /// <param name="parent">The parent method body the instruction is residing in.</param>
        /// <returns>A number representing the stack delta.</returns>
        public int GetStackDelta(CilMethodBody parent)
        {
            return GetStackPushCount(parent) - GetStackPopCount(parent);
        }

        private bool Equals(CilInstruction other)
        {
            return Offset == other.Offset && OpCode.Equals(other.OpCode) && Equals(Operand, other.Operand);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj is CilInstruction other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Offset;
                hashCode = (hashCode * 397) ^ OpCode.GetHashCode();
                hashCode = (hashCode * 397) ^ (Operand != null ? Operand.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"IL_{Offset:X4}: {OpCode.Name}{(Operand == null ? string.Empty : " " + OperandToString())}";
        }
    }
}
