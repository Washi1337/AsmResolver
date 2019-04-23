using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cil
{
    /// <summary>
    /// Provides a representation of a CIL method body found in a .NET image. 
    /// </summary>
    public class CilMethodBody : MethodBody, IOperandResolver
    {
        /// <summary>
        /// Provides information about the state of the stack at a particular point of execution in a method.  
        /// </summary>
        private struct StackState
        {
            /// <summary>
            /// The index of the instruction the state is associated to.
            /// </summary>
            public readonly int InstructionIndex;
            
            /// <summary>
            /// The number of values currently on the stack.
            /// </summary>
            public readonly int StackSize;

            public StackState(int instructionIndex, int stackSize)
            {
                InstructionIndex = instructionIndex;
                StackSize = stackSize;
            }

            #if DEBUG
            public override string ToString()
            {
                return $"InstructionIndex: {InstructionIndex}, StackSize: {StackSize}";
            }
            #endif
        }

        /// <summary>
        /// Interprets a raw method body to a CILMethodBody, disassembling all instructions, obtaining the local variables
        /// and reading the extra sections for exception handlers.
        /// </summary>
        /// <param name="method">The parent method the method body is associated to.</param>
        /// <param name="rawMethodBody">The raw method body to convert.</param>
        /// <returns>The converted method body.</returns>
        public static CilMethodBody FromRawMethodBody(MethodDefinition method, CilRawMethodBody rawMethodBody)
        {
            var body = new CilMethodBody(method);

            var fatBody = rawMethodBody as CilRawFatMethodBody;
            if (fatBody != null)
            {
                // Read fat method body header fields.
                body.MaxStack = fatBody.MaxStack;
                body.InitLocals = fatBody.InitLocals;

                if (method.Image.TryResolveMember(new MetadataToken(fatBody.LocalVarSigToken), out var signature))
                    body.Signature = signature as StandAloneSignature;
            }
            else
            {
                // Set values for tiny method bodies.
                body.MaxStack = 8;
                body.InitLocals = false;
            }

            // Read code.
            var codeReader = new MemoryStreamReader(rawMethodBody.Code);
            var disassembler = new CilDisassembler(codeReader, body);
            foreach (var instruction in disassembler.Disassemble())
                body.Instructions.Add(instruction);

            if (fatBody != null)
            {
                // Read exception handlers.
                foreach (var section in fatBody.ExtraSections)
                {
                    var sectionReader = new MemoryStreamReader(section.Data);
                    while (sectionReader.CanRead(section.IsFat
                        ? ExceptionHandler.FatExceptionHandlerSize
                        : ExceptionHandler.SmallExceptionHandlerSize))
                    {
                        body.ExceptionHandlers.Add(ExceptionHandler.FromReader(body, sectionReader, section.IsFat));
                    }
                }
            }
            
            return body;
        }

        private readonly LazyValue<ParameterSignature> _thisParameter;
        
        public CilMethodBody(MethodDefinition method)
        {
            Method = method;
            MaxStack = 8;
            Instructions = new CilInstructionCollection(this);
            ExceptionHandlers = new List<ExceptionHandler>();
            ComputeMaxStackOnBuild = true;

            _thisParameter = new LazyValue<ParameterSignature>(() => method.DeclaringType != null
                    ? new ParameterSignature(method.DeclaringType.ToTypeSignature()) 
                    : null);
        }

        /// <summary>
        /// Gets the method the method body is associated to.
        /// </summary>
        public MethodDefinition Method
        {
            get;
        }

        /// <summary>
        /// Gets the this parameter, if present.
        /// </summary>
        public ParameterSignature ThisParameter => _thisParameter.Value;

        /// <summary>
        /// Gets a value indicating whether variables are initialized by the runtime to their default values.
        /// </summary>
        public bool InitLocals
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="MaxStack"/> field should be recalculated upon rebuilding
        /// the method body. Set to false if a custom value is provided.
        /// </summary>
        public bool ComputeMaxStackOnBuild
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating the maximum amount of values that can be pushed onto the stack.
        /// </summary>
        public int MaxStack
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the method body is using the fat format.
        /// </summary>
        public bool IsFat => MaxStack > 8 
                             || GetCodeSize() >= 64
                             || (Signature?.Signature is LocalVariableSignature localVarSig && localVarSig.Variables.Count > 0) 
                             || ExceptionHandlers.Count > 0;

        /// <summary>
        /// Gets the standalone signature that was referenced by the method header. This signature typically contains
        /// a <see cref="LocalVariableSignature"/> which holds the variables.
        /// </summary>
        public StandAloneSignature Signature
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the collection of instructions present in the method body.
        /// </summary>
        public CilInstructionCollection Instructions
        {
            get;
        }

        /// <summary>
        /// Gets the collection of exception handlers defined in the method body.
        /// </summary>
        public IList<ExceptionHandler> ExceptionHandlers
        {
            get;
        }

        /// <inheritdoc />
        public override uint GetCodeSize()
        {
            return (uint) Instructions.Size;
        }

        /// <inheritdoc />
        public override FileSegment CreateRawMethodBody(MetadataBuffer buffer)
        {
            using (var codeStream = new MemoryStream())
            {
                if (ComputeMaxStackOnBuild)
                    MaxStack = ComputeMaxStack();
                else
                    Instructions.CalculateOffsets();
                
                WriteCode(buffer, new BinaryStreamWriter(codeStream));

                if (!IsFat)
                {
                    return new CilRawSmallMethodBody
                    {
                        Code = codeStream.ToArray()
                    };
                }

                Signature?.Signature?.Prepare(buffer);

                var fatBody = new CilRawFatMethodBody
                {
                    HasSections = ExceptionHandlers.Count > 0,
                    InitLocals = InitLocals,
                    LocalVarSigToken = buffer.TableStreamBuffer.GetStandaloneSignatureToken(Signature).ToUInt32(),
                    MaxStack = (ushort) MaxStack,
                    Code = codeStream.ToArray()
                };

                if (ExceptionHandlers.Count > 0)
                {
                    using (var sectionStream = new MemoryStream())
                    {
                        var sectionWriter = new BinaryStreamWriter(sectionStream);
                        bool useFatFormat = ExceptionHandlers.Any(x => x.IsFatFormatRequired);
                        foreach (var handler in ExceptionHandlers)
                        {
                            handler.IsFat = useFatFormat;
                            handler.Write(buffer, sectionWriter);
                        }

                        fatBody.ExtraSections.Add(new CilExtraSection
                        {
                            IsExceptionHandler = true,
                            IsFat = useFatFormat,
                            HasMoreSections = false,
                            Data = sectionStream.ToArray()
                        });
                    }
                }

                return fatBody;
            }
        }

        private void WriteCode(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            var builder = new DefaultOperandBuilder(this, buffer); 
            var assembler = new CilAssembler(builder, writer);

            foreach (var instruction in Instructions)
                assembler.Write(instruction);
        }

        /// <summary>
        /// Computes the maximum values pushed onto the stack by this method body.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="StackInbalanceException">Occurs when the method body will result in an unbalanced stack.</exception>
        /// <remarks>This method will force the offsets of each instruction to be calculated.</remarks>
        public int ComputeMaxStack()
        {
            if (Instructions.Count == 0)
                return 0;
            
            Instructions.CalculateOffsets();

            var visitedInstructions = new Dictionary<int, StackState>();
            var agenda = new Stack<StackState>();

            // Add entrypoints to agenda.
            agenda.Push(new StackState(0, 0));
            foreach (var handler in ExceptionHandlers)
            {
                agenda.Push(new StackState(Instructions.GetIndexByOffset(handler.TryStart.Offset), 0));
                agenda.Push(new StackState(Instructions.GetIndexByOffset(handler.HandlerStart.Offset),
                    handler.HandlerType == ExceptionHandlerType.Finally ? 0 : 1));
                if (handler.FilterStart != null)
                    agenda.Push(new StackState(Instructions.GetIndexByOffset(handler.FilterStart.Offset), 1));
            }

            while (agenda.Count > 0)
            {
                var currentState = agenda.Pop();
                if (currentState.InstructionIndex >= Instructions.Count)
                {
                    var last = Instructions[Instructions.Count - 1];
                    throw new StackInbalanceException(this, last.Offset + last.Size);
                }

                var instruction = Instructions[currentState.InstructionIndex];

                if (visitedInstructions.TryGetValue(currentState.InstructionIndex, out var visitedState))
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
                    int nextStackSize = currentState.StackSize - instruction.GetStackPopCount(this);
                    if (nextStackSize < 0)
                        throw new StackInbalanceException(this, instruction.Offset);
                    nextStackSize += instruction.GetStackPushCount(this);

                    // Add outgoing edges to agenda.
                    switch (instruction.OpCode.FlowControl)
                    {
                        case CilFlowControl.Branch:
                            agenda.Push(new StackState(
                                Instructions.GetIndexByOffset(((CilInstruction) instruction.Operand).Offset),
                                nextStackSize));
                            break;
                        case CilFlowControl.CondBranch:
                            switch (instruction.OpCode.OperandType)
                            {
                                case CilOperandType.InlineBrTarget:
                                case CilOperandType.ShortInlineBrTarget:
                                    agenda.Push(new StackState(
                                        Instructions.GetIndexByOffset(((CilInstruction) instruction.Operand).Offset),
                                        nextStackSize));
                                    break;
                                case CilOperandType.InlineSwitch:
                                    foreach (var target in ((IEnumerable<CilInstruction>) instruction.Operand))
                                    {
                                        agenda.Push(new StackState(
                                            Instructions.GetIndexByOffset(target.Offset),
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

        IMetadataMember IOperandResolver.ResolveMember(MetadataToken token)
        {
            Method.Image.TryResolveMember(token, out var result);
            return result;
        }

        string IOperandResolver.ResolveString(uint token)
        {
            return Method.Image.Header.GetStream<UserStringStream>().GetStringByOffset(token & 0xFFFFFF);
        }

        VariableSignature IOperandResolver.ResolveVariable(int index)
        {
            return !(Signature?.Signature is LocalVariableSignature localVarSig)
                   || index < 0
                   || index >= localVarSig.Variables.Count
                ? null
                : localVarSig.Variables[index];
        }

        ParameterSignature IOperandResolver.ResolveParameter(int index)
        {
            if (Method.Signature.HasThis)
            {
                if (index == 0)
                    return ThisParameter;
                index--;
            }

            return index >= 0 && index < Method.Signature.Parameters.Count 
                ? Method.Signature.Parameters[index] 
                : null;
        }
    }
}
