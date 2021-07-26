﻿using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Provides a mechanism for encoding CIL instructions to an output stream.
    /// </summary>
    public class CilAssembler
    {
        private readonly IBinaryStreamWriter _writer;
        private readonly ICilOperandBuilder _operandBuilder;
        private readonly string? _diagnosticPrefix;
        private readonly IErrorListener _errorListener;

        /// <summary>
        /// Creates a new CIL instruction encoder.
        /// </summary>
        /// <param name="writer">The output stream to write the encoded instructions to.</param>
        /// <param name="operandBuilder">The object to use for creating raw operands.</param>
        public CilAssembler(IBinaryStreamWriter writer, ICilOperandBuilder operandBuilder)
            : this(writer, operandBuilder, null, ThrowErrorListener.Instance)
        {
        }

        /// <summary>
        /// Creates a new CIL instruction encoder.
        /// </summary>
        /// <param name="writer">The output stream to write the encoded instructions to.</param>
        /// <param name="operandBuilder">The object to use for creating raw operands.</param>
        /// <param name="methodBodyName">The name of the method that is being serialized.</param>
        /// <param name="errorListener">The object used for recording error listener.</param>
        public CilAssembler(
            IBinaryStreamWriter writer,
            ICilOperandBuilder operandBuilder,
            string? methodBodyName,
            IErrorListener errorListener)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _errorListener = errorListener ?? throw new ArgumentNullException(nameof(errorListener));
            _operandBuilder = operandBuilder ?? throw new ArgumentNullException(nameof(operandBuilder));
            _diagnosticPrefix = !string.IsNullOrEmpty(methodBodyName)
                ? $"[In {methodBodyName}]: "
                : null;
        }

        /// <summary>
        /// Writes a collection of CIL instructions to the output stream.
        /// </summary>
        /// <param name="instructions">The instructions to write.</param>
        public void WriteInstructions(IList<CilInstruction> instructions)
        {
            for (int i = 0; i < instructions.Count; i++)
                WriteInstruction(instructions[i]);
        }

        /// <summary>
        /// Writes a single instruction to the output stream.
        /// </summary>
        /// <param name="instruction">The instruction to write.</param>
        public void WriteInstruction(CilInstruction instruction)
        {
            WriteOpCode(instruction.OpCode);
            WriteOperand(instruction);
        }

        private void WriteOpCode(CilOpCode opCode)
        {
            _writer.WriteByte(opCode.Byte1);
            if (opCode.IsLarge)
                _writer.WriteByte(opCode.Byte2);
        }

        private void WriteOperand(CilInstruction instruction)
        {
            switch (instruction.OpCode.OperandType)
            {
                case CilOperandType.InlineNone:
                    break;

                case CilOperandType.ShortInlineI:
                    _writer.WriteSByte(OperandToSByte(instruction));
                    break;

                case CilOperandType.InlineI:
                    _writer.WriteInt32(OperandToInt32(instruction));
                    break;

                case CilOperandType.InlineI8:
                    _writer.WriteInt64(OperandToInt64(instruction));
                    break;

                case CilOperandType.ShortInlineR:
                    _writer.WriteSingle(OperandToFloat32(instruction));
                    break;

                case CilOperandType.InlineR:
                    _writer.WriteDouble(OperandToFloat64(instruction));
                    break;

                case CilOperandType.ShortInlineVar:
                    _writer.WriteByte((byte) OperandToLocalIndex(instruction));
                    break;

                case CilOperandType.InlineVar:
                    _writer.WriteUInt16(OperandToLocalIndex(instruction));
                    break;

                case CilOperandType.ShortInlineArgument:
                    _writer.WriteByte((byte) OperandToArgumentIndex(instruction));
                    break;

                case CilOperandType.InlineArgument:
                    _writer.WriteUInt16(OperandToArgumentIndex(instruction));
                    break;

                case CilOperandType.ShortInlineBrTarget:
                    _writer.WriteSByte((sbyte) OperandToBranchDelta(instruction));
                    break;

                case CilOperandType.InlineBrTarget:
                    _writer.WriteInt32(OperandToBranchDelta(instruction));
                    break;

                case CilOperandType.InlineSwitch:
                    var labels = instruction.Operand as IList<ICilLabel> ?? Array.Empty<ICilLabel>();
                    _writer.WriteInt32(labels.Count);

                    int baseOffset = (int) _writer.Offset + labels.Count * sizeof(int);
                    for (int i = 0; i < labels.Count; i++)
                        _writer.WriteInt32(labels[i].Offset - baseOffset);

                    break;

                case CilOperandType.InlineString:
                    _writer.WriteUInt32(_operandBuilder.GetStringToken(instruction.Operand));
                    break;

                case CilOperandType.InlineField:
                case CilOperandType.InlineMethod:
                case CilOperandType.InlineSig:
                case CilOperandType.InlineTok:
                case CilOperandType.InlineType:
                    _writer.WriteUInt32(_operandBuilder.GetMemberToken(instruction.Operand).ToUInt32());
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private int OperandToBranchDelta(CilInstruction instruction)
        {
            bool isShort = instruction.OpCode.OperandType == CilOperandType.ShortInlineBrTarget;

            int delta;
            switch (instruction.Operand)
            {
                case sbyte x:
                    delta = x;
                    break;

                case int x:
                    delta = x;
                    break;

                case ICilLabel label:
                    int operandSize = isShort ? sizeof(sbyte) : sizeof(int);
                    delta = label.Offset - (int) (_writer.Offset + (ulong) operandSize);
                    break;

                default:
                    return ThrowInvalidOperandType<sbyte>(instruction, typeof(ICilLabel), typeof(sbyte));
            }

            if (isShort && (delta < sbyte.MinValue || delta > sbyte.MaxValue))
            {
                _errorListener.RegisterException(new OverflowException(
                    $"{_diagnosticPrefix}Branch target at IL_{instruction.Offset:X4} is too far away for a ShortInlineBr instruction."));
            }

            return delta;
        }

        private ushort OperandToLocalIndex(CilInstruction instruction)
        {
            int variableIndex = _operandBuilder.GetVariableIndex(instruction.Operand);
            if (instruction.OpCode.OperandType == CilOperandType.ShortInlineVar && variableIndex > byte.MaxValue)
            {
                _errorListener.RegisterException(new OverflowException(
                    $"{_diagnosticPrefix}Local index at IL_{instruction.Offset:X4} is too large for a ShortInlineVar instruction."));
            }

            return unchecked((ushort) variableIndex);
        }

        private ushort OperandToArgumentIndex(CilInstruction instruction)
        {
            int variableIndex = _operandBuilder.GetArgumentIndex(instruction.Operand);
            if (instruction.OpCode.OperandType == CilOperandType.ShortInlineArgument && variableIndex > byte.MaxValue)
            {
                _errorListener.RegisterException(new OverflowException(
                    $"{_diagnosticPrefix}Argument index at IL_{instruction.Offset:X4} is too large for a ShortInlineArgument instruction."));
            }

            return unchecked((ushort) variableIndex);
        }

        private sbyte OperandToSByte(CilInstruction instruction)
        {
            if (instruction.Operand is sbyte x)
                return x;
            return ThrowInvalidOperandType<sbyte>(instruction, typeof(sbyte));
        }

        private int OperandToInt32(CilInstruction instruction)
        {
            if (instruction.Operand is int x)
                return x;
            return ThrowInvalidOperandType<int>(instruction, typeof(int));
        }

        private long OperandToInt64(CilInstruction instruction)
        {
            if (instruction.Operand is long x)
                return x;
            return ThrowInvalidOperandType<long>(instruction, typeof(long));
        }

        private float OperandToFloat32(CilInstruction instruction)
        {
            if (instruction.Operand is float x)
                return x;
            return ThrowInvalidOperandType<float>(instruction, typeof(float));
        }

        private double OperandToFloat64(CilInstruction instruction)
        {
            if (instruction.Operand is double x)
                return x;
            return ThrowInvalidOperandType<double>(instruction, typeof(double));
        }

        private T? ThrowInvalidOperandType<T>(CilInstruction instruction, Type expectedOperand)
        {
            string found = instruction.Operand?.GetType().Name ?? "null";
            _errorListener.RegisterException(new ArgumentOutOfRangeException(
                $"{_diagnosticPrefix}Expected a {expectedOperand.Name} operand at IL_{instruction.Offset:X4}, but found {found}."));
            return default;
        }

        private T? ThrowInvalidOperandType<T>(CilInstruction instruction, params Type[] expectedOperands)
        {
            string[] names = expectedOperands
                .Select(o => o.Name)
                .ToArray();

            string operandTypesString = expectedOperands.Length > 1
                ? $"{string.Join(", ", names.Take(names.Length - 1))} or {names[names.Length - 1]}"
                : names[0];

            string found = instruction.Operand?.GetType().Name ?? "null";
            _errorListener.RegisterException(new ArgumentOutOfRangeException(
                $"{_diagnosticPrefix}Expected a {operandTypesString} operand at IL_{instruction.Offset:X4}, but found {found}."));
            return default;
        }
    }
}
