using System;

namespace AsmResolver.DotNet.Code.Cil
{
    /// <summary>
    /// Represents the exception that occurs when an inconsistency was detected during a stack analysis.
    /// </summary>
    public class StackImbalanceException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="StackImbalanceException"/> class.
        /// </summary>
        /// <param name="body">The method body in which the inconsistency was detected.</param>
        /// <param name="offset">The offset at which the inconsistency was detected.</param>
        public StackImbalanceException(CilMethodBody body, int offset)
            : base(string.Format("Stack imbalance was detected at offset IL_{0:X4} in method body of {1}",
                offset, body.Owner))
        {
            Body = body;
            Offset = offset;
        }

        /// <summary>
        /// Gets the method body in which the inconsistency was detected.
        /// </summary>
        public CilMethodBody Body
        {
            get;
        }

        /// <summary>
        /// Gets the offset at which the inconsistency was detected.
        /// </summary>
        public int Offset
        {
            get;
        }
    }
}