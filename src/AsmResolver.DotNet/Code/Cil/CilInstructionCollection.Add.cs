using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Cil;

namespace AsmResolver.DotNet.Code.Cil
{
    public partial class CilInstructionCollection
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private CilInstruction AddAndReturn(CilOpCode code, object operand = null)
        {
            var instruction = new CilInstruction(code, operand);
            Add(instruction);
            return instruction;
        }
        
        /// <summary>
        /// Adds an instruction to the end of the collection.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">Occurs when the provided operation requires an operand.</exception>
        public CilInstruction Add(CilOpCode code)
        {
            if (code.OperandType != CilOperandType.InlineNone)
                throw new InvalidCilInstructionException(code);

            return AddAndReturn(code);
        }

        /// <summary>
        /// Adds a branch instruction to the end of the collection.
        /// </summary>
        /// <param name="code">The branch opcode.</param>
        /// <param name="label">The label referenced by the branch instruction.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not a branch opcode.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Occurs when <paramref name="label"/> is null.
        /// </exception>
        public CilInstruction Add(CilOpCode code, ICilLabel label)
        {
            if (code.OperandType != CilOperandType.InlineBrTarget && code.OperandType != CilOperandType.ShortInlineBrTarget)
                throw new InvalidCilInstructionException(code);
            if (label is null)
                throw new ArgumentNullException(nameof(label));

            return AddAndReturn(code, label);
        }

        /// <summary>
        /// Adds a switch instruction to the end of the collection.
        /// </summary>
        /// <param name="code">The switch opcode.</param>
        /// <param name="labels">The labels referenced by the switch instruction.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not a branch opcode.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Occurs when <paramref name="labels"/> is null.
        /// </exception>
        public CilInstruction Add(CilOpCode code, params ICilLabel[] labels) => 
            Add(code, (IEnumerable<ICilLabel>) labels);

        /// <summary>
        /// Adds a switch instruction to the end of the collection.
        /// </summary>
        /// <param name="code">The switch opcode.</param>
        /// <param name="labels">The labels referenced by the switch instruction.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not a branch opcode.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Occurs when <paramref name="labels"/> is null.
        /// </exception>
        public CilInstruction Add(CilOpCode code, IEnumerable<ICilLabel> labels)
        {
            if (code.OperandType != CilOperandType.InlineSwitch)
                throw new InvalidCilInstructionException(code);
            if (labels is null)
                throw new ArgumentNullException(nameof(labels));

            return AddAndReturn(code, labels.ToList());
        }

        /// <summary>
        /// Adds a instruction to the end of the collection that pushes an integer constant.
        /// </summary>
        /// <param name="code">The opcode.</param>
        /// <param name="constant">The constant to push.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not an opcode referencing an integer constant.
        /// </exception>
        public CilInstruction Add(CilOpCode code, int constant)
        {
            object operand = code.OperandType switch
            {
                CilOperandType.InlineI => constant,
                CilOperandType.ShortInlineI => (sbyte) constant,
                _ => throw new InvalidCilInstructionException(code)
            };

            return AddAndReturn(code, operand);
        }

        /// <summary>
        /// Adds a instruction to the end of the collection that pushes a 64-bit integer constant.
        /// </summary>
        /// <param name="code">The opcode.</param>
        /// <param name="constant">The constant to push.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not an opcode referencing a 64-bit integer constant.
        /// </exception>
        public CilInstruction Add(CilOpCode code, long constant)
        {
            if (code.OperandType != CilOperandType.InlineI8)
                throw new InvalidCilInstructionException(code);

            return AddAndReturn(code, constant);
        }

        /// <summary>
        /// Adds a instruction to the end of the collection that references a float32 constant.
        /// </summary>
        /// <param name="code">The opcode.</param>
        /// <param name="constant">The constant to push.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not an opcode referencing a float32 constant.
        /// </exception>
        public CilInstruction Add(CilOpCode code, float constant)
        {
            if (code.OperandType != CilOperandType.ShortInlineR)
                throw new InvalidCilInstructionException(code);

            return AddAndReturn(code, constant);
        }

        /// <summary>
        /// Adds a instruction to the end of the collection that references a float64 constant.
        /// </summary>
        /// <param name="code">The opcode.</param>
        /// <param name="constant">The constant to push.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not an opcode referencing a float64 constant.
        /// </exception>
        public CilInstruction Add(CilOpCode code, double constant)
        {
            if (code.OperandType != CilOperandType.InlineR)
                throw new InvalidCilInstructionException(code);

            return AddAndReturn(code, constant);
        }

