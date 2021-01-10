using System;
using System.Runtime.Serialization;
using AsmResolver.PE.DotNet.Cil;

namespace AsmResolver.DotNet.Code.Cil
{
    /// <summary>
    /// Represents the exception that occurs when an invalid CIL instruction was constructed.
    /// </summary>
    [Serializable]
    public class InvalidCilInstructionException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="InvalidCilInstructionException"/> class.
        /// </summary>
        public InvalidCilInstructionException()
            : base("Invalid combination of CIL operation code and operand in the instruction.")
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="InvalidCilInstructionException"/> class.
        /// </summary>
        /// <param name="code">The operation code that was attempted to create an instruction with.</param>
        public InvalidCilInstructionException(CilOpCode code)
            : base(code.OperandType == CilOperandType.InlineNone
                ? $"Operation code {code.Mnemonic} cannot have an operand."
                : $"Operation code {code.Mnemonic} requires an {code.OperandType} operand.")
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="InvalidCilInstructionException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public InvalidCilInstructionException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="InvalidCilInstructionException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner cause of the exception.</param>
        public InvalidCilInstructionException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <inheritdoc />
        protected InvalidCilInstructionException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}