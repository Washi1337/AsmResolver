using System;

namespace AsmResolver.DotNet.Code.Cil
{
    public class StackImbalanceException : Exception
    {
        public StackImbalanceException(CilMethodBody body, int offset)
            : base(string.Format("Stack imbalance was detected at offset IL_{0:X4} in method body of {1}",
                offset, body.Owner))
        {
            Body = body;
            Offset = offset;
        }

        public CilMethodBody Body
        {
            get;
        }

        public int Offset
        {
            get;
        }
    }
}