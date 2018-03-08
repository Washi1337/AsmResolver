using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.Net.Builder;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cil
{
    public class CilMethodBody : MethodBody, IOperandResolver
    {
        private struct StackState
        {
            public readonly int InstructionIndex;
            public readonly int StackSize;

            public StackState(int instructionIndex, int stackSize)
            {
                InstructionIndex = instructionIndex;
                StackSize = stackSize;
            }

            public override string ToString()
            {
                return string.Format("InstructionIndex: {0}, StackSize: {1}", InstructionIndex, StackSize);
            }
        }

        public static int GetMethodBodySize(ReadingContext context)
        {   
            var reader = context.Reader;
            long start = reader.Position;
            
            byte bodyHeader = reader.ReadByte();
            if ((bodyHeader & 0x3) == 0x3)
            {
                reader.Position--;
                
                ushort fatBodyHeader = reader.ReadUInt16();
                bool hasSection = (fatBodyHeader & 0x8) == 0x8;
                int headerSize = (fatBodyHeader >> 12) * 4;
                
                reader.Position += sizeof(ushort); // max stack
                uint codeSize = reader.ReadUInt32();
                reader.Position += sizeof(uint); // localvarsig
                reader.Position += codeSize; // cil code.
                
                if (hasSection)
                {
                    reader.Align(4);
                    byte sectionHeader;
                    do
                    {
                        sectionHeader = reader.ReadByte();
                        if ((sectionHeader & 0x01) == 0x01)
                        {
                            bool isFat = (sectionHeader & 0x40) == 0x40;
                            int dataSize = 0;
                            if (isFat)
                            {
                                dataSize = reader.ReadByte() |
                                           (reader.ReadByte() << 0x08) |
                                           reader.ReadByte() << 0x10;
                            }
                            else
                            {
                                dataSize = reader.ReadByte();
                                reader.ReadUInt16();
                            }

                            reader.Position += dataSize - 4;
                        }
                    } while ((sectionHeader & 0x80) == 0x80);
                }
            }
            else if ((bodyHeader & 0x2) == 0x2)
            {
                uint codeSize = (uint)(bodyHeader >> 2);
                reader.Position += codeSize;
            }

            return (int) (reader.Position - start);
        }
        
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
                ushort fatBodyHeader = reader.ReadUInt16();
                int headerSize = (fatBodyHeader >> 12) * 4;

                bool hasSections = (fatBodyHeader & 0x8) == 0x8;
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

            body._cilReadingContext = context.CreateSubContext(reader.Position, (int)codeSize);
            return body;
        }

        private IList<CilInstruction> _instructions; 
        private ReadingContext _cilReadingContext;
        private ReadingContext _sectionReadingContext;
        private List<ExceptionHandler> _handlers;

        public CilMethodBody(MethodDefinition method)
        {
            Method = method;
            MaxStack = 8;
            // TODO: catch if method is not added to type yet.
            ThisParameter = new ParameterSignature(new TypeDefOrRefSignature(Method.DeclaringType));
        }

        public MethodDefinition Method
        {
            get;
            private set;
        }

        public ParameterSignature ThisParameter
        {
            get;
            private set;
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
                if (_cilReadingContext == null)
                    return _instructions = new List<CilInstruction>();

                var cilReader = _cilReadingContext.Reader;
                cilReader.Position = cilReader.StartPosition;
                var disassembler = new CilDisassembler(cilReader, this);
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
            reader.Position = reader.StartPosition;
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

        public int GetInstructionIndex(int offset)
        {
            int left = 0;
            int right = Instructions.Count - 1;

            while (left <= right)
            {
                int m = (left + right) / 2;
                int currentOffset = Instructions[m].Offset;

                if (currentOffset > offset)
                    right = m - 1;
                else if (currentOffset < offset)
                    left = m + 1;
                else
                    return m;
            }

            return -1;
        }

        public CilInstruction GetInstructionByOffset(int offset)
        {
            int index = GetInstructionIndex(offset);
            return index == -1 ? null : Instructions[index];
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
            return (uint) Instructions.Sum(x => x.Size);
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

        public override RvaDataSegment CreateDataSegment(MetadataBuffer buffer)
        {
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryStreamWriter(stream);
                
                if (IsFat)
                {
                    writer.WriteUInt16((ushort)((ExceptionHandlers.Count > 0 ? 0x8 : 0) |
                                                (InitLocals ? 0x10 : 0) | 0x3003));
                    writer.WriteUInt16((ushort)MaxStack);
                    writer.WriteUInt32(GetCodeSize());
                    writer.WriteUInt32(Signature == null ? 0 : buffer.TableStreamBuffer.GetStandaloneSignatureToken(Signature).ToUInt32());
                }
                else
                {
                    writer.WriteByte((byte)(0x2 | GetCodeSize() << 2));
                }
    
                WriteCode(buffer, writer);
    
                if (ExceptionHandlers.Count > 0)
                    WriteExceptionHandlers(buffer, writer);

                return new RvaDataSegment(stream.ToArray());
            }
        }

        public int ComputeMaxStack()
        {
            CalculateOffsets();

            var visitedInstructions = new Dictionary<int, StackState>();
            var agenda = new Stack<StackState>();

            // Add entrypoints to agenda.
            agenda.Push(new StackState(0, 0));
            foreach (var handler in ExceptionHandlers)
            {
                agenda.Push(new StackState(GetInstructionIndex(handler.TryStart.Offset), 0));
                agenda.Push(new StackState(GetInstructionIndex(handler.HandlerStart.Offset),
                    handler.HandlerType == ExceptionHandlerType.Finally ? 0 : 1));
                if (handler.FilterStart!= null)
                    agenda.Push(new StackState(GetInstructionIndex(handler.FilterStart.Offset), 1));
            }

            while (agenda.Count > 0)
            {
                var currentState = agenda.Pop();
                var instruction = Instructions[currentState.InstructionIndex];

                StackState visitedState;
                if (visitedInstructions.TryGetValue(currentState.InstructionIndex, out visitedState))
                {
                    // Check if previously visited state is consistent with current observation.
                    if (visitedState.StackSize != currentState.StackSize)
                        throw new StackInbalanceException(this, instruction.Offset);
                }
                else
                {
                    // Mark instruction as visited and store current state.
                    visitedInstructions[currentState.InstructionIndex] = currentState;

                    // Compute next stack size.
                    int nextStackSize = currentState.StackSize + instruction.GetStackDelta(this);

                    // Add outgoing edges to agenda.
                    switch (instruction.OpCode.FlowControl)
                    {
                        case CilFlowControl.Branch:
                            agenda.Push(new StackState(
                                GetInstructionIndex(((CilInstruction) instruction.Operand).Offset),
                                nextStackSize));
                            break;
                        case CilFlowControl.CondBranch:
                            switch (instruction.OpCode.OperandType)
                            {
                                case CilOperandType.InlineBrTarget:
                                case CilOperandType.ShortInlineBrTarget:
                                    agenda.Push(new StackState(
                                        GetInstructionIndex(((CilInstruction) instruction.Operand).Offset),
                                        nextStackSize));
                                    break;
                                case CilOperandType.InlineSwitch:
                                    foreach (var target in ((IEnumerable<CilInstruction>) instruction.Operand))
                                    {
                                        agenda.Push(new StackState(
                                            GetInstructionIndex(target.Offset),
                                            nextStackSize));
                                    }
                                    break;
                            }
                            agenda.Push(new StackState(
                                currentState.InstructionIndex + 1,
                                nextStackSize));
                            break;
                        case CilFlowControl.Call:
                        case CilFlowControl.Break:
                        case CilFlowControl.Meta:
                        case CilFlowControl.Phi:
                        case CilFlowControl.Next:
                            agenda.Push(new StackState(
                                currentState.InstructionIndex + 1,
                                nextStackSize));
                            break;
                        case CilFlowControl.Return:
                            if (nextStackSize != 0)
                                throw new StackInbalanceException(this, instruction.Offset);
                            break;
                    }
                }
            }

            return visitedInstructions.Max(x => x.Value.StackSize);
        }

        private void WriteCode(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            CalculateOffsets();

            var builder = new DefaultOperandBuilder(this, buffer); 
            var assembler = new CilAssembler(builder, writer);

            foreach (var instruction in Instructions)
                assembler.Write(instruction);
        }
        
        private void WriteExceptionHandlers(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            var useFatFormat = ExceptionHandlers.Any(x => x.IsFatFormatRequired);

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
                handler.Write(buffer, writer);
            }
        }

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
