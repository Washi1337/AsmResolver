using AsmResolver.IO;
using AsmResolver.PE.DotNet.Cil;

namespace AsmResolver.DotNet.Code.Cil
{
    /// <summary>
    /// Represents a region of code in a CIL method body that is protected by a (filtered) exception handler, finally or
    /// faulting clause.
    /// </summary>
    public class CilExceptionHandler
    {
        /// <summary>
        /// The size in bytes of an exception handler using the tiny format.
        /// </summary>
        public const uint TinyExceptionHandlerSize = 2 * sizeof(byte) + 3 * sizeof(ushort) + sizeof(uint);

        /// <summary>
        /// The size in bytes of an exception handler using the fat format.
        /// </summary>
        public const uint FatExceptionHandlerSize = 6 * sizeof(uint);

        /// <summary>
        /// Reads a single exception handler from the provided input stream.
        /// </summary>
        /// <param name="module">The module the exception handler is defined in.</param>
        /// <param name="body">The method body containing the exception handler.</param>
        /// <param name="reader">The input stream.</param>
        /// <param name="isFat"><c>true</c> if the fat format should be used, <c>false</c> otherwise.</param>
        /// <returns>The exception handler.</returns>
        public static CilExceptionHandler FromReader(ModuleDefinition module, CilMethodBody body,
            ref BinaryStreamReader reader, bool isFat)
        {
            CilExceptionHandlerType handlerType;
            int tryStartOffset;
            int tryEndOffset;
            int handlerStartOffset;
            int handlerEndOffset;

            // Read raw structure.
            if (isFat)
            {
                handlerType = (CilExceptionHandlerType) reader.ReadUInt32();
                tryStartOffset = reader.ReadInt32();
                tryEndOffset = tryStartOffset + reader.ReadInt32();
                handlerStartOffset = reader.ReadInt32();
                handlerEndOffset = handlerStartOffset + reader.ReadInt32();
            }
            else
            {
                handlerType = (CilExceptionHandlerType) reader.ReadUInt16();
                tryStartOffset = reader.ReadUInt16();
                tryEndOffset = tryStartOffset + reader.ReadByte();
                handlerStartOffset = reader.ReadUInt16();
                handlerEndOffset = handlerStartOffset + reader.ReadByte();
            }

            int exceptionTokenOrFilterStart = reader.ReadInt32();

            // Create handler.
            var handler = new CilExceptionHandler
            {
                HandlerType = handlerType,
                TryStart = body.Instructions.GetLabel(tryStartOffset),
                TryEnd = body.Instructions.GetLabel(tryEndOffset),
                HandlerStart = body.Instructions.GetLabel(handlerStartOffset),
                HandlerEnd = body.Instructions.GetLabel(handlerEndOffset),
            };

            // Interpret last field.
            switch (handler.HandlerType)
            {
                case CilExceptionHandlerType.Exception when module.TryLookupMember(exceptionTokenOrFilterStart, out var member):
                    handler.ExceptionType = member as ITypeDefOrRef;
                    break;
                case CilExceptionHandlerType.Filter:
                    handler.FilterStart = body.Instructions.GetByOffset(exceptionTokenOrFilterStart)?.CreateLabel()
                                          ?? new CilOffsetLabel(exceptionTokenOrFilterStart);
                    break;
            }

            return handler;
        }

        /// <summary>
        /// Gets or sets the type of the protected region.
        /// </summary>
        /// <remarks>
        /// This property determines whether the <see cref="FilterStart"/> and/or <see cref="ExceptionType"/> properties
        /// have any meaning and are persisted or not.
        /// </remarks>
        public CilExceptionHandlerType HandlerType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the instruction that marks the start of the the protected region.
        /// </summary>
        public ICilLabel? TryStart
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the instruction that marks the end of the protected region.
        /// </summary>
        /// <remarks>
        /// This instruction marker is exclusive; the referenced instruction does not belong to the protected
        /// region anymore.
        /// </remarks>
        public ICilLabel? TryEnd
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the instruction that marks the start of the handler region.
        /// </summary>
        public ICilLabel? HandlerStart
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the instruction that marks the end of the handler region.
        /// </summary>
        /// <remarks>
        /// This instruction marker is exclusive; the referenced instruction does not belong to the handler
        /// region anymore.
        /// </remarks>
        public ICilLabel? HandlerEnd
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the instruction that marks the start of the filter region.
        /// </summary>
        /// <remarks>
        /// This property only has meaning if the <see cref="HandlerType"/> property is set to
        /// <see cref="CilExceptionHandlerType.Filter"/>.
        /// </remarks>
        public ICilLabel? FilterStart
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of exceptions that this exception handler catches.
        /// </summary>
        /// <remarks>
        /// This property only has meaning if the <see cref="HandlerType"/> property is set to
        /// <see cref="CilExceptionHandlerType.Exception"/>.
        /// </remarks>
        public ITypeDefOrRef? ExceptionType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the exception handler requires the fat format to be used to encode the
        /// boundaries of the protected region.
        /// </summary>
        public bool IsFat =>
            TryStart?.Offset >= ushort.MaxValue
            || HandlerStart?.Offset >= ushort.MaxValue
            || TryEnd?.Offset - TryStart?.Offset >= byte.MaxValue
            || HandlerEnd?.Offset - HandlerStart?.Offset >= byte.MaxValue;

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("{0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}, {8}: {9}, {10}: {11}, {12}: {13}",
                nameof(HandlerType), HandlerType,
                nameof(TryStart), TryStart,
                nameof(TryEnd), TryEnd,
                nameof(HandlerStart), HandlerStart,
                nameof(HandlerEnd), HandlerEnd,
                nameof(FilterStart), FilterStart,
                nameof(ExceptionType), ExceptionType);
        }
    }
}
