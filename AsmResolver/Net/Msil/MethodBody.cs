using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Services;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Builder;
using AsmResolver.Net.Builder;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Msil
{
    public class MethodBody : FileSegmentBuilder, IOperandResolver
    {
        private sealed class MethodBodyOperandBuilder : IOperandBuilder
        {
            private readonly NetBuildingContext _buildingContext;
            private readonly MethodBody _owner;

            public MethodBodyOperandBuilder(NetBuildingContext buildingContext, MethodBody owner)
            {
                if (buildingContext == null)
                    throw new ArgumentNullException("buildingContext");
                if (owner == null)
                    throw new ArgumentNullException("owner");
                _buildingContext = buildingContext;
                _owner = owner;
            }

            public MetadataToken GetMemberToken(MetadataMember member)
            {
                return member.MetadataToken;
            }

            public uint GetStringOffset(string value)
            {
                return 0x70000000 | _buildingContext.GetStreamBuffer<UserStringStreamBuffer>().GetStringOffset(value);
            }

            public int GetVariableIndex(VariableSignature variable)
            {
                var localVarSig = _owner.Signature != null ? _owner.Signature.Signature as LocalVariableSignature : null;
                if (localVarSig == null)
                    throw new InvalidOperationException("Method body does not have a valid local variable signature.");
                return localVarSig.Variables.IndexOf(variable);
            }

            public int GetParameterIndex(ParameterSignature parameter)
            {
                return _owner.Method.Signature.Parameters.IndexOf(parameter);
            }
        }

        public static MethodBody FromReader(MethodDefinition method, ReadingContext context)
        {
            var reader = context.Reader;
            var body = new MethodBody(method)
            {
                StartOffset = reader.Position,
            };

            var bodyHeader = reader.ReadByte();
            uint codeSize;

            if ((bodyHeader & 0x3) == 0x3)
            {
                reader.Position--;
                var fatBodyHeader = reader.ReadUInt16();
                var headerSize = (fatBodyHeader >> 12) * 4;

                var hasSections = (fatBodyHeader & 0x8) == 0x8;
                body.InitLocals = (fatBodyHeader & 0x10) == 0x10;
                body.MaxStack = reader.ReadUInt16();
                codeSize = reader.ReadUInt32();

                var localVarSig = reader.ReadUInt32();
                if (localVarSig != 0)
                {
                    var header = method.Header;
                    var tableStream = header.GetStream<TableStream>();

                    MetadataMember signature;
                    tableStream.TryResolveMember(new MetadataToken(localVarSig), out signature);
                    body.Signature = signature as StandAloneSignature;
                }

                if (hasSections)
                {
                    body._sectionReadingContext = context.CreateSubContext(reader.Position + codeSize);
                    body._sectionReadingContext.Reader.Align(4);
                }
            }
            else if ((bodyHeader & 0x2) == 0x2)
            {
                codeSize = (uint)(bodyHeader >> 2);
                body.MaxStack = 8;
            }
            else
                throw new ArgumentException("Invalid method body header signature.");

            body._msilReadingContext = context.CreateSubContext(reader.Position, (int)codeSize);
            return body;
        }

        private IList<MsilInstruction> _instructions; 
        private ReadingContext _msilReadingContext;
        private ReadingContext _sectionReadingContext;
        private List<ExceptionHandler> _handlers;
        private ParameterSignature _thisParameter;

        public MethodBody(MethodDefinition method)
        {
            Method = method;
            MaxStack = 8;
        }

        public MethodDefinition Method
        {
            get;
            private set;
        }

        public ParameterSignature ThisParameter
        {
            get
            {
                if (_thisParameter != null)
                    return _thisParameter;
                return _thisParameter = new ParameterSignature(new TypeDefOrRefSignature(Method.DeclaringType));
            }
        }

        public bool InitLocals
        {
            get;
            set;
        }

        public int MaxStack
        {
            get;
            set;
        }

        public bool IsFat
        {
            get
            {
                var localVarSig = Signature == null ? null : Signature.Signature as LocalVariableSignature;
                return MaxStack > 8 ||
                       GetCodeSize() >= 64 ||
                       (localVarSig != null && localVarSig.Variables.Count > 0) ||
                       ExceptionHandlers.Count > 0;
            }
        }

        public StandAloneSignature Signature
        {
            get;
            set;
        }

        public IList<MsilInstruction> Instructions
        {
            get
            {
                if (_instructions != null)
                    return _instructions;
                if (_msilReadingContext == null)
                    return _instructions = new List<MsilInstruction>();
                
                var disassembler = new MsilDisassembler(_msilReadingContext.Reader, this);
                var instructions = disassembler.Disassemble();
                return _instructions = instructions;
            }
        }

        public IList<ExceptionHandler> ExceptionHandlers
        {
            get
            {
                if (_handlers != null)
                    return _handlers;
                var handlers = new List<ExceptionHandler>();

                if (_sectionReadingContext != null)
                    handlers.AddRange(ReadExceptionHandlers());

                return _handlers = handlers;
            }
        }

        private IEnumerable<ExceptionHandler> ReadExceptionHandlers()
        {
            var reader = _sectionReadingContext.Reader;
            byte sectionHeader;
            do
            {
                sectionHeader = reader.ReadByte();
                if ((sectionHeader & 0x01) == 0x01)
                {
                    var isFat = (sectionHeader & 0x40) == 0x40;
                    var handlerCount = 0;
                    if (isFat)
                    {
                        handlerCount = ((reader.ReadByte() |
                                         (reader.ReadByte() << 0x08) |
                                         reader.ReadByte() << 0x10) / 24);
                    }
                    else
                    {
                        handlerCount = reader.ReadByte() / 12;
                        reader.ReadUInt16();
                    }

                    for (int i = 0; i < handlerCount; i++)
                        yield return ExceptionHandler.FromReader(this, reader, isFat);
                }
            } while ((sectionHeader & 0x80) == 0x80);
        }

        public MsilInstruction GetInstructionByOffset(int offset)
        {
            return Instructions.FirstOrDefault(x => x.Offset == offset);
        }

        public void CalculateOffsets()
        {
            var currentOffset = 0;
            foreach (var instruction in Instructions)
            {
                instruction.Offset = currentOffset;
                currentOffset += instruction.Size;
            }
        }

        public uint GetCodeSize()
        {
            var sum = 0;
            foreach (MsilInstruction x in Instructions)
                sum += x.Size;
            return (uint)sum;
        }

        public override uint GetPhysicalLength()
        {
            var size = IsFat ? 12u : 1u;
            size += GetCodeSize();

            foreach (var handler in ExceptionHandlers)
            {
                handler.IsFat = IsFat;
                size += sizeof (uint) + handler.GetPhysicalLength();
            }

            return size;
        }

        public override void Build(BuildingContext context)
        {
            base.Build(context);

            var stringsBuffer = ((NetBuildingContext)context).GetStreamBuffer<UserStringStreamBuffer>();
            foreach (var instruction in Instructions)
            {
                var value = instruction.Operand as string;
                if (value != null)
                {
                    stringsBuffer.GetStringOffset(value);
                }
            }
        }

        public override void UpdateReferences(BuildingContext context)
        {
            base.UpdateReferences(context);
            CalculateOffsets();
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;

            if (IsFat)
            {
                writer.WriteUInt16((ushort)((ExceptionHandlers.Count > 0 ? 0x8 : 0) |
                                            (InitLocals ? 0x10 : 0) | 0x3003));
                writer.WriteUInt16((ushort)MaxStack);
                writer.WriteUInt32(GetCodeSize());
                writer.WriteUInt32(Signature == null ? 0 : Signature.MetadataToken.ToUInt32());
            }
            else
            {
                writer.WriteByte((byte)(0x2 | GetCodeSize() << 2));
            }

            WriteCode(context);

            if (ExceptionHandlers.Count > 0)
                WriteExceptionHandlers(context);
        }

        private void WriteCode(WritingContext context)
        {
            var builder = new MethodBodyOperandBuilder((NetBuildingContext)context.BuildingContext, this);
            var assembler = new MsilAssembler(builder, context.Writer);

            foreach (var instruction in Instructions)
                assembler.Write(instruction);
        }

        private void WriteExceptionHandlers(WritingContext context)
        {
            var useFatFormat = ExceptionHandlers.Any(x => x.IsFatFormatRequired);
            var writer = context.Writer;

            writer.WriteByte((byte)(0x01 | (useFatFormat ? 0x40 : 0)));
            if (useFatFormat)
            {
                var byteLength = ExceptionHandlers.Count * 24;
                writer.WriteByte((byte)(byteLength & 0xFF));
                writer.WriteByte((byte)((byteLength & 0xFF00) >> 0x08));
                writer.WriteByte((byte)((byteLength & 0xFF0000) >> 0x10));
            }
            else
            {
                writer.WriteByte((byte)(ExceptionHandlers.Count * 12));
                writer.WriteUInt16(0);
            }

            foreach (var handler in ExceptionHandlers)
            {
                handler.IsFat = useFatFormat;
                handler.Write(context);
            }
        }

        MetadataMember IOperandResolver.ResolveMember(MetadataToken token)
        {
            return Method.Header.GetStream<TableStream>().ResolveMember(token);
        }

        string IOperandResolver.ResolveString(uint token)
        {
            return Method.Header.GetStream<UserStringStream>().GetStringByOffset(token & 0xFFFFFF);
        }

        VariableSignature IOperandResolver.ResolveVariable(int index)
        {
            var localVarSig = Signature != null ? Signature.Signature as LocalVariableSignature : null;
            if (localVarSig == null)
                return null;
            return localVarSig.Variables[index];
        }

        ParameterSignature IOperandResolver.ResolveParameter(int index)
        {
            if (Method.Signature.Attributes.HasFlag(CallingConventionAttributes.HasThis))
            {
                if (index == 0)
                    return ThisParameter;
                index--;
            }
            return Method.Signature.Parameters[index];
        }
    }

}
