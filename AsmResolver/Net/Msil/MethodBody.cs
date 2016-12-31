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
using AsmResolver.X86;

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

        public static MethodBody FromReadingContext(MethodDefinition method, ReadingContext context)
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

        public void ExpandMacros()
        {
            foreach (var instruction in Instructions)
                ExpandMacro(instruction);
            CalculateOffsets();
        }

        private void ExpandMacro(MsilInstruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case MsilCode.Br_S:
                    instruction.OpCode = MsilOpCodes.Br;
                    break;
                case MsilCode.Leave_S:
                    instruction.OpCode = MsilOpCodes.Leave;
                    break;
                case MsilCode.Brfalse_S:
                    instruction.OpCode = MsilOpCodes.Brfalse;
                    break;
                case MsilCode.Brtrue_S:
                    instruction.OpCode = MsilOpCodes.Brtrue;
                    break;
                case MsilCode.Beq_S:
                    instruction.OpCode = MsilOpCodes.Beq;
                    break;
                case MsilCode.Bge_S:
                    instruction.OpCode = MsilOpCodes.Bge;
                    break;
                case MsilCode.Bge_Un_S:
                    instruction.OpCode = MsilOpCodes.Bge_Un;
                    break;
                case MsilCode.Bgt_S:
                    instruction.OpCode = MsilOpCodes.Bgt;
                    break;
                case MsilCode.Bgt_Un_S:
                    instruction.OpCode = MsilOpCodes.Bgt_Un;
                    break;
                case MsilCode.Ble_S:
                    instruction.OpCode = MsilOpCodes.Ble;
                    break;
                case MsilCode.Ble_Un_S:
                    instruction.OpCode = MsilOpCodes.Ble_Un;
                    break;
                case MsilCode.Blt_S:
                    instruction.OpCode = MsilOpCodes.Blt;
                    break;
                case MsilCode.Blt_Un_S:
                    instruction.OpCode = MsilOpCodes.Blt_Un;
                    break;
                case MsilCode.Bne_Un_S:
                    instruction.OpCode = MsilOpCodes.Bne_Un;
                    break;

                case MsilCode.Ldloc_S:
                    instruction.OpCode = MsilOpCodes.Ldloc;
                    break;

                case MsilCode.Ldloca_S:
                    instruction.OpCode = MsilOpCodes.Ldloca;
                    break;

                case MsilCode.Ldloc_0:
                case MsilCode.Ldloc_1:
                case MsilCode.Ldloc_2:
                case MsilCode.Ldloc_3:
                    instruction.Operand = ((IOperandResolver) this).ResolveVariable(instruction.OpCode.Name[instruction.OpCode.Name.Length - 1] - 48);
                    instruction.OpCode = MsilOpCodes.Ldloc;
                    break;

                case MsilCode.Stloc_S:
                    instruction.OpCode = MsilOpCodes.Stloc;
                    break;

                case MsilCode.Stloc_0:
                case MsilCode.Stloc_1:
                case MsilCode.Stloc_2:
                case MsilCode.Stloc_3:
                    instruction.Operand = ((IOperandResolver) this).ResolveVariable(instruction.OpCode.Name[instruction.OpCode.Name.Length - 1] - 48);
                    instruction.OpCode = MsilOpCodes.Stloc;
                    break;

                case MsilCode.Ldarg_S:
                    instruction.OpCode = MsilOpCodes.Ldarg;
                    break;

                case MsilCode.Ldarga_S:
                    instruction.OpCode = MsilOpCodes.Ldarga;
                    break;

                case MsilCode.Ldarg_0:
                case MsilCode.Ldarg_1:
                case MsilCode.Ldarg_2:
                case MsilCode.Ldarg_3:
                    instruction.Operand = ((IOperandResolver) this).ResolveParameter(instruction.OpCode.Name[instruction.OpCode.Name.Length - 1] - 48);
                    instruction.OpCode = MsilOpCodes.Ldarg;
                    break;

                case MsilCode.Starg_S:
                    instruction.OpCode = MsilOpCodes.Starg;
                    break;

                case MsilCode.Ldc_I4_0:
                case MsilCode.Ldc_I4_1:
                case MsilCode.Ldc_I4_2:
                case MsilCode.Ldc_I4_3:
                case MsilCode.Ldc_I4_4:
                case MsilCode.Ldc_I4_5:
                case MsilCode.Ldc_I4_6:
                case MsilCode.Ldc_I4_7:
                case MsilCode.Ldc_I4_8:
                    instruction.Operand = instruction.OpCode.Name[instruction.OpCode.Name.Length - 1] - 48;
                    instruction.OpCode = MsilOpCodes.Ldc_I4;
                    break;
                case MsilCode.Ldc_I4_S:
                    instruction.OpCode = MsilOpCodes.Ldc_I4;
                    break;
                case MsilCode.Ldc_I4_M1:
                    instruction.OpCode = MsilOpCodes.Ldc_I4;
                    instruction.Operand = -1;
                    break;
            }
        }

        public void OptimizeMacros()
        {
            CalculateOffsets();
            foreach (var instruction in Instructions)
                OptimizeMacro(instruction);
            CalculateOffsets();
        }

        private void OptimizeMacro(MsilInstruction instruction)
        {
            switch (instruction.OpCode.OperandType)
            {
                case MsilOperandType.InlineBrTarget:
                    TryOptimizeBranch(instruction);
                    break;
                case MsilOperandType.InlineVar:
                    TryOptimizeVariable(instruction);
                    break;
                case MsilOperandType.InlineArgument:
                    TryOptimizeArgument(instruction);
                    break;
            }

            if (instruction.OpCode.Code == MsilCode.Ldc_I4)
                TryOptimizeLdc(instruction);
        }

        private void TryOptimizeBranch(MsilInstruction instruction)
        {
            MsilInstruction operand = instruction.Operand as MsilInstruction;
            int relativeOperand = operand.Offset - (instruction.Offset + 2);
            if (operand == null || relativeOperand < sbyte.MinValue || relativeOperand > sbyte.MaxValue)
                return;
            switch (instruction.OpCode.Code)
            {
                case MsilCode.Br:
                    instruction.OpCode = MsilOpCodes.Br_S;
                    break;
                case MsilCode.Leave:
                    instruction.OpCode = MsilOpCodes.Leave_S;
                    break;
                case MsilCode.Brfalse:
                    instruction.OpCode = MsilOpCodes.Brfalse_S;
                    break;
                case MsilCode.Brtrue:
                    instruction.OpCode = MsilOpCodes.Brtrue_S;
                    break;
                case MsilCode.Beq:
                    instruction.OpCode = MsilOpCodes.Beq_S;
                    break;
                case MsilCode.Bge:
                    instruction.OpCode = MsilOpCodes.Bge_S;
                    break;
                case MsilCode.Bge_Un:
                    instruction.OpCode = MsilOpCodes.Bge_Un_S;
                    break;
                case MsilCode.Bgt:
                    instruction.OpCode = MsilOpCodes.Bgt_S;
                    break;
                case MsilCode.Bgt_Un:
                    instruction.OpCode = MsilOpCodes.Bgt_Un_S;
                    break;
                case MsilCode.Ble:
                    instruction.OpCode = MsilOpCodes.Ble_S;
                    break;
                case MsilCode.Ble_Un:
                    instruction.OpCode = MsilOpCodes.Ble_Un_S;
                    break;
                case MsilCode.Blt:
                    instruction.OpCode = MsilOpCodes.Blt_S;
                    break;
                case MsilCode.Blt_Un:
                    instruction.OpCode = MsilOpCodes.Blt_Un_S;
                    break;
                case MsilCode.Bne_Un:
                    instruction.OpCode = MsilOpCodes.Bne_Un_S;
                    break;
            }
        }

        private void TryOptimizeVariable(MsilInstruction instruction)
        {
            var variable = instruction.Operand as VariableSignature;
            var localVarSig = Signature != null ? Signature.Signature as LocalVariableSignature : null;
            if (localVarSig == null || variable == null)
                return;
            int index = localVarSig.Variables.IndexOf(variable);
            if (index < 0 || index > byte.MaxValue)
                return;

            switch (instruction.OpCode.Code)
            {
                case MsilCode.Ldloc:
                    if (index <= 3)
                    {
                        instruction.OpCode = MsilOpCodes.SingleByteOpCodes[MsilOpCodes.Ldloc_0.Op2 + index];
                        instruction.Operand = null;
                    }
                    else
                    {
                        instruction.OpCode = MsilOpCodes.Ldloc_S;
                    }
                    break;
                case MsilCode.Ldloca:
                    instruction.OpCode = MsilOpCodes.Ldloca_S;
                    break;
                case MsilCode.Stloc:
                    if (index <= 3)
                    {
                        instruction.OpCode = MsilOpCodes.SingleByteOpCodes[MsilOpCodes.Stloc_0.Op2 + index];
                        instruction.Operand = null;
                    }
                    else
                    {
                        instruction.OpCode = MsilOpCodes.Stloc_S;
                    }
                    break;
            }
        }

        private void TryOptimizeArgument(MsilInstruction instruction)
        {
            var parameter = instruction.Operand as ParameterSignature;
            if (Method == null || Method.Signature == null || parameter == null)
                return;
            int index = Method.Signature.Parameters.IndexOf(parameter);
            if (index < 0 || index > byte.MaxValue)
                return;

            switch (instruction.OpCode.Code)
            {
                case MsilCode.Ldarg:
                    if (index <= 3)
                    {
                        instruction.OpCode = MsilOpCodes.SingleByteOpCodes[MsilOpCodes.Ldarg_0.Op2 + index];
                        instruction.Operand = null;
                    }
                    else
                    {
                        instruction.OpCode = MsilOpCodes.Ldarg_S;
                    }
                    break;
                case MsilCode.Ldarga:
                    instruction.OpCode = MsilOpCodes.Ldarga_S;
                    break;
            }
        }

        private void TryOptimizeLdc(MsilInstruction instruction)
        {
            int value = (int) instruction.Operand;
            if (value >= -1 && value <= 8)
            {
                instruction.OpCode = MsilOpCodes.SingleByteOpCodes[MsilOpCodes.Ldc_I4_0.Op2 + value];
                instruction.Operand = null;
            }
            else if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
            {
                instruction.OpCode = MsilOpCodes.Ldc_I4_S;
                instruction.Operand = Convert.ToSByte(value);
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

            writer.Align(4);

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
            MetadataMember result;
            Method.Header.GetStream<TableStream>().TryResolveMember(token, out result);
            return result;
        }

        string IOperandResolver.ResolveString(uint token)
        {
            return Method.Header.GetStream<UserStringStream>().GetStringByOffset(token & 0xFFFFFF);
        }

        VariableSignature IOperandResolver.ResolveVariable(int index)
        {
            var localVarSig = Signature != null ? Signature.Signature as LocalVariableSignature : null;
            if (localVarSig == null && index >= 0 && index < localVarSig.Variables.Count)
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
