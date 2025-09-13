using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using AsmResolver.DotNet.Serialized;
using AsmResolver.PE.DotNet.Cil;

namespace AsmResolver.DotNet.Code.Cil
{
    /// <summary>
    /// Represents a method body of a method defined in a .NET assembly, implemented using the Common Intermediate Language (CIL).
    /// </summary>
    public class CilMethodBody : MethodBody
    {
        private CilInstructionCollection? _instructions;
        private IList<CilExceptionHandler>? _exceptionHandlers;

        /// <summary>
        /// Gets a value indicating whether the method body has been fully decoded.
        /// That is, all instructions and exception handlers are disassembled.
        /// </summary>
        [MemberNotNullWhen(true, nameof(_instructions))]
        [MemberNotNullWhen(true, nameof(_exceptionHandlers))]
        protected internal bool IsInitialized => _instructions is not null && _exceptionHandlers is not null;

        /// <summary>
        /// Gets a collection of instructions to be executed in the method.
        /// </summary>
        public CilInstructionCollection Instructions
        {
            get
            {
                EnsureIsInitialized();
                return _instructions;
            }
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
        public virtual bool IsFat
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
            get
            {
                EnsureIsInitialized();
                return _exceptionHandlers;
            }
        }

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
            ICilOperandResolver? operandResolver = null)
        {
            return new SerializedCilMethodBody(context, method, rawBody, operandResolver);
        }

        /// <summary>
        /// Ensures the instructions and exception handlers of this method body are fully initialized.
        /// </summary>
        [MemberNotNull(nameof(_instructions))]
        [MemberNotNull(nameof(_exceptionHandlers))]
        protected void EnsureIsInitialized()
        {
            if (IsInitialized)
                return;

            var instructions = new CilInstructionCollection(this);
            var exceptionHandlers = new List<CilExceptionHandler>();

            Initialize(instructions, exceptionHandlers);

            lock (this)
            {
                if (IsInitialized)
                    return;
                _instructions = instructions;
                _exceptionHandlers = exceptionHandlers;
            }
        }

        /// <summary>
        /// Populates the provided instructions and exception handlers lists.
        /// </summary>
        protected virtual void Initialize(
            CilInstructionCollection instructions,
            List<CilExceptionHandler> exceptionHandlers)
        {
        }

        /// <summary>
        /// Verifies all branch targets and exception handler labels in the method body for validity.
        /// </summary>
        /// <exception cref="InvalidCilInstructionException">Occurs when one branch instruction in the method body is invalid.</exception>
        /// <exception cref="AggregateException">Occurs when multiple branch instructions in the method body are invalid.</exception>
        /// <remarks>This method will force the offsets of each instruction to be calculated.</remarks>
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
