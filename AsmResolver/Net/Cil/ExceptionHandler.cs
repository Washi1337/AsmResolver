using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cil
{
    public class ExceptionHandler
    {
        public const int SmallExceptionHandlerSize = 2 * sizeof(byte) + 3 * sizeof(ushort) + sizeof(uint);
        public const int FatExceptionHandlerSize = 6 * sizeof(uint);
        
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

        public bool IsFat
        {
            get;
            set;
        }

        public bool IsFatFormatRequired => TryEnd.Offset - TryStart.Offset > byte.MaxValue 
                                           || HandlerStart.Offset - HandlerEnd.Offset > byte.MaxValue
                                           || TryStart.Offset > ushort.MaxValue 
                                           || TryEnd.Offset > ushort.MaxValue 
                                           || HandlerStart.Offset > ushort.MaxValue 
                                           || HandlerEnd.Offset > ushort.MaxValue;

        public ExceptionHandlerType HandlerType
        {
            get;
            set;
        }

        public CilInstruction TryStart
        {
            get;
            set;
        }

        public CilInstruction TryEnd
        {
            get;
            set;
        }

        public CilInstruction HandlerStart
        {
            get;
            set;
        }

        public CilInstruction HandlerEnd
        {
            get;
            set;
        }

        public ITypeDefOrRef CatchType
        {
            get;
            set;
        }

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
