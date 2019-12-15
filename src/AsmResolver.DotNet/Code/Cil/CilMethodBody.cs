using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Blob;
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
        /// Creates a CIL method body from a raw CIL method body. 
        /// </summary>
        /// <param name="method">The method that owns the method body.</param>
        /// <param name="rawBody">The raw method body.</param>
        /// <param name="operandResolver">The object instance to use for resolving operands of an instruction in the
        /// method body.</param>
        /// <returns>The method body.</returns>
        public static CilMethodBody FromRawMethodBody(MethodDefinition method, CilRawMethodBody rawBody, ICilOperandResolver operandResolver = null)
        {
            var result = new CilMethodBody(method);

            if (operandResolver is null)
                operandResolver = result;
            
            ReadInstructions(result, rawBody, operandResolver);

            if (rawBody is CilRawFatMethodBody fatBody)
            {
                result.MaxStack = fatBody.MaxStack;
                result.InitializeLocals = fatBody.InitLocals;

                ReadLocalVariables(method, result, fatBody);
                
                // TODO: Add exception handlers.
            }
            else
            {
                result.MaxStack = 8;
                result.InitializeLocals = false;
            }
            
            return result;
        }

        private static void ReadInstructions(CilMethodBody result, CilRawMethodBody rawBody,
            ICilOperandResolver operandResolver)
        {
            var reader = new ByteArrayReader(rawBody.Code);
            var disassembler = new CilDisassembler(reader);
            result.Instructions.AddRange(disassembler.ReadAllInstructions());

            // Resolve operands.
            foreach (var instruction in result.Instructions)
                instruction.Operand = ResolveOperand(result, instruction, operandResolver) ?? instruction.Operand;
        }

        private static object ResolveOperand(CilMethodBody methodBody, CilInstruction instruction, ICilOperandResolver resolver)
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
                        var target = methodBody.Instructions.GetByOffset(((ICilLabel) instruction.Operand).Offset);
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

        private static void ReadLocalVariables(MethodDefinition method, CilMethodBody result, CilRawFatMethodBody fatBody)
        {
            if (fatBody.LocalVarSigToken != MetadataToken.Zero
                && method.Module.TryLookupMember(fatBody.LocalVarSigToken, out var member)
                && member is StandAloneSignature signature
                && signature.Signature is LocalVariablesSignature localVariablesSignature)
            {
                foreach (var type in localVariablesSignature.VariableTypes)
                    result.LocalVariables.Add(new CilLocalVariable(type));
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
        /// Gets a collection of local variables defined in the method body.
        /// </summary>
        public CilLocalVariableCollection LocalVariables
        {
            get;
        } = new CilLocalVariableCollection();
        
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
            return index >= 0 && index < parameters.Count ? parameters[index] : null;
        }
    } 
}