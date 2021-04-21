using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Code.Cil
{
    /// <summary>
    /// Represents a method body of a method defined in a .NET assembly, implemented using the Common Intermediate Language (CIL).
    /// </summary>
    public class CilMethodBody : MethodBody
    {
        /// <summary>
        /// Creates a new method body.
        /// </summary>
        /// <param name="owner">The method that owns the method body.</param>
        public CilMethodBody(MethodDefinition owner)
            : base(owner)
        {
            Instructions = new CilInstructionCollection(this);
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
        } = new();

        /// <summary>
        /// Gets a collection of regions protected by exception handlers, finally or faulting clauses defined in the method body.
        /// </summary>
        public IList<CilExceptionHandler> ExceptionHandlers
        {
            get;
        } = new List<CilExceptionHandler>();

        /// <summary>
        /// Gets or sets flags that alter the behaviour of the method body serializer for this specific method body.
        /// </summary>
        public CilMethodBodyBuildFlags BuildFlags
        {
            get;
            set;
        } = CilMethodBodyBuildFlags.FullValidation;

        /// <summary>
        /// Gets or sets a value indicating whether a .NET assembly builder should automatically compute and update the
        /// <see cref="MaxStack"/> property according to the contents of the method body.
        /// </summary>
        public bool ComputeMaxStackOnBuild
        {
            get => (BuildFlags & CilMethodBodyBuildFlags.ComputeMaxStack) == CilMethodBodyBuildFlags.ComputeMaxStack;
            set => BuildFlags = (BuildFlags & ~CilMethodBodyBuildFlags.ComputeMaxStack)
                                | (value ? CilMethodBodyBuildFlags.ComputeMaxStack : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether a .NET assembly builder should verify branch instructions and
        /// exception handler labels in this method body for validity.
        /// </summary>
        /// <remarks>
        /// The value of this property will be ignored if <see cref="ComputeMaxStackOnBuild"/> is set to <c>true</c>.
        /// </remarks>
        public bool VerifyLabelsOnBuild
        {
            get => (BuildFlags & CilMethodBodyBuildFlags.VerifyLabels) == CilMethodBodyBuildFlags.VerifyLabels;
            set => BuildFlags = (BuildFlags & ~CilMethodBodyBuildFlags.VerifyLabels)
                                | (value ? CilMethodBodyBuildFlags.VerifyLabels : 0);
        }

        /// <summary>
        ///     Creates a CIL method body from a dynamic method.
        /// </summary>
        /// <param name="method">The method that owns the method body.</param>
        /// <param name="dynamicMethodObj">The Dynamic Method/Delegate/DynamicResolver.</param>
        /// <returns>The method body.</returns>
        public static CilMethodBody FromDynamicMethod(MethodDefinition method, object dynamicMethodObj)
        {
            if (!(method.Module is SerializedModuleDefinition module))
                throw new ArgumentException("Method body should reference a serialized module.");

            var result = new CilMethodBody(method);
            dynamicMethodObj = DynamicMethodHelper.ResolveDynamicResolver(dynamicMethodObj);

            //Get Runtime Fields
            byte[] code = FieldReader.ReadField<byte[]>(dynamicMethodObj, "m_code");
            var scope = FieldReader.ReadField<object>(dynamicMethodObj, "m_scope");
            var tokenList = FieldReader.ReadField<List<object>>(scope, "m_tokens");
            byte[] localSig = FieldReader.ReadField<byte[]>(dynamicMethodObj, "m_localSignature");
            byte[] ehHeader = FieldReader.ReadField<byte[]>(dynamicMethodObj, "m_exceptionHeader");
            var ehInfos = FieldReader.ReadField<IList<object>>(dynamicMethodObj, "m_exceptions");

            //Local Variables
            DynamicMethodHelper.ReadLocalVariables(result, method, localSig);

            // Read raw instructions.
            var reader = ByteArrayReaderFactory.CreateReader(code);
            var disassembler = new CilDisassembler(reader, new DynamicCilOperandResolver(module, result, tokenList));
            result.Instructions.AddRange(disassembler.ReadInstructions());

            //Exception Handlers
            DynamicMethodHelper.ReadReflectionExceptionHandlers(result, ehInfos, ehHeader, new ReferenceImporter(module));

            return result;
        }

        /// <summary>
        /// Creates a CIL method body from a raw CIL method body.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="method">The method that owns the method body.</param>
        /// <param name="rawBody">The raw method body.</param>
        /// <param name="operandResolver">The object instance to use for resolving operands of an instruction in the
        ///     method body.</param>
        /// <returns>The method body.</returns>
        public static CilMethodBody FromRawMethodBody(
            ModuleReaderContext context,
            MethodDefinition method,
            CilRawMethodBody rawBody,
            ICilOperandResolver operandResolver = null)
        {
            var result = new CilMethodBody(method);

            operandResolver ??= new PhysicalCilOperandResolver(context.ParentModule, result);

            // Interpret body header.
            var fatBody = rawBody as CilRawFatMethodBody;
            if (fatBody is not null)
            {
                result.MaxStack = fatBody.MaxStack;
                result.InitializeLocals = fatBody.InitLocals;
                ReadLocalVariables(method.Module, result, fatBody);
            }
            else
            {
                result.MaxStack = 8;
                result.InitializeLocals = false;
            }

            // Parse instructions.
            ReadInstructions(result, operandResolver, rawBody);

            // Read exception handlers.
            if (fatBody is not null)
                ReadExceptionHandlers(fatBody, result);

            return result;
        }

        private static void ReadLocalVariables(
            ModuleDefinition module,
            CilMethodBody result,
            CilRawFatMethodBody fatBody)
        {
            if (fatBody.LocalVarSigToken != MetadataToken.Zero
                && module.TryLookupMember(fatBody.LocalVarSigToken, out var member)
                && member is StandAloneSignature {Signature: LocalVariablesSignature localVariablesSignature})
            {
                var variableTypes = localVariablesSignature.VariableTypes;
                for (int i = 0; i < variableTypes.Count; i++)
                    result.LocalVariables.Add(new CilLocalVariable(variableTypes[i]));
            }
        }

        private static void ReadInstructions(
            CilMethodBody result,
            ICilOperandResolver operandResolver,
            CilRawMethodBody rawBody)
        {
            var reader = ByteArrayReaderFactory.CreateReader(rawBody.Code);
            var disassembler = new CilDisassembler(reader, operandResolver);
            result.Instructions.AddRange(disassembler.ReadInstructions());
        }

        private static void ReadExceptionHandlers(CilRawFatMethodBody fatBody, CilMethodBody result)
        {
            for (int i = 0; i < fatBody.ExtraSections.Count; i++)
            {
                var section = fatBody.ExtraSections[i];
                if (section.IsEHTable)
                {
                    var reader = ByteArrayReaderFactory.CreateReader(section.Data);
                    uint size = section.IsFat
                        ? CilExceptionHandler.FatExceptionHandlerSize
                        : CilExceptionHandler.TinyExceptionHandlerSize;

                    while (reader.CanRead(size))
                        result.ExceptionHandlers.Add(CilExceptionHandler.FromReader(result, ref reader, section.IsFat));
                }
            }
        }

        /// <summary>
        /// Verifies all branch targets and exception handler labels in the method body for validity.
        /// </summary>
        /// <exception cref="InvalidCilInstructionException">Occurs when one branch instruction in the method body is invalid.</exception>
        /// <exception cref="AggregateException">Occurs when multiple branch instructions in the method body are invalid.</exception>
        /// <remarks>This method will force the offsets of each instruction to be calculated.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VerifyLabels() => VerifyLabels(true);

        /// <summary>
        /// Verifies all branch targets and exception handler labels in the method body for validity.
        /// </summary>
        /// <param name="calculateOffsets">Determines whether offsets should be calculated beforehand.</param>
        /// <exception cref="InvalidCilInstructionException">Occurs when one branch instruction in the method body is invalid.</exception>
        /// <exception cref="AggregateException">Occurs when multiple branch instructions in the method body are invalid.</exception>
        public void VerifyLabels(bool calculateOffsets)
        {
            if (calculateOffsets)
                Instructions.CalculateOffsets();
            new CilLabelVerifier(this).Verify();
        }

        /// <summary>
        /// Computes the maximum values pushed onto the stack by this method body.
        /// </summary>
        /// <exception cref="StackImbalanceException">Occurs when the method body will result in an unbalanced stack.</exception>
        /// <remarks>This method will force the offsets of each instruction to be calculated.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ComputeMaxStack() => ComputeMaxStack(true);

        /// <summary>
        /// Computes the maximum values pushed onto the stack by this method body.
        /// </summary>
        /// <param name="calculateOffsets">Determines whether offsets should be calculated beforehand.</param>
        /// <exception cref="StackImbalanceException">Occurs when the method body will result in an unbalanced stack.</exception>
        public int ComputeMaxStack(bool calculateOffsets)
        {
            if (calculateOffsets)
                Instructions.CalculateOffsets();
            return new CilMaxStackCalculator(this).Compute();
        }
    }
}
