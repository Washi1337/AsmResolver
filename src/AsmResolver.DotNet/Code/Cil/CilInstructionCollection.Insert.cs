using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Code.Cil
{
    public partial class CilInstructionCollection
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private CilInstruction InsertAndReturn(int index, CilOpCode code, object operand = null)
        {
            var instruction = new CilInstruction(code, operand);
            Insert(index, instruction);
            return instruction;
        }

        /// <summary>
        /// Verifies and inserts an instruction into the collection.
        /// </summary>
        /// <param name="index">The zero-based index at which the instruction should be inserted at.</param>
        /// <param name="code">The code.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">Occurs when the provided operation requires an operand.</exception>
        public CilInstruction Insert(int index, CilOpCode code)
        {
            if (code.OperandType != CilOperandType.InlineNone)
                throw new InvalidCilInstructionException(code);

            return InsertAndReturn(index, code);
        }

        /// <summary>
        /// Verifies and inserts a branch instruction into the collection.
        /// </summary>
        /// <param name="index">The zero-based index at which the instruction should be inserted at.</param>
        /// <param name="code">The branch opcode.</param>
        /// <param name="label">The label referenced by the branch instruction.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not a branch opcode.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Occurs when <paramref name="label"/> is null.
        /// </exception>
        public CilInstruction Insert(int index, CilOpCode code, ICilLabel label)
        {
            if (code.OperandType != CilOperandType.InlineBrTarget && code.OperandType != CilOperandType.ShortInlineBrTarget)
                throw new InvalidCilInstructionException(code);
            if (label is null)
                throw new ArgumentNullException(nameof(label));

            return InsertAndReturn(index, code, label);
        }

        /// <summary>
        /// Verifies and inserts a switch instruction into the collection.
        /// </summary>
        /// <param name="index">The zero-based index at which the instruction should be inserted at.</param>
        /// <param name="code">The switch opcode.</param>
        /// <param name="labels">The labels referenced by the switch instruction.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not a branch opcode.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Occurs when <paramref name="labels"/> is null.
        /// </exception>
        public CilInstruction Insert(int index, CilOpCode code, params ICilLabel[] labels) => 
            Add(code, (IEnumerable<ICilLabel>) labels);

        /// <summary>
        /// Verifies and inserts a switch instruction into the collection.
        /// </summary>
        /// <param name="index">The zero-based index at which the instruction should be inserted at.</param>
        /// <param name="code">The switch opcode.</param>
        /// <param name="labels">The labels referenced by the switch instruction.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not a branch opcode.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Occurs when <paramref name="labels"/> is null.
        /// </exception>
        public CilInstruction Insert(int index, CilOpCode code, IEnumerable<ICilLabel> labels)
        {
            if (code.OperandType != CilOperandType.InlineSwitch)
                throw new InvalidCilInstructionException(code);
            if (labels is null)
                throw new ArgumentNullException(nameof(labels));

            return InsertAndReturn(index, code, labels.ToList());
        }

        /// <summary>
        /// Verifies and inserts a instruction into the collection that pushes an integer constant.
        /// </summary>
        /// <param name="index">The zero-based index at which the instruction should be inserted at.</param>
        /// <param name="code">The opcode.</param>
        /// <param name="constant">The constant to push.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not an opcode referencing an integer constant.
        /// </exception>
        public CilInstruction Insert(int index, CilOpCode code, int constant)
        {
            object operand = code.OperandType switch
            {
                CilOperandType.InlineI => constant,
                CilOperandType.ShortInlineI => (sbyte) constant,
                _ => throw new InvalidCilInstructionException(code)
            };

            return InsertAndReturn(index, code, operand);
        }

        /// <summary>
        /// Verifies and inserts a instruction into the collection that pushes a 64-bit integer constant.
        /// </summary>
        /// <param name="index">The zero-based index at which the instruction should be inserted at.</param>
        /// <param name="code">The opcode.</param>
        /// <param name="constant">The constant to push.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not an opcode referencing a 64-bit integer constant.
        /// </exception>
        public CilInstruction Insert(int index, CilOpCode code, long constant)
        {
            if (code.OperandType != CilOperandType.InlineI8)
                throw new InvalidCilInstructionException(code);

            return InsertAndReturn(index, code, constant);
        }

        /// <summary>
        /// Verifies and inserts a instruction into the collection that references a float32 constant.
        /// </summary>
        /// <param name="index">The zero-based index at which the instruction should be inserted at.</param>
        /// <param name="code">The opcode.</param>
        /// <param name="constant">The constant to push.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not an opcode referencing a float32 constant.
        /// </exception>
        public CilInstruction Insert(int index, CilOpCode code, float constant)
        {
            if (code.OperandType != CilOperandType.ShortInlineR)
                throw new InvalidCilInstructionException(code);

            return InsertAndReturn(index, code, constant);
        }

        /// <summary>
        /// Verifies and inserts a instruction into the collection that references a float64 constant.
        /// </summary>
        /// <param name="index">The zero-based index at which the instruction should be inserted at.</param>
        /// <param name="code">The opcode.</param>
        /// <param name="constant">The constant to push.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not an opcode referencing a float64 constant.
        /// </exception>
        public CilInstruction Insert(int index, CilOpCode code, double constant)
        {
            if (code.OperandType != CilOperandType.InlineR)
                throw new InvalidCilInstructionException(code);

            return InsertAndReturn(index, code, constant);
        }

        /// <summary>
        /// Verifies and inserts a instruction into the collection that pushes a string constant.
        /// </summary>
        /// <param name="index">The zero-based index at which the instruction should be inserted at.</param>
        /// <param name="code">The opcode.</param>
        /// <param name="constant">The constant to push.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not an opcode referencing a string constant.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Occurs when <paramref name="constant"/> is null.
        /// </exception>
        public CilInstruction Insert(int index, CilOpCode code, string constant)
        {
            if (code.OperandType != CilOperandType.InlineString)
                throw new InvalidCilInstructionException(code);

            if (constant is null)
                throw new ArgumentNullException(nameof(constant));

            return InsertAndReturn(index, code, constant);
        }

        /// <summary>
        /// Verifies and inserts a instruction into the collection that references a local variable.
        /// </summary>
        /// <param name="index">The zero-based index at which the instruction should be inserted at.</param>
        /// <param name="code">The opcode.</param>
        /// <param name="variable">The referenced variable.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not an opcode referencing a variable.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Occurs when <paramref name="variable"/> is null.
        /// </exception>
        public CilInstruction Insert(int index, CilOpCode code, CilLocalVariable variable)
        {
            if (code.OperandType != CilOperandType.InlineVar && code.OperandType != CilOperandType.ShortInlineVar)
                throw new InvalidCilInstructionException(code);

            if (variable is null)
                throw new ArgumentNullException(nameof(variable));

            return InsertAndReturn(index, code, variable);
        }

        /// <summary>
        /// Verifies and inserts a instruction into the collection that references a parameter.
        /// </summary>
        /// <param name="index">The zero-based index at which the instruction should be inserted at.</param>
        /// <param name="code">The opcode.</param>
        /// <param name="parameter">The referenced parameter.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not an opcode referencing a parameter.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Occurs when <paramref name="parameter"/> is null.
        /// </exception>
        public CilInstruction Insert(int index, CilOpCode code, Parameter parameter)
        {
            if (code.OperandType != CilOperandType.InlineArgument && code.OperandType != CilOperandType.ShortInlineArgument)
                throw new InvalidCilInstructionException(code);

            if (parameter is null)
                throw new ArgumentNullException(nameof(parameter));

            return InsertAndReturn(index, code, parameter);
        }

        /// <summary>
        /// Verifies and inserts an instruction into the collection that references a field.
        /// </summary>
        /// <param name="index">The zero-based index at which the instruction should be inserted at.</param>
        /// <param name="code">The field opcode.</param>
        /// <param name="field">The field referenced by the instruction.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not a field opcode.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Occurs when <paramref name="field"/> is null.
        /// </exception>
        public CilInstruction Insert(int index, CilOpCode code, IFieldDescriptor field)
        {
            if (code.OperandType != CilOperandType.InlineField && code.OperandType != CilOperandType.InlineTok)
                throw new InvalidCilInstructionException(code);

            if (field is null)
                throw new ArgumentNullException(nameof(field));

            return InsertAndReturn(index, code, field);
        }

        /// <summary>
        /// Verifies and inserts an instruction into the collection that references a method.
        /// </summary>
        /// <param name="index">The zero-based index at which the instruction should be inserted at.</param>
        /// <param name="code">The method opcode.</param>
        /// <param name="method">The method referenced by the instruction.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not a method opcode.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Occurs when <paramref name="method"/> is null.
        /// </exception>
        public CilInstruction Insert(int index, CilOpCode code, IMethodDescriptor method)
        {
            if (code.OperandType != CilOperandType.InlineMethod && code.OperandType != CilOperandType.InlineTok)
                throw new InvalidCilInstructionException(code);

            if (method is null)
                throw new ArgumentNullException(nameof(method));

            return InsertAndReturn(index, code, method);
        }

        /// <summary>
        /// Verifies and inserts an instruction into the collection that references a type.
        /// </summary>
        /// <param name="index">The zero-based index at which the instruction should be inserted at.</param>
        /// <param name="code">The type opcode.</param>
        /// <param name="type">The type referenced by the instruction.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not a type opcode.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Occurs when <paramref name="type"/> is null.
        /// </exception>
        public CilInstruction Insert(int index, CilOpCode code, ITypeDefOrRef type)
        {
            if (code.OperandType != CilOperandType.InlineType && code.OperandType != CilOperandType.InlineTok)
                throw new InvalidCilInstructionException(code);

            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return InsertAndReturn(index, code, type);
        }

        /// <summary>
        /// Verifies and inserts an instruction into the collection that references a metadata member.
        /// </summary>
        /// <param name="index">The zero-based index at which the instruction should be inserted at.</param>
        /// <param name="code">The method opcode.</param>
        /// <param name="member">The member referenced by the instruction.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not a member opcode.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Occurs when <paramref name="member"/> is null.
        /// </exception>
        public CilInstruction Insert(int index, CilOpCode code, IMetadataMember member)
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

            return InsertAndReturn(index, code, member);
        }

        /// <summary>
        /// Verifies and inserts a instruction into the collection that references a standalone signature.
        /// </summary>
        /// <param name="index">The zero-based index at which the instruction should be inserted at.</param>
        /// <param name="code">The opcode.</param>
        /// <param name="signature">The referenced signature.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not an opcode referencing a standalone signature.
        /// </exception>
        public CilInstruction Insert(int index, CilOpCode code, StandAloneSignature signature)
        {
            if (code.OperandType != CilOperandType.InlineSig)
                throw new InvalidCilInstructionException(code);

            return InsertAndReturn(index, code, signature);
        }

        /// <summary>
        /// Verifies and inserts a instruction into the collection that references a metadata member by its token.
        /// </summary>
        /// <param name="index">The zero-based index at which the instruction should be inserted at.</param>
        /// <param name="code">The opcode.</param>
        /// <param name="token">The token of the referenced member.</param>
        /// <returns>The created instruction.</returns>
        /// <exception cref="InvalidCilInstructionException">
        /// Occurs when the provided operation is not an opcode referencing a metadata member.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the provided token is not valid in a CIL stream.
        /// </exception>
        public CilInstruction Insert(int index, CilOpCode code, MetadataToken token)
        {
            switch (code.OperandType)
            {
                case CilOperandType.InlineField:
                case CilOperandType.InlineMethod:
                case CilOperandType.InlineSig:
                case CilOperandType.InlineTok:
                case CilOperandType.InlineType:
                case CilOperandType.InlineString:
                    break;
                default:
                    throw new InvalidCilInstructionException(code);
            }

            switch (token.Table)
            {
                case TableIndex.TypeRef:
                case TableIndex.TypeDef:
                case TableIndex.TypeSpec:
                case TableIndex.Field:
                case TableIndex.Method:
                case TableIndex.MethodSpec:
                case TableIndex.MemberRef:
                case TableIndex.StandAloneSig:
                case (TableIndex) 0x70:
                    break;
                default:
                    throw new InvalidCilInstructionException(code);
            }

            return InsertAndReturn(index, code, token);
        }
        
    }
}