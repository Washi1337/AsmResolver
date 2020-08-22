using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Code.Cil
{
    /// <summary>
    /// Represents a method body of a method defined in a .NET assembly, implemented using the Common Intermediate Language (CIL). 
    /// </summary>
    public class CilMethodBody : MethodBody, ICilOperandResolver
    {
         /// <summary>
        ///     Creates a CIL method body from a dynamic method.
        /// </summary>
        /// <param name="method">The method that owns the method body.</param>
        /// <param name="dynamicMethodObj">The Dynamic Method/Delegate/DynamicResolver.</param>
        /// <param name="operandResolver">
        ///     The object instance to use for resolving operands of an instruction in the
        ///     method body.
        /// </param>
        /// <param name="importer">
        ///     The object instance to use for importing operands of an instruction in the
        ///     method body.
        /// </param>
        /// <returns>The method body.</returns>
        public static CilMethodBody FromDynamicMethod(MethodDefinition method, object dynamicMethodObj,
            ICilOperandResolver operandResolver = null,ReferenceImporter importer = null)
        {
            var result = new CilMethodBody(method);

            operandResolver ??= result;
            
            importer ??= new ReferenceImporter(method.Module);
            
            dynamicMethodObj = DynamicMethodHelper.ResolveDynamicResolver(dynamicMethodObj);

            //Get Runtime Fields
            var code = FieldReader.ReadField<byte[]>(dynamicMethodObj, "m_code");
            var scope = FieldReader.ReadField<object>(dynamicMethodObj, "m_scope");
            var tokenList = FieldReader.ReadField<List<object>>(scope, "m_tokens");
            var localSig = FieldReader.ReadField<byte[]>(dynamicMethodObj, "m_localSignature");
            var ehHeader = FieldReader.ReadField<byte[]>(dynamicMethodObj, "m_exceptionHeader");
            var ehInfos = FieldReader.ReadField<IList<object>>(dynamicMethodObj, "m_exceptions");

            // Read raw instructions.
            var reader = new ByteArrayReader(code);
            var disassembler = new CilDisassembler(reader);
            result.Instructions.AddRange(disassembler.ReadAllInstructions());

            //Local Variables
            result.ReadLocalVariables(method,localSig);

            //Exception Handlers
            result.ReadReflectionExceptionHandlers(ehInfos, ehHeader, importer);
            
            // Resolve all operands.
            foreach (var instruction in result.Instructions)
            {
                instruction.Operand = 
                    result.ResolveOperandReflection(instruction, operandResolver, tokenList, importer) ?? 
                    instruction.Operand;
            }

            return result;
        } 
         
        /// <summary>
        /// Creates a CIL method body from a raw CIL method body. 
        /// </summary>
        /// <param name="method">The method that owns the method body.</param>
        /// <param name="rawBody">The raw method body.</param>
        /// <param name="operandResolver">The object instance to use for resolving operands of an instruction in the
        /// method body.</param>
        /// <returns>The method body.</returns>
        public static CilMethodBody FromRawMethodBody(MethodDefinition method, CilRawMethodBody rawBody,
            ICilOperandResolver operandResolver = null)
        {
            var result = new CilMethodBody(method);

            if (operandResolver is null)
                operandResolver = result;

            // Read raw instructions.
            var reader = new ByteArrayReader(rawBody.Code);
            var disassembler = new CilDisassembler(reader);
            result.Instructions.AddRange(disassembler.ReadAllInstructions());

            // Read out extra metadata.
            if (rawBody is CilRawFatMethodBody fatBody)
            {
                result.MaxStack = fatBody.MaxStack;
                result.InitializeLocals = fatBody.InitLocals;

                ReadLocalVariables(method.Module, result, fatBody);
                ReadExceptionHandlers(fatBody, result);
            }
            else
            {
                result.MaxStack = 8;
                result.InitializeLocals = false;
            }

            // Resolve operands.
            foreach (var instruction in result.Instructions)
                instruction.Operand = ResolveOperand(result, instruction, operandResolver) ?? instruction.Operand;
            
            return result;
        }

        private static object ResolveOperand(CilMethodBody methodBody, CilInstruction instruction,
            ICilOperandResolver resolver)
        {
            switch (instruction.OpCode.OperandType)
            {
                case CilOperandType.InlineBrTarget:
                case CilOperandType.ShortInlineBrTarget:
                    return new CilInstructionLabel(
                        methodBody.Instructions.GetByOffset(((ICilLabel) instruction.Operand).Offset));

                case CilOperandType.InlineField:
                case CilOperandType.InlineMethod:
                case CilOperandType.InlineSig:
                case CilOperandType.InlineTok:
                case CilOperandType.InlineType:
                    return resolver.ResolveMember((MetadataToken) instruction.Operand);

                case CilOperandType.InlineString:
                    return resolver.ResolveString((MetadataToken) instruction.Operand);

                case CilOperandType.InlineSwitch:
                    var result = new List<ICilLabel>();
                    var labels = (IEnumerable<ICilLabel>) instruction.Operand;
                    foreach (var label in labels)
                    {
                        var target = methodBody.Instructions.GetByOffset(label.Offset);
                        result.Add(target == null ? label : new CilInstructionLabel(target));
                    }

                    return result;

                case CilOperandType.InlineVar:
                case CilOperandType.ShortInlineVar:
                    return resolver.ResolveLocalVariable(Convert.ToInt32(instruction.Operand));

                case CilOperandType.InlineArgument:
                case CilOperandType.ShortInlineArgument:
                    return resolver.ResolveParameter(Convert.ToInt32(instruction.Operand));

                case CilOperandType.InlineI:
                case CilOperandType.InlineI8:
                case CilOperandType.InlineNone:
                case CilOperandType.InlineR:
                case CilOperandType.ShortInlineI:
                case CilOperandType.ShortInlineR:
                    return instruction.Operand;

                case CilOperandType.InlinePhi:
                    throw new NotSupportedException();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void ReadLocalVariables(ModuleDefinition module, CilMethodBody result,
            CilRawFatMethodBody fatBody)
        {
            if (fatBody.LocalVarSigToken != MetadataToken.Zero
                && module.TryLookupMember(fatBody.LocalVarSigToken, out var member)
                && member is StandAloneSignature signature
                && signature.Signature is LocalVariablesSignature localVariablesSignature)
            {
                foreach (var type in localVariablesSignature.VariableTypes)
                    result.LocalVariables.Add(new CilLocalVariable(type));
            }
        }

        private static void ReadExceptionHandlers(CilRawFatMethodBody fatBody, CilMethodBody result)
        {
            foreach (var section in fatBody.ExtraSections)
            {
                if (section.IsEHTable)
                {
                    var reader = new ByteArrayReader(section.Data);
                    int size = section.IsFat
                        ? CilExceptionHandler.FatExceptionHandlerSize
                        : CilExceptionHandler.TinyExceptionHandlerSize;

                    while (reader.CanRead(size))
                        result.ExceptionHandlers.Add(CilExceptionHandler.FromReader(result, reader, section.IsFat));
                }
            }
        }

        /// <summary>
        /// Creates a new method body.
        /// </summary>
        /// <param name="owner">The method that owns the method body.</param>
        public CilMethodBody(MethodDefinition owner)
        {
            Owner = owner;
            Instructions = new CilInstructionCollection(this);
        }

        /// <summary>
        /// Gets the method that owns the method body.
        /// </summary>
        public MethodDefinition Owner
        {
            get;
        }

        /// <summary>
        /// Gets a collection of instructions to be executed in the method.
        /// </summary>
        public CilInstructionCollection Instructions
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value indicating the maximum amount of values stored onto the stack.
        /// </summary>
        public int MaxStack
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether a .NET assembly builder should automatically compute and update the
        /// <see cref="MaxStack"/> property according to the contents of the method body. 
        /// </summary>
        public bool ComputeMaxStackOnBuild
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Gets or sets a value indicating whether all local variables should be initialized to zero by the runtime
        /// upon execution of the method body.
        /// </summary>
        public bool InitializeLocals
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the method body is considered fat. That is, it has at least one of the
        /// following properties
        /// <list type="bullet">
        ///    <item><description>The method is larger than 64 bytes.</description></item>
        ///    <item><description>The method defines exception handlers.</description></item>
        ///    <item><description>The method defines local variables.</description></item>
        ///    <item><description>The method needs more than 8 values on the stack.</description></item>
        /// </list>
        /// </summary>
        public bool IsFat
        {
            get
            {
                if (ExceptionHandlers.Count > 0
                    || LocalVariables.Count > 0
                    || MaxStack > 8)
                {
                    return true;
                }

                if (Instructions.Count == 0)
                    return false;

                var last = Instructions[Instructions.Count - 1];
                return last.Offset + last.Size >= 64;
            }
        }

        /// <summary>
        /// Gets a collection of local variables defined in the method body.
        /// </summary>
        public CilLocalVariableCollection LocalVariables
        {
            get;
        } = new CilLocalVariableCollection();

        /// <summary>
        /// Gets a collection of regions protected by exception handlers, finally or faulting clauses defined in the method body.
        /// </summary>
        public IList<CilExceptionHandler> ExceptionHandlers
        {
            get;
        } = new List<CilExceptionHandler>();

        /// <inheritdoc />
        IMetadataMember ICilOperandResolver.ResolveMember(MetadataToken token)
        {
            Owner.Module.TryLookupMember(token, out var member);
            return member;
        }

        /// <inheritdoc />
        string ICilOperandResolver.ResolveString(MetadataToken token)
        {
            Owner.Module.TryLookupString(token, out string value);
            return value;
        }

        /// <inheritdoc />
        CilLocalVariable ICilOperandResolver.ResolveLocalVariable(int index)
        {
            return index >= 0 && index < LocalVariables.Count ? LocalVariables[index] : null;
        }

        /// <inheritdoc />
        Parameter ICilOperandResolver.ResolveParameter(int index)
        {
            var parameters = Owner.Parameters;
            return parameters.ContainsSignatureIndex(index) ? parameters.GetBySignatureIndex(index) : null;
        }

        /// <summary>
        /// Computes the maximum values pushed onto the stack by this method body.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="StackImbalanceException">Occurs when the method body will result in an unbalanced stack.</exception>
        /// <remarks>This method will force the offsets of each instruction to be calculated.</remarks>
        public int ComputeMaxStack()
        {
            if (Instructions.Count == 0)
                return 0;

            Instructions.CalculateOffsets();

            var recordedStackSizes = new int?[Instructions.Count]; 
            var agenda = new Stack<StackState>();

            // Add entrypoints to agenda.
            agenda.Push(new StackState(0, 0));
            foreach (var handler in ExceptionHandlers)
            {
                // Determine stack size at the start of the handler block.
                int stackDelta = handler.HandlerType switch
                {
                    CilExceptionHandlerType.Exception => 1,
                    CilExceptionHandlerType.Filter => 1,
                    CilExceptionHandlerType.Finally => 0,
                    CilExceptionHandlerType.Fault => 0,
                    _ => throw new ArgumentOutOfRangeException(nameof(handler.HandlerType))
                };
                
                agenda.Push(new StackState(Instructions.GetIndexByOffset(handler.TryStart.Offset), 0));
                agenda.Push(new StackState(Instructions.GetIndexByOffset(handler.HandlerStart.Offset), stackDelta));
                
                if (handler.FilterStart != null)
                    agenda.Push(new StackState(Instructions.GetIndexByOffset(handler.FilterStart.Offset), 1));
            }

            while (agenda.Count > 0)
            {
                var currentState = agenda.Pop();
                if (currentState.InstructionIndex >= Instructions.Count)
                {
                    var last = Instructions[Instructions.Count - 1];
                    throw new StackImbalanceException(this, last.Offset + last.Size);
                }

                var instruction = Instructions[currentState.InstructionIndex];

                var recordedStackSize = recordedStackSizes[currentState.InstructionIndex];
                if (recordedStackSize.HasValue)
                {
                    // Check if previously visited state is consistent with current observation.
                    if (recordedStackSize.Value != currentState.StackSize)
                        throw new StackImbalanceException(this, instruction.Offset);
                }
                else
                {
                    // Mark instruction as visited and store current state.
                    recordedStackSizes[currentState.InstructionIndex] = currentState.StackSize;

                    // Compute next stack size.
                    int popCount = instruction.GetStackPopCount(this);
                    int nextStackSize = popCount == -1 ? 0 : currentState.StackSize - popCount;
                    if (nextStackSize < 0)
                        throw new StackImbalanceException(this, instruction.Offset);
                    nextStackSize += instruction.GetStackPushCount();

                    // Add outgoing edges to agenda.
                    switch (instruction.OpCode.FlowControl)
                    {
                        case CilFlowControl.Branch:
                            // Schedule branch target.
                            agenda.Push(new StackState(
                                Instructions.GetIndexByOffset(((ICilLabel) instruction.Operand).Offset),
                                nextStackSize));
                            break;
                        
                        case CilFlowControl.ConditionalBranch when instruction.OpCode.Code == CilCode.Switch:
                            // Schedule all switch targets for processing.
                            foreach (var target in (IEnumerable<ICilLabel>) instruction.Operand)
                            {
                                agenda.Push(new StackState(
                                    Instructions.GetIndexByOffset(target.Offset),
                                    nextStackSize));
                            }

                            // Schedule default case (= fallthrough instruction).
                            agenda.Push(new StackState(
                                currentState.InstructionIndex + 1,
                                nextStackSize));
                            break;
                            
                        case CilFlowControl.ConditionalBranch:
                            // Schedule branch target.
                            agenda.Push(new StackState(
                                Instructions.GetIndexByOffset(((ICilLabel) instruction.Operand).Offset),
                                nextStackSize));

                            // Schedule fallthrough instruction.
                            agenda.Push(new StackState(
                                currentState.InstructionIndex + 1,
                                nextStackSize));
                            break;
                        
                        case CilFlowControl.Call:
                        case CilFlowControl.Break:
                        case CilFlowControl.Meta:
                        case CilFlowControl.Phi:
                        case CilFlowControl.Next:
                            // Schedule fallthrough instruction.
                            agenda.Push(new StackState(
                                currentState.InstructionIndex + 1,
                                nextStackSize));
                            break;
                        
                        case CilFlowControl.Return:
                            // Verify final stack size is correct.
                            if (nextStackSize != 0)
                                throw new StackImbalanceException(this, instruction.Offset);
                            break;
                    }
                }
            }

            return recordedStackSizes.Max(x => x.GetValueOrDefault());
        }

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
    }
}