        /// <summary>
        /// Adds a instruction to the end of the collection that pushes a string constant.
        /// </summary>
        /// <param name="code">The opcode.</param>
        /// <param name="constant">The constant to push.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not an opcode referencing a string constant.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Occurs when <paramref name="constant"/> is null.
        /// </exception>
        public CilInstruction Add(CilOpCode code, string constant)
        {
            if (code.OperandType != CilOperandType.InlineString)
                throw new InvalidCilInstructionException(code);

            if (constant is null)
                throw new ArgumentNullException(nameof(constant));

            return AddAndReturn(code, constant);
        }

        /// <summary>
        /// Adds a instruction to the end of the collection that references a local variable.
        /// </summary>
        /// <param name="code">The opcode.</param>
        /// <param name="variable">The referenced variable.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not an opcode referencing a variable.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Occurs when <paramref name="variable"/> is null.
        /// </exception>
        public CilInstruction Add(CilOpCode code, CilLocalVariable variable)
        {
            if (code.OperandType != CilOperandType.InlineVar && code.OperandType != CilOperandType.ShortInlineVar)
                throw new InvalidCilInstructionException(code);

            if (variable is null)
                throw new ArgumentNullException(nameof(variable));

            return AddAndReturn(code, variable);
        }

        /// <summary>
        /// Adds a instruction to the end of the collection that references a parameter.
        /// </summary>
        /// <param name="code">The opcode.</param>
        /// <param name="parameter">The referenced parameter.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not an opcode referencing a parameter.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Occurs when <paramref name="parameter"/> is null.
        /// </exception>
        public CilInstruction Add(CilOpCode code, Parameter parameter)
        {
            if (code.OperandType != CilOperandType.InlineArgument && code.OperandType != CilOperandType.ShortInlineArgument)
                throw new InvalidCilInstructionException(code);

            if (parameter is null)
                throw new ArgumentNullException(nameof(parameter));

            return AddAndReturn(code, parameter);
        }

        /// <summary>
        /// Adds an instruction to the end of the collection that references a field.
        /// </summary>
        /// <param name="code">The field opcode.</param>
        /// <param name="field">The field referenced by the instruction.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not a field opcode.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Occurs when <paramref name="field"/> is null.
        /// </exception>
        public CilInstruction Add(CilOpCode code, IFieldDescriptor field)
        {
            if (code.OperandType != CilOperandType.InlineField && code.OperandType != CilOperandType.InlineTok)
                throw new InvalidCilInstructionException(code);

            if (field is null)
                throw new ArgumentNullException(nameof(field));

            return AddAndReturn(code, field);
        }

        /// <summary>
        /// Adds an instruction to the end of the collection that references a method.
        /// </summary>
        /// <param name="code">The method opcode.</param>
        /// <param name="method">The method referenced by the instruction.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not a method opcode.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Occurs when <paramref name="method"/> is null.
        /// </exception>
        public CilInstruction Add(CilOpCode code, IMethodDescriptor method)
        {
            if (code.OperandType != CilOperandType.InlineMethod && code.OperandType != CilOperandType.InlineTok)
                throw new InvalidCilInstructionException(code);

            if (method is null)
                throw new ArgumentNullException(nameof(method));

            return AddAndReturn(code, method);
        }

        /// <summary>
        /// Adds an instruction to the end of the collection that references a type.
        /// </summary>
        /// <param name="code">The type opcode.</param>
        /// <param name="type">The type referenced by the instruction.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not a type opcode.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Occurs when <paramref name="type"/> is null.
        /// </exception>
        public CilInstruction Add(CilOpCode code, ITypeDefOrRef type)
        {
            if (code.OperandType != CilOperandType.InlineType && code.OperandType != CilOperandType.InlineTok)
                throw new InvalidCilInstructionException(code);

            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return AddAndReturn(code, type);
        }

        /// <summary>
        /// Adds an instruction to the end of the collection that references a metadata member.
        /// </summary>
        /// <param name="code">The method opcode.</param>
        /// <param name="member">The member referenced by the instruction.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not a member opcode.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Occurs when <paramref name="member"/> is null.
        /// </exception>
        public CilInstruction Add(CilOpCode code, IMetadataMember member)
        {
            switch (code.OperandType)
            {
                case CilOperandType.InlineField:
                case CilOperandType.InlineMethod:
                case CilOperandType.InlineSig:
                case CilOperandType.InlineTok:
                case CilOperandType.InlineType:
                    break;
                default:
                    throw new InvalidCilInstructionException(code);
            }

            if (member is null)
                throw new ArgumentNullException(nameof(member));

            return AddAndReturn(code, member);
        }

        /// <summary>
        /// Adds a instruction to the end of the collection that references a standalone signature.
        /// </summary>
        /// <param name="code">The opcode.</param>
        /// <param name="signature">The referenced signature.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not an opcode referencing a standalone signature.
        /// </exception>
        public CilInstruction Add(CilOpCode code, StandAloneSignature signature)
        {
            if (code.OperandType != CilOperandType.InlineSig)
                throw new InvalidCilInstructionException(code);

            return AddAndReturn(code, signature);
        }
    }
}