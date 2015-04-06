using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.Net.Msil
{
    public struct MsilOpCode : IEquatable<MsilOpCode>
    {
        // taken from System.Reflection.Emit.OpCode

        internal MsilOpCode(MsilCode value, int flags)
            : this()
        {
            Name = value.ToString().ToLowerInvariant().Replace('_', '.');
	        StackBehaviourPop = (MsilStackBehaviour)(flags >> 12 & 31);
            StackBehaviourPush = (MsilStackBehaviour)(flags >> 17 & 31);
	        OperandType = (MsilOperandType)(flags & 31);
	        OpCodeType = (MsilOpCodeType)(flags >> 9 & 7);
	        Size = (flags >> 22 & 3);
	        Op1 = (byte)((ushort)value >> 8);
	        Op2 = (byte)value;
            Code = value;
	        FlowControl = (MsilFlowControl)(flags >> 5 & 15);
	        // m_endsUncondJmpBlk = ((flags & 16777216) != 0);
	        // m_stackChange = flags >> 28;

            if (Size == 1)
                MsilOpCodes.SingleByteOpCodes[Op2] = this;
            else
                MsilOpCodes.MultiByteOpCodes[Op2] = this;
        }

        public MsilCode Code
        {
            get;
            set;
        }

        public string Name
        {
            get;
            private set;
        }

        public MsilStackBehaviour StackBehaviourPop
        {
            get;
            private set;
        }

        public MsilStackBehaviour StackBehaviourPush
        {
            get;
            private set;
        }

        public MsilOperandType OperandType
        {
            get;
            private set;
        }

        public MsilOpCodeType OpCodeType
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

        public MsilFlowControl FlowControl
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return Name;
        }

        public bool Equals(MsilOpCode other)
        {
            return Code == other.Code;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is MsilOpCode && Equals((MsilOpCode)obj);
        }

        public override int GetHashCode()
        {
            return (int)Code;
        }
    }
}
