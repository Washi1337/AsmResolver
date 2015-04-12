using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Msil
{
    public class ExceptionHandler : FileSegment
    {
        public static ExceptionHandler FromReader(MethodBody methodBody, IBinaryStreamReader reader, bool fatFormat)
        {
            var offset = reader.Position;
            var handerType = fatFormat ? reader.ReadUInt32() : reader.ReadUInt16();
            var tryOffset = fatFormat ? reader.ReadInt32() : reader.ReadUInt16();
            var tryLength = fatFormat ? reader.ReadInt32() : reader.ReadByte();
            var handlerOffset = fatFormat ? reader.ReadInt32() : reader.ReadUInt16();
            var handlerLength = fatFormat ? reader.ReadInt32() : reader.ReadByte();
            var classTokenOrFilterOffset = reader.ReadUInt32();

            var handler = new ExceptionHandler((ExceptionHandlerType)handerType)
            {
                StartOffset = offset,
                IsFat = fatFormat,
                MethodBody = methodBody,
                TryStart = methodBody.GetInstructionByOffset(tryOffset),
                TryEnd = methodBody.GetInstructionByOffset(tryOffset + tryLength),
                HandlerStart = methodBody.GetInstructionByOffset(handlerOffset),
                HandlerEnd = methodBody.GetInstructionByOffset(handlerOffset + handlerLength),
            };

            switch (handler.HandlerType)
            {
                case ExceptionHandlerType.Exception:
                    handler.CatchType = (ITypeDefOrRef)((IOperandResolver)methodBody).ResolveMember(new MetadataToken(classTokenOrFilterOffset));
                    break;
                case ExceptionHandlerType.Filter:
                    handler.FilterStart = methodBody.GetInstructionByOffset((int)classTokenOrFilterOffset);
                    break;
            }

            return handler;
        }

        public MethodBody MethodBody
        {
            get;
            internal set;
        }

        private ExceptionHandler(ExceptionHandlerType handlerType)
        {
            HandlerType = handlerType;
        }

        public bool IsFat
        {
            get;
            set;
        }

        public bool IsFatFormatRequired
        {
            get
            {
                return ((TryEnd.Offset - TryStart.Offset) > byte.MaxValue) ||
                       ((HandlerStart.Offset - HandlerEnd.Offset) > byte.MaxValue) ||
                       (TryStart.Offset > ushort.MaxValue) ||
                       (TryEnd.Offset > ushort.MaxValue) ||
                       (HandlerStart.Offset > ushort.MaxValue) ||
                       (HandlerEnd.Offset > ushort.MaxValue);
            }
        }

        public ExceptionHandlerType HandlerType
        {
            get;
            set;
        }

        public MsilInstruction TryStart
        {
            get;
            set;
        }

        public MsilInstruction TryEnd
        {
            get;
            set;
        }

        public MsilInstruction HandlerStart
        {
            get;
            set;
        }

        public MsilInstruction HandlerEnd
        {
            get;
            set;
        }

        public ITypeDefOrRef CatchType
        {
            get;
            set;
        }

        public MsilInstruction FilterStart
        {
            get;
            set;
        }

        public override uint GetPhysicalLength()
        {
            if (IsFat)
                return 7 * sizeof (uint);
            
            return 2 * sizeof (ushort) +
                   1 * sizeof (byte) +
                   1 * sizeof (ushort) +
                   1 * sizeof (byte) +
                   2 * sizeof (uint);
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            if (IsFat)
            {
                writer.WriteUInt32((uint)HandlerType);
                writer.WriteUInt32((uint)TryStart.Offset);
                writer.WriteUInt32((uint)(TryEnd.Offset - TryStart.Offset));
                writer.WriteUInt32((uint)HandlerStart.Offset);
                writer.WriteUInt32((uint)(HandlerEnd.Offset - HandlerStart.Offset));
            }
            else
            {
                writer.WriteUInt16((ushort)HandlerType);
                writer.WriteUInt16((ushort)TryStart.Offset);
                writer.WriteByte((byte)(TryEnd.Offset - TryStart.Offset));
                writer.WriteUInt16((ushort)HandlerStart.Offset);
                writer.WriteByte((byte)(HandlerEnd.Offset - HandlerStart.Offset));
            }

            switch (HandlerType)
            {
                case ExceptionHandlerType.Exception:
                    writer.WriteUInt32(CatchType.MetadataToken.ToUInt32());
                    break;
                case ExceptionHandlerType.Filter:
                    writer.WriteUInt32((uint)FilterStart.Offset);
                    break;
                default:
                    writer.WriteUInt32(0);
                    break;
            }
        }
    }
}
