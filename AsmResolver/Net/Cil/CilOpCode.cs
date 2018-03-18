using System;

namespace AsmResolver.Net.Cil
{
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

        public CilCode Code
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public CilStackBehaviour StackBehaviourPop
        {
            get;
            private set;
        }

        public CilStackBehaviour StackBehaviourPush
        {
            get;
            private set;
        }

        public CilOperandType OperandType
        {
            get;
            private set;
        }

        public CilOpCodeType OpCodeType
        {
            get;
            private set;
        }

        public int Size
        {
            get;
            private set;
        }

        public byte Op1
        {
            get;
            private set;
        }

        public byte Op2
        {
            get;
            private set;
        }

        public CilFlowControl FlowControl
        {
            get;
            private set;
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
            return obj is CilOpCode && Equals((CilOpCode)obj);
        }

        public override int GetHashCode()
        {
            return (int)Code;
        }
    }
}
