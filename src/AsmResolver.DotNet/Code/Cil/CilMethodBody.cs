using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Code.Cil
{
    /// <summary>
    ///     Represents a method body of a method defined in a .NET assembly, implemented using the Common Intermediate Language
    ///     (CIL).
    /// </summary>
    public class CilMethodBody : MethodBody, ICilOperandResolver
    {
        /// <summary>
        ///     Creates a new method body.
        /// </summary>
        /// <param name="owner">The method that owns the method body.</param>
        public CilMethodBody(MethodDefinition owner)
        {
            Owner = owner;
            Instructions = new CilInstructionCollection(this);
        }

        /// <summary>
        ///     Gets the method that owns the method body.
        /// </summary>
        public MethodDefinition Owner { get; }

        /// <summary>
        ///     Gets a collection of instructions to be executed in the method.
        /// </summary>
        public CilInstructionCollection Instructions { get; }

        /// <summary>
        ///     Gets or sets a value indicating the maximum amount of values stored onto the stack.
        /// </summary>
        public int MaxStack { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether a .NET assembly builder should automatically compute and update the
        ///     <see cref="MaxStack" /> property according to the contents of the method body.
        /// </summary>
        public bool ComputeMaxStackOnBuild { get; set; } = true;

        /// <summary>
        ///     Gets or sets a value indicating whether all local variables should be initialized to zero by the runtime
        ///     upon execution of the method body.
        /// </summary>
        public bool InitializeLocals { get; set; }

        /// <summary>
        ///     Gets a value indicating whether the method body is considered fat. That is, it has at least one of the
        ///     following properties
        ///     <list type="bullet">
        ///         <item>
        ///             <description>The method is larger than 64 bytes.</description>
        ///         </item>
        ///         <item>
        ///             <description>The method defines exception handlers.</description>
        ///         </item>
        ///         <item>
        ///             <description>The method defines local variables.</description>
        ///         </item>
        ///         <item>
        ///             <description>The method needs more than 8 values on the stack.</description>
        ///         </item>
        ///     </list>
        /// </summary>
        public bool IsFat
        {
            get
            {
                if (ExceptionHandlers.Count > 0
                    || LocalVariables.Count > 0
                    || MaxStack > 8)
                    return true;

                if (Instructions.Count == 0)
                    return false;

                var last = Instructions[Instructions.Count - 1];
                return last.Offset + last.Size > 64;
            }
        }

        /// <summary>
        ///     Gets a collection of local variables defined in the method body.
        /// </summary>
        public CilLocalVariableCollection LocalVariables { get; } = new CilLocalVariableCollection();

        /// <summary>
        ///     Gets a collection of regions protected by exception handlers, finally or faulting clauses defined in the method
        ///     body.
        /// </summary>
        public IList<CilExceptionHandler> ExceptionHandlers { get; } = new List<CilExceptionHandler>();

        /// <inheritdoc />
        IMetadataMember ICilOperandResolver.ResolveMember(MetadataToken token)
        {
            Owner.Module.TryLookupMember(token, out var member);
            return member;
        }

        /// <inheritdoc />
        string ICilOperandResolver.ResolveString(MetadataToken token)
        {
            Owner.Module.TryLookupString(token, out var value);
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
        ///     Creates a CIL method body from a dynamic method.
        /// </summary>
        /// <param name="method">The method that owns the method body.</param>
        /// <param name="dynamicMethodObj">The dynamic method.</param>
        /// <param name="operandResolver">
        ///     The object instance to use for resolving operands of an instruction in the
        ///     method body.
        /// </param>
        /// <returns>The method body.</returns>
        public static CilMethodBody FromDynamicMethod(MethodDefinition method, object dynamicMethodObj,
            ICilOperandResolver operandResolver = null, ReferenceImporter importer = null)
        {
            var result = new CilMethodBody(method);

            if (operandResolver is null)
                operandResolver = result;

            if (importer is null)
                importer = new ReferenceImporter(method.Module);

            //Get dynamic method
            if (dynamicMethodObj is Delegate deleg)
                dynamicMethodObj = deleg.Method;

            if (dynamicMethodObj is null)
                throw new ArgumentNullException(nameof(dynamicMethodObj));

            if (dynamicMethodObj.GetType().ToString().Contains("RTDynamicMethod"))
                dynamicMethodObj = FieldReader.ReadField<object>(dynamicMethodObj, "m_owner");

            if (dynamicMethodObj.GetType().ToString().Contains("DynamicMethod"))
            {
                var resolver = FieldReader.ReadField<object>(dynamicMethodObj, "m_resolver");
                if (resolver != null)
                    dynamicMethodObj = resolver;
            }

            //Create Resolver if it does not exist.
            if (dynamicMethodObj.GetType().ToString() != "System.Reflection.Emit.DynamicResolver" &&
                dynamicMethodObj.GetType().ToString().Contains("DynamicMethod"))
                dynamicMethodObj = Activator.CreateInstance(
                    typeof(OpCode).Module.GetTypes()
                        .First(t => t.Name == "DynamicResolver"), (BindingFlags) (-1), null,
                    new[]
                    {
                        dynamicMethodObj.GetType().GetRuntimeMethods().First(q => q.Name == "GetILGenerator")
                            .Invoke(dynamicMethodObj, null)
                    }, null);

            //Get Runtime Fields
            var code = FieldReader.ReadField<byte[]>(dynamicMethodObj, "m_code");
            var maxStack = FieldReader.ReadField<int>(dynamicMethodObj, "m_stackSize");
            var scope = FieldReader.ReadField<object>(dynamicMethodObj, "m_scope");
            var tokenList =
                FieldReader.ReadField<List<object>>(scope, "m_tokens");
            var localSig = FieldReader.ReadField<byte[]>(dynamicMethodObj, "m_localSignature");
            var ehHeader = FieldReader.ReadField<byte[]>(dynamicMethodObj, "m_exceptionHeader");
            var ehInfos = FieldReader.ReadField<IList<object>>(dynamicMethodObj, "m_exceptions");

            // Read raw instructions.
            var reader = new ByteArrayReader(code);
            var disassembler = new CilDisassembler(reader);
            result.Instructions.AddRange(disassembler.ReadAllInstructions());

            // TODO: Read Locals & ExceptionHandlers.

            //Local Variables

            var locals =
                CallingConventionSignature.FromReader(method.Module, new ByteArrayReader(localSig)) as
                    LocalVariablesSignature;

            for (var i = 0; i < locals?.VariableTypes.Count; i++)
                result.LocalVariables.Add(new CilLocalVariable(locals.VariableTypes[i]));

            //Exception Handlers

            if (ehHeader != null && ehHeader.Length > 4)
                //Sample needed!
                throw new NotImplementedException("Exception Handlers From ehHeader Not Supported Yet.");
            if (ehInfos != null && ehInfos.Count > 0)
                for (var i = 0; i < ehInfos.Count(); i++)
                {
                    //Get ExceptionHandlerInfo Field Values
                    var endFinally = FieldReader.ReadField<int>(ehInfos[i], "m_endFinally");
                    var endFinallyLabel = endFinally < 0 ? null : result.Instructions.GetByOffset(endFinally)?.CreateLabel() ?? new CilOffsetLabel(endFinally);
                    var endTry = FieldReader.ReadField<int>(ehInfos[i], "m_endAddr");
                    var endTryLabel = result.Instructions.GetByOffset(endTry)?.CreateLabel() ?? new CilOffsetLabel(endTry);
                    var handlerEnd = FieldReader.ReadField<int[]>(ehInfos[i], "m_catchEndAddr")[i];
                    var exceptionType = FieldReader.ReadField<Type[]>(ehInfos[i], "m_catchClass")[i];
                    var handlerStart = FieldReader.ReadField<int[]>(ehInfos[i], "m_catchAddr")[i];
                    var tryStart = FieldReader.ReadField<int>(ehInfos[i], "m_startAddr");
                    var handlerType = (CilExceptionHandlerType) FieldReader.ReadField<int[]>(ehInfos[i], "m_type")[i];

                    //Create the handler
                    var handler = new CilExceptionHandler
                    {
                        HandlerType = handlerType,
                        TryStart = result.Instructions.GetByOffset(tryStart)?.CreateLabel() ?? new CilOffsetLabel(tryStart),
                        TryEnd = handlerType == CilExceptionHandlerType.Finally ? endFinallyLabel : endTryLabel,
                        FilterStart = null,
                        HandlerStart = result.Instructions.GetByOffset(handlerStart)?.CreateLabel() ?? new CilOffsetLabel(handlerStart),
                        HandlerEnd = result.Instructions.GetByOffset(handlerEnd)?.CreateLabel() ?? new CilOffsetLabel(handlerEnd),
                        ExceptionType = importer.ImportType(exceptionType)
                    };

                    result.ExceptionHandlers.Add(handler);
                }

            // Resolve all operands.
            foreach (var instruction in result.Instructions)
                instruction.Operand =
                    ResolveOperandReflection(result, instruction, operandResolver, tokenList, importer) ??
                    instruction.Operand;
            return result;
        }

        private static object ResolveOperandReflection(CilMethodBody methodBody, CilInstruction instruction,
            ICilOperandResolver resolver, List<object> Tokens, ReferenceImporter Importer)
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
                    return ReadToken(((MetadataToken) instruction.Operand).ToUInt32(), Tokens, Importer);
                case CilOperandType.InlineString:
                    return ReadToken(((MetadataToken) instruction.Operand).ToUInt32(), Tokens, Importer);
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

        private static object ReadToken(uint token, IList<object> Tokens, ReferenceImporter importer)
        {
            var rid = token & 0x00FFFFFF;
            switch (token >> 24)
            {
                case 0x02:
                    var type = Tokens[(int) rid];
                    if (type is RuntimeTypeHandle)
                        return importer.ImportType(Type.GetTypeFromHandle((RuntimeTypeHandle) type));
                    return null;
                case 0x04:
                    var field = Tokens[(int) rid];
                    if (field is null)
                        return null;

                    if (field is RuntimeFieldHandle)
                        return importer.ImportField(FieldInfo.GetFieldFromHandle((RuntimeFieldHandle) field));

                    if (field.GetType().ToString() == "System.Reflection.Emit.GenericFieldInfo")
                    {
                        var context = FieldReader.ReadField<RuntimeTypeHandle>(field, "m_context");
                        return importer.ImportField(FieldInfo.GetFieldFromHandle(
                            FieldReader.ReadField<RuntimeFieldHandle>(field, "m_field"), context));
                    }

                    return null;
                case 0x06:
                case 0x0A:
                    var obj = Tokens[(int) rid];
                    if (obj is RuntimeMethodHandle)
                        return importer.ImportMethod(MethodBase.GetMethodFromHandle((RuntimeMethodHandle) obj));
                    if (obj.GetType().ToString() == "System.Reflection.Emit.GenericMethodInfo")
                    {
                        var context =
                            FieldReader.ReadField<RuntimeTypeHandle>(obj, "m_context");
                        var method = MethodBase.GetMethodFromHandle(
                            FieldReader.ReadField<RuntimeMethodHandle>(obj, "m_method"), context);
                        return importer.ImportMethod(method);
                    }

                    if (obj.GetType().ToString() == "System.Reflection.Emit.VarArgMethod")
                    {
                        var method = GetVarArgMethod(obj);
                        if (!method.GetType().ToString().Contains("DynamicMethod"))
                            return importer.ImportMethod((MethodInfo) method);
                        obj = method;
                    }

                    if (obj.GetType().ToString().Contains("DynamicMethod"))
                        throw new Exception("DynamicMethod calls another DynamicMethod");
                    return null;
                case 0x11:
                    return CallingConventionSignature.FromReader(importer.TargetModule,
                        new ByteArrayReader(Tokens[(int) rid] as byte[]));
                case 0x70:
                    return Tokens[(int) rid] as string;
                default:
                    return null;
            }
        }

        private static object GetVarArgMethod(object obj)
        {
            if (FieldReader.ExistsField(obj, "m_dynamicMethod"))
            {
                // .NET 4.0+
                var method =
                    FieldReader.ReadField<MethodInfo>(obj, "m_method");
                var dynMethod =
                    FieldReader.ReadField<object>(obj, "m_dynamicMethod");
                return dynMethod ?? method;
            }

            // .NET 2.0
            // This is either a DynamicMethod or a MethodInfo
            return FieldReader.ReadField<MethodInfo>(obj, "m_method");
        }

        /// <summary>
        ///     Creates a CIL method body from a raw CIL method body.
        /// </summary>
        /// <param name="method">The method that owns the method body.</param>
        /// <param name="rawBody">The raw method body.</param>
        /// <param name="operandResolver">
        ///     The object instance to use for resolving operands of an instruction in the
        ///     method body.
        /// </param>
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
                foreach (var type in localVariablesSignature.VariableTypes)
                    result.LocalVariables.Add(new CilLocalVariable(type));
        }

        private static void ReadExceptionHandlers(CilRawFatMethodBody fatBody, CilMethodBody result)
        {
            foreach (var section in fatBody.ExtraSections)
                if (section.IsEHTable)
                {
                    var reader = new ByteArrayReader(section.Data);
                    var size = section.IsFat
                        ? CilExceptionHandler.FatExceptionHandlerSize
                        : CilExceptionHandler.TinyExceptionHandlerSize;

                    while (reader.CanRead(size))
                        result.ExceptionHandlers.Add(CilExceptionHandler.FromReader(result, reader, section.IsFat));
                }
        }

        /// <summary>
        ///     Computes the maximum values pushed onto the stack by this method body.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="StackImbalanceException">Occurs when the method body will result in an unbalanced stack.</exception>
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
                    handler.HandlerType == CilExceptionHandlerType.Finally ? 0 : 1));
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

                if (visitedInstructions.TryGetValue(currentState.InstructionIndex, out var visitedState))
                {
                    // Check if previously visited state is consistent with current observation.
                    if (visitedState.StackSize != currentState.StackSize)
                        throw new StackImbalanceException(this, instruction.Offset);
                }
                else
                {
                    // Mark instruction as visited and store current state.
                    visitedInstructions[currentState.InstructionIndex] = currentState;

                    // Compute next stack size.
                    var popCount = instruction.GetStackPopCount(this);
                    var nextStackSize = popCount == -1 ? 0 : currentState.StackSize - popCount;
                    if (nextStackSize < 0)
                        throw new StackImbalanceException(this, instruction.Offset);
                    nextStackSize += instruction.GetStackPushCount();

                    // Add outgoing edges to agenda.
                    switch (instruction.OpCode.FlowControl)
                    {
                        case CilFlowControl.Branch:
                            agenda.Push(new StackState(
                                Instructions.GetIndexByOffset(((ICilLabel) instruction.Operand).Offset),
                                nextStackSize));
                            break;
                        case CilFlowControl.ConditionalBranch:
                            switch (instruction.OpCode.OperandType)
                            {
                                case CilOperandType.InlineBrTarget:
                                case CilOperandType.ShortInlineBrTarget:
                                    agenda.Push(new StackState(
                                        Instructions.GetIndexByOffset(((ICilLabel) instruction.Operand).Offset),
                                        nextStackSize));
                                    break;
                                case CilOperandType.InlineSwitch:
                                    foreach (var target in (IEnumerable<ICilLabel>) instruction.Operand)
                                        agenda.Push(new StackState(
                                            Instructions.GetIndexByOffset(target.Offset),
                                            nextStackSize));

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
                                throw new StackImbalanceException(this, instruction.Offset);
                            break;
                    }
                }
            }

            return visitedInstructions.Max(x => x.Value.StackSize);
        }

        /// <summary>
        ///     Provides information about the state of the stack at a particular point of execution in a method.
        /// </summary>
        private struct StackState
        {
            /// <summary>
            ///     The index of the instruction the state is associated to.
            /// </summary>
            public readonly int InstructionIndex;

            /// <summary>
            ///     The number of values currently on the stack.
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