using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cil
{
    public class CilMethodBody : MethodBody, IOperandResolver
    {
        public static CilMethodBody FromReadingContext(MethodDefinition method, ReadingContext context)
        {
            var reader = context.Reader;
            var body = new CilMethodBody(method)
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
                    IMetadataMember signature;
                    method.Image.TryResolveMember(new MetadataToken(localVarSig), out signature);
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

        private IList<CilInstruction> _instructions; 
        private ReadingContext _msilReadingContext;
        private ReadingContext _sectionReadingContext;
        private List<ExceptionHandler> _handlers;
        private ParameterSignature _thisParameter;

        public CilMethodBody(MethodDefinition method)
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

        public IList<CilInstruction> Instructions
        {
            get
            {
                if (_instructions != null)
                    return _instructions;
                if (_msilReadingContext == null)
                    return _instructions = new List<CilInstruction>();
                
                var disassembler = new CilDisassembler(_msilReadingContext.Reader, this);
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

        public CilInstruction GetInstructionByOffset(int offset)
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

        private void ExpandMacro(CilInstruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case CilCode.Br_S:
                    instruction.OpCode = CilOpCodes.Br;
                    break;
                case CilCode.Leave_S:
                    instruction.OpCode = CilOpCodes.Leave;
                    break;
                case CilCode.Brfalse_S:
                    instruction.OpCode = CilOpCodes.Brfalse;
                    break;
                case CilCode.Brtrue_S:
                    instruction.OpCode = CilOpCodes.Brtrue;
                    break;
                case CilCode.Beq_S:
                    instruction.OpCode = CilOpCodes.Beq;
                    break;
                case CilCode.Bge_S:
                    instruction.OpCode = CilOpCodes.Bge;
                    break;
                case CilCode.Bge_Un_S:
                    instruction.OpCode = CilOpCodes.Bge_Un;
                    break;
                case CilCode.Bgt_S:
                    instruction.OpCode = CilOpCodes.Bgt;
                    break;
                case CilCode.Bgt_Un_S:
                    instruction.OpCode = CilOpCodes.Bgt_Un;
                    break;
                case CilCode.Ble_S:
                    instruction.OpCode = CilOpCodes.Ble;
                    break;
                case CilCode.Ble_Un_S:
                    instruction.OpCode = CilOpCodes.Ble_Un;
                    break;
                case CilCode.Blt_S:
                    instruction.OpCode = CilOpCodes.Blt;
                    break;
                case CilCode.Blt_Un_S:
                    instruction.OpCode = CilOpCodes.Blt_Un;
                    break;
                case CilCode.Bne_Un_S:
                    instruction.OpCode = CilOpCodes.Bne_Un;
                    break;

                case CilCode.Ldloc_S:
                    instruction.OpCode = CilOpCodes.Ldloc;
                    break;

                case CilCode.Ldloca_S:
                    instruction.OpCode = CilOpCodes.Ldloca;
                    break;

                case CilCode.Ldloc_0:
                case CilCode.Ldloc_1:
                case CilCode.Ldloc_2:
                case CilCode.Ldloc_3:
                    instruction.Operand = ((IOperandResolver) this).ResolveVariable(instruction.OpCode.Name[instruction.OpCode.Name.Length - 1] - 48);
                    instruction.OpCode = CilOpCodes.Ldloc;
                    break;

                case CilCode.Stloc_S:
                    instruction.OpCode = CilOpCodes.Stloc;
                    break;

                case CilCode.Stloc_0:
                case CilCode.Stloc_1:
                case CilCode.Stloc_2:
                case CilCode.Stloc_3:
                    instruction.Operand = ((IOperandResolver) this).ResolveVariable(instruction.OpCode.Name[instruction.OpCode.Name.Length - 1] - 48);
                    instruction.OpCode = CilOpCodes.Stloc;
                    break;

                case CilCode.Ldarg_S:
                    instruction.OpCode = CilOpCodes.Ldarg;
                    break;

                case CilCode.Ldarga_S:
                    instruction.OpCode = CilOpCodes.Ldarga;
                    break;

                case CilCode.Ldarg_0:
                case CilCode.Ldarg_1:
                case CilCode.Ldarg_2:
                case CilCode.Ldarg_3:
                    instruction.Operand = ((IOperandResolver) this).ResolveParameter(instruction.OpCode.Name[instruction.OpCode.Name.Length - 1] - 48);
                    instruction.OpCode = CilOpCodes.Ldarg;
                    break;

                case CilCode.Starg_S:
                    instruction.OpCode = CilOpCodes.Starg;
                    break;

                case CilCode.Ldc_I4_0:
                case CilCode.Ldc_I4_1:
                case CilCode.Ldc_I4_2:
                case CilCode.Ldc_I4_3:
                case CilCode.Ldc_I4_4:
                case CilCode.Ldc_I4_5:
                case CilCode.Ldc_I4_6:
                case CilCode.Ldc_I4_7:
                case CilCode.Ldc_I4_8:
                    instruction.Operand = instruction.OpCode.Name[instruction.OpCode.Name.Length - 1] - 48;
                    instruction.OpCode = CilOpCodes.Ldc_I4;
                    break;
                case CilCode.Ldc_I4_S:
                    instruction.OpCode = CilOpCodes.Ldc_I4;
                    break;
                case CilCode.Ldc_I4_M1:
                    instruction.OpCode = CilOpCodes.Ldc_I4;
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

        private void OptimizeMacro(CilInstruction instruction)
        {
            switch (instruction.OpCode.OperandType)
            {
                case CilOperandType.InlineBrTarget:
                    TryOptimizeBranch(instruction);
                    break;
                case CilOperandType.InlineVar:
                    TryOptimizeVariable(instruction);
                    break;
                case CilOperandType.InlineArgument:
                    TryOptimizeArgument(instruction);
                    break;
            }

            if (instruction.OpCode.Code == CilCode.Ldc_I4)
                TryOptimizeLdc(instruction);
        }

        private void TryOptimizeBranch(CilInstruction instruction)
        {
            CilInstruction operand = instruction.Operand as CilInstruction;
            int relativeOperand = operand.Offset - (instruction.Offset + 2);
            if (operand == null || relativeOperand < sbyte.MinValue || relativeOperand > sbyte.MaxValue)
                return;
            switch (instruction.OpCode.Code)
            {
                case CilCode.Br:
                    instruction.OpCode = CilOpCodes.Br_S;
                    break;
                case CilCode.Leave:
                    instruction.OpCode = CilOpCodes.Leave_S;
                    break;
                case CilCode.Brfalse:
                    instruction.OpCode = CilOpCodes.Brfalse_S;
                    break;
                case CilCode.Brtrue:
                    instruction.OpCode = CilOpCodes.Brtrue_S;
                    break;
                case CilCode.Beq:
                    instruction.OpCode = CilOpCodes.Beq_S;
                    break;
                case CilCode.Bge:
                    instruction.OpCode = CilOpCodes.Bge_S;
                    break;
                case CilCode.Bge_Un:
                    instruction.OpCode = CilOpCodes.Bge_Un_S;
                    break;
                case CilCode.Bgt:
                    instruction.OpCode = CilOpCodes.Bgt_S;
                    break;
                case CilCode.Bgt_Un:
                    instruction.OpCode = CilOpCodes.Bgt_Un_S;
                    break;
                case CilCode.Ble:
                    instruction.OpCode = CilOpCodes.Ble_S;
                    break;
                case CilCode.Ble_Un:
                    instruction.OpCode = CilOpCodes.Ble_Un_S;
                    break;
                case CilCode.Blt:
                    instruction.OpCode = CilOpCodes.Blt_S;
                    break;
                case CilCode.Blt_Un:
                    instruction.OpCode = CilOpCodes.Blt_Un_S;
                    break;
                case CilCode.Bne_Un:
                    instruction.OpCode = CilOpCodes.Bne_Un_S;
                    break;
            }
        }

        private void TryOptimizeVariable(CilInstruction instruction)
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
                case CilCode.Ldloc:
                    if (index <= 3)
                    {
                        instruction.OpCode = CilOpCodes.SingleByteOpCodes[CilOpCodes.Ldloc_0.Op2 + index];
                        instruction.Operand = null;
                    }
                    else
                    {
                        instruction.OpCode = CilOpCodes.Ldloc_S;
                    }
                    break;
                case CilCode.Ldloca:
                    instruction.OpCode = CilOpCodes.Ldloca_S;
                    break;
                case CilCode.Stloc:
                    if (index <= 3)
                    {
                        instruction.OpCode = CilOpCodes.SingleByteOpCodes[CilOpCodes.Stloc_0.Op2 + index];
                        instruction.Operand = null;
                    }
                    else
                    {
                        instruction.OpCode = CilOpCodes.Stloc_S;
                    }
                    break;
            }
        }

        private void TryOptimizeArgument(CilInstruction instruction)
        {
            var parameter = instruction.Operand as ParameterSignature;
            if (Method == null || Method.Signature == null || parameter == null)
                return;
            int index = Method.Signature.Parameters.IndexOf(parameter);
            if (index < 0 || index > byte.MaxValue)
                return;

            switch (instruction.OpCode.Code)
            {
                case CilCode.Ldarg:
                    if (index <= 3)
                    {
                        instruction.OpCode = CilOpCodes.SingleByteOpCodes[CilOpCodes.Ldarg_0.Op2 + index];
                        instruction.Operand = null;
                    }
                    else
                    {
                        instruction.OpCode = CilOpCodes.Ldarg_S;
                    }
                    break;
                case CilCode.Ldarga:
                    instruction.OpCode = CilOpCodes.Ldarga_S;
                    break;
            }
        }

        private void TryOptimizeLdc(CilInstruction instruction)
        {
            int value = (int) instruction.Operand;
            if (value >= -1 && value <= 8)
            {
                instruction.OpCode = CilOpCodes.SingleByteOpCodes[CilOpCodes.Ldc_I4_0.Op2 + value];
                instruction.Operand = null;
            }
            else if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
            {
                instruction.OpCode = CilOpCodes.Ldc_I4_S;
                instruction.Operand = Convert.ToSByte(value);
            }
        }

        public uint GetCodeSize()
        {
            var sum = 0;
            foreach (CilInstruction x in Instructions)
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

        public override void Write(WritingContext context)
        {
            // TODO
            throw new NotImplementedException();
        }

//        public override void Build(BuildingContext context)
//        {
//            base.Build(context);
//
//            var stringsBuffer = ((NetBuildingContext)context).GetStreamBuffer<UserStringStreamBuffer>();
//            foreach (var instruction in Instructions)
//            {
//                var value = instruction.Operand as string;
//                if (value != null)
//                {
//                    stringsBuffer.GetStringOffset(value);
//                }
//            }
//        }
//
//        public override void UpdateReferences(BuildingContext context)
//        {
//            base.UpdateReferences(context);
//            CalculateOffsets();
//        }
//
//        public override void Write(WritingContext context)
//        {
//            var writer = context.Writer;
//
//            if (IsFat)
//            {
//                writer.WriteUInt16((ushort)((ExceptionHandlers.Count > 0 ? 0x8 : 0) |
//                                            (InitLocals ? 0x10 : 0) | 0x3003));
//                writer.WriteUInt16((ushort)MaxStack);
//                writer.WriteUInt32(GetCodeSize());
//                writer.WriteUInt32(Signature == null ? 0 : Signature.MetadataToken.ToUInt32());
//            }
//            else
//            {
//                writer.WriteByte((byte)(0x2 | GetCodeSize() << 2));
//            }
//
//            WriteCode(context);
//
//            if (ExceptionHandlers.Count > 0)
//                WriteExceptionHandlers(context);
//        }
//
//        private void WriteCode(WritingContext context)
//        {
//            var builder = new MethodBodyOperandBuilder((NetBuildingContext)context.BuildingContext, this);
//            var assembler = new CilAssembler(builder, context.Writer);
//
//            foreach (var instruction in Instructions)
//                assembler.Write(instruction);
//        }
//
//        private void WriteExceptionHandlers(WritingContext context)
//        {
//            var useFatFormat = ExceptionHandlers.Any(x => x.IsFatFormatRequired);
//            var writer = context.Writer;
//
//            writer.Align(4);
//
//            writer.WriteByte((byte)(0x01 | (useFatFormat ? 0x40 : 0)));
//            if (useFatFormat)
//            {
//                var byteLength = ExceptionHandlers.Count * 24;
//                writer.WriteByte((byte)(byteLength & 0xFF));
//                writer.WriteByte((byte)((byteLength & 0xFF00) >> 0x08));
//                writer.WriteByte((byte)((byteLength & 0xFF0000) >> 0x10));
//            }
//            else
//            {
//                writer.WriteByte((byte)(ExceptionHandlers.Count * 12));
//                writer.WriteUInt16(0);
//            }
//
//            foreach (var handler in ExceptionHandlers)
//            {
//                handler.IsFat = useFatFormat;
//                handler.Write(context);
//            }
//        }

        IMetadataMember IOperandResolver.ResolveMember(MetadataToken token)
        {
            IMetadataMember result;
            Method.Image.TryResolveMember(token, out result);
            return result;
        }

        string IOperandResolver.ResolveString(uint token)
        {
            return Method.Image.Header.GetStream<UserStringStream>().GetStringByOffset(token & 0xFFFFFF);
        }

        VariableSignature IOperandResolver.ResolveVariable(int index)
        {
            var localVarSig = Signature != null ? Signature.Signature as LocalVariableSignature : null;
            if (localVarSig == null || index < 0 || index >= localVarSig.Variables.Count)
                return null;
            return localVarSig.Variables[index];
        }

        ParameterSignature IOperandResolver.ResolveParameter(int index)
        {
            if (Method.Signature.HasThis)
            {
                if (index == 0)
                    return ThisParameter;
                index--;
            }
            return Method.Signature.Parameters[index];
        }
    }

}
