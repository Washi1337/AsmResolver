using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cil
{
    /// <summary>
    /// Represents a single exception handler in a CIL method body.
    /// </summary>
    public class ExceptionHandler
    {
        public const int SmallExceptionHandlerSize = 2 * sizeof(byte) + 3 * sizeof(ushort) + sizeof(uint);
        public const int FatExceptionHandlerSize = 6 * sizeof(uint);
        
        /// <summary>
        /// Reads a single exception handler from the given input stream.
        /// </summary>
        /// <param name="cilMethodBody">The method body that contains the exception handler.</param>
        /// <param name="reader">The input stream to read from.</param>
        /// <param name="fatFormat">A value indicating whether the fat format or the small format should be used.</param>
        /// <returns>The exception handler.</returns>
        public static ExceptionHandler FromReader(CilMethodBody cilMethodBody, IBinaryStreamReader reader, bool fatFormat)
        {
            var handlerType = (ExceptionHandlerType) (fatFormat ? reader.ReadUInt32() : reader.ReadUInt16());
            int tryOffset = fatFormat ? reader.ReadInt32() : reader.ReadUInt16();
            int tryLength = fatFormat ? reader.ReadInt32() : reader.ReadByte();
            int handlerOffset = fatFormat ? reader.ReadInt32() : reader.ReadUInt16();
            int handlerLength = fatFormat ? reader.ReadInt32() : reader.ReadByte();
            uint classTokenOrFilterOffset = reader.ReadUInt32();

            var handler = new ExceptionHandler(handlerType)
            {
                IsFat = fatFormat,
                TryStart = cilMethodBody.Instructions.GetByOffset(tryOffset),
                TryEnd = cilMethodBody.Instructions.GetByOffset(tryOffset + tryLength),
                HandlerStart = cilMethodBody.Instructions.GetByOffset(handlerOffset),
                HandlerEnd = cilMethodBody.Instructions.GetByOffset(handlerOffset + handlerLength),
            };

            switch (handler.HandlerType)
            {
                case ExceptionHandlerType.Exception:
                    handler.CatchType = (ITypeDefOrRef) ((IOperandResolver) cilMethodBody).ResolveMember(
                            new MetadataToken(classTokenOrFilterOffset));
                    break;
                case ExceptionHandlerType.Filter:
                    handler.FilterStart = cilMethodBody.Instructions.GetByOffset((int)classTokenOrFilterOffset);
                    break;
            }

            return handler;
        } 

        public ExceptionHandler(ExceptionHandlerType handlerType)
        {
            HandlerType = handlerType;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the exception handler is using the fat format or not.
        /// </summary>
        public bool IsFat
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the fat format is required for the exception handler.
        /// </summary>
        public bool IsFatFormatRequired => TryEnd.Offset - TryStart.Offset > byte.MaxValue 
                                           || HandlerStart.Offset - HandlerEnd.Offset > byte.MaxValue
                                           || TryStart.Offset > ushort.MaxValue 
                                           || TryEnd.Offset > ushort.MaxValue 
                                           || HandlerStart.Offset > ushort.MaxValue 
                                           || HandlerEnd.Offset > ushort.MaxValue;

        /// <summary>
        /// Gets or sets the exception handler type.
        /// </summary>
        public ExceptionHandlerType HandlerType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the instruction that marks the start of the try block.
        /// </summary>
        public CilInstruction TryStart
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the instruction that marks the end of the try block.
        /// </summary>
        /// <remarks>
        /// This instruction is not included by the try block itself.
        /// </remarks>
        public CilInstruction TryEnd
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the instruction that marks the start of the handler block.
        /// </summary>
        public CilInstruction HandlerStart
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the instruction that marks the end of the handler block.
        /// </summary>
        /// <remarks>
        /// This instruction is not included by the handler block itself.
        /// </remarks>
        public CilInstruction HandlerEnd
        {
            get;
            set;
        }

        /// <summary>
        /// When <see cref="HandlerType"/> equals <see cref="ExceptionHandlerType.Exception"/>, gets or sets the type of
        /// the exception that is caught by the handler.  
        /// </summary>
        public ITypeDefOrRef CatchType
        {
            get;
            set;
        }

        /// <summary>
        /// When <see cref="HandlerType"/> equals <see cref="ExceptionHandlerType.Filter"/>, gets or sets the
        /// instruction that marks the start of the filter block.
        /// </summary>
        public CilInstruction FilterStart
        {
            get;
            set;
        }

        public uint GetPhysicalLength()
        {
            return IsFat ? FatExceptionHandlerSize : (uint) SmallExceptionHandlerSize;
        }

        public void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            if (IsFat)
            {
                writer.WriteUInt32((uint) HandlerType);
                writer.WriteUInt32((uint) TryStart.Offset);
                writer.WriteUInt32((uint) (TryEnd.Offset - TryStart.Offset));
                writer.WriteUInt32((uint) HandlerStart.Offset);
                writer.WriteUInt32((uint) (HandlerEnd.Offset - HandlerStart.Offset));
            }
            else
            {
                writer.WriteUInt16((ushort) HandlerType);
                writer.WriteUInt16((ushort) TryStart.Offset);
                writer.WriteByte((byte) (TryEnd.Offset - TryStart.Offset));
                writer.WriteUInt16((ushort) HandlerStart.Offset);
                writer.WriteByte((byte) (HandlerEnd.Offset - HandlerStart.Offset));
            }

            switch (HandlerType)
            {
                case ExceptionHandlerType.Exception:
                    writer.WriteUInt32(buffer.TableStreamBuffer.GetTypeToken(CatchType).ToUInt32());
                    break;
                case ExceptionHandlerType.Filter:
                    writer.WriteUInt32((uint) FilterStart.Offset);
                    break;
                default:
                    writer.WriteUInt32(0);
                    break;
            }
        }
    }
}
