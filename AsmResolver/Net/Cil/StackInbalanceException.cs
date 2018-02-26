using System;

namespace AsmResolver.Net.Cil
{
    public class StackInbalanceException : Exception
    {
        public StackInbalanceException(CilMethodBody body, int offset)
            : base(string.Format("Stack inbalance was detected at offset {0:X4} in method body of {1}", offset,
                body.Method))
        {
            Body = body;
            Offset = offset;
        }

        public CilMethodBody Body
        {
            get;
            private set;
        }

        public int Offset
        {
            get;
            private set;
        }
    }
}
