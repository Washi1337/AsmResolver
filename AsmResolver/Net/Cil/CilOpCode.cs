using System;

namespace AsmResolver.Net.Cil
{
    /// <summary>
    /// Represents a single CIL operation code used in a CIL instruction.
    /// </summary>
    public struct CilOpCode : IEquatable<CilOpCode>
    {
        public static bool operator ==(CilOpCode a, CilOpCode b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(CilOpCode a, CilOpCode b)
        {
            return !a.Equals(b);
        }

        // taken from System.Reflection.Emit.OpCode

        internal CilOpCode(CilCode value, int flags)
            : this()
        {
            Name = value.ToString().ToLowerInvariant().Replace('_', '.');
	        StackBehaviourPop = (CilStackBehaviour)(flags >> 12 & 31);
            StackBehaviourPush = (CilStackBehaviour)(flags >> 17 & 31);
	        OperandType = (CilOperandType)(flags & 31);
	        OpCodeType = (CilOpCodeType)(flags >> 9 & 7);
	        Size = (flags >> 22 & 3);
	        Op1 = (byte)((ushort)value >> 8);
	        Op2 = (byte)value;
            Code = value;
	        FlowControl = (CilFlowControl)(flags >> 5 & 15);
	        // m_endsUncondJmpBlk = ((flags & 16777216) != 0);
	        // m_stackChange = flags >> 28;

            if (Size == 1)
                CilOpCodes.SingleByteOpCodes[Op2] = this;
            else
                CilOpCodes.MultiByteOpCodes[Op2] = this;
        }

        /// <summary>
        /// Gets the actual code value of the operation code, as used in the binary representation.
        /// </summary>
        public CilCode Code
        {
            get;
        }

        /// <summary>
        /// Gets the mnemonic of the operation code, as used in an assembler.
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating the effects the operation code has regarding the removal of values from the stack.
        /// </summary>
        public CilStackBehaviour StackBehaviourPop
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating the effects the operation code has regarding the addition of values to the stack.
        /// </summary>
        public CilStackBehaviour StackBehaviourPush
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating the type of operand an instruction using the operation code has.
        /// </summary>
        public CilOperandType OperandType
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating the category the operation code falls into.
        /// </summary>
        public CilOpCodeType OpCodeType
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating the size in bytes the operation code uses. 
        /// </summary>
        /// <remarks>
        /// This size excludes the operand.
        /// </remarks>
        public int Size
        {
            get;
        }

        /// <summary>
        /// Gets the prefix byte used in the binary representation of the operation code (if available).
        /// </summary>
        public byte Op1
        {
            get;
        }

        /// <summary>
        /// Gets the primary byte used in the binary representation of the operation code.
        /// </summary>
        public byte Op2
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating the effects the instruction has regarding the flow of the program.
        /// </summary>
        public CilFlowControl FlowControl
        {
            get;
        }

        public override string ToString()
        {
            return Name;
        }

        public bool Equals(CilOpCode other)
        {
            return Code == other.Code;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is CilOpCode code && Equals(code);
        }

        public override int GetHashCode()
        {
            return (int) Code;
        }
    }
}
