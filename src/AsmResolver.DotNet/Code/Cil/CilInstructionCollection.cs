using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Cil;

namespace AsmResolver.DotNet.Code.Cil
{
    /// <summary>
    /// Represents a collection of CIL instructions found in a method body.
    /// </summary>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class CilInstructionCollection : IList<CilInstruction>
    {
        private readonly List<CilInstruction> _items = new List<CilInstruction>();

        /// <summary>
        /// Creates a new collection of CIL instructions stored in a method body.
        /// </summary>
        /// <param name="body">The method body that owns the collection.</param>
        public CilInstructionCollection(CilMethodBody body)
        {
            Owner = body ?? throw new ArgumentNullException(nameof(body));
        }

        /// <summary>
        /// Gets the method body that owns the collection of CIL instructions.
        /// </summary>
        public CilMethodBody Owner
        {
            get;
        }

        /// <inheritdoc />
        public int Count => _items.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets the size in bytes of the collection.
        /// </summary>
        public int Size => _items.Sum(x => x.Size);
        
        /// <inheritdoc />
        public CilInstruction this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }
        
        /// <inheritdoc />
        public void Add(CilInstruction item) => _items.Add(item);

        /// <summary>
        /// Adds a collection of CIL instructions to the end of the list.
        /// </summary>
        /// <param name="items">The instructions to add.</param>
        public void AddRange(IEnumerable<CilInstruction> items) => _items.AddRange(items);

        /// <summary>
        /// Inserts a collection of CIL instructions at the provided index.
        /// </summary>
        /// <param name="index">The index to insert the instructions into.</param>
        /// <param name="items">The instructions to insert.</param>
        public void InsertRange(int index, IEnumerable<CilInstruction> items) => _items.InsertRange(index, items);

        /// <inheritdoc />
        public void Clear() => _items.Clear();

        /// <inheritdoc />
        public bool Contains(CilInstruction item) => _items.Contains(item);

        /// <inheritdoc />
        public void CopyTo(CilInstruction[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public bool Remove(CilInstruction item) => _items.Remove(item);

        /// <summary>
        /// Removes a range of CIL instructions from the collection.
        /// </summary>
        /// <param name="index">The starting index.</param>
        /// <param name="count">The number of instructions to remove.</param>
        public void RemoveRange(int index, int count) => _items.RemoveRange(index, count);

        /// <inheritdoc />
        public int IndexOf(CilInstruction item) => _items.IndexOf(item);

        /// <inheritdoc />
        public void Insert(int index, CilInstruction item) => _items.Insert(index, item);

        /// <inheritdoc />
        public void RemoveAt(int index) => _items.RemoveAt(index);

        /// <summary>
        /// Removes a set of CIL instructions based on a list of indices that are relative to a starting index.
        /// </summary>
        /// <param name="baseIndex">The base index.</param>
        /// <param name="relativeIndices">The indices relative to <paramref name="baseIndex"/> to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when any relative index in <paramref name="relativeIndices"/> results in an index that is
        /// out of bounds of the instruction collection.
        /// </exception>
        public void RemoveAt(int baseIndex, params int[] relativeIndices) =>
            RemoveAt(baseIndex, relativeIndices.AsEnumerable());
        
        /// <summary>
        /// Removes a set of CIL instructions based on a list of indices that are relative to a starting index.
        /// </summary>
        /// <param name="baseIndex">The base index.</param>
        /// <param name="relativeIndices">The indices relative to <paramref name="baseIndex"/> to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when any relative index in <paramref name="relativeIndices"/> results in an index that is
        /// out of bounds of the instruction collection.
        /// </exception>
        public void RemoveAt(int baseIndex, IEnumerable<int> relativeIndices)
        {
            // Verify and translate relative indices into absolute indices.
            var absoluteIndices = new List<int>();
            foreach (int relativeIndex in relativeIndices.Distinct())
            {
                int absoluteIndex = baseIndex + relativeIndex;
                if (absoluteIndex < 0 || absoluteIndex >= _items.Count)
                    throw new ArgumentOutOfRangeException(nameof(relativeIndices));
                absoluteIndices.Add(absoluteIndex);
            }

            absoluteIndices.Sort();
            
            // Remove indices.
            for (int i = 0; i < absoluteIndices.Count; i++)
            {
                int index = absoluteIndices[i];
                _items.RemoveAt(index);
                
                // Removal of instruction offsets all remaining indices by one. Update remaining indices. 
                for (int j = i+1; j < absoluteIndices.Count; j++)
                    absoluteIndices[j]--;
            }
        }

        /// <summary>
        /// Returns an enumerator that enumerates through the instructions sequentially.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public Enumerator GetEnumerator() => new Enumerator(this);

        /// <inheritdoc />
        IEnumerator<CilInstruction> IEnumerable<CilInstruction>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Searches for an instruction with the given offset.
        /// </summary>
        /// <param name="offset">The offset of the instruction to find.</param>
        /// <returns>The index the instruction is located at, or -1 if an instruction with the provided offset could not
        /// be found.</returns>
        /// <remarks>Requires the offsets of the instructions pre-calculated. This can be done by calling
        /// <see cref="CalculateOffsets"/> prior to calling this method.</remarks>
        public int GetIndexByOffset(int offset)
        {
            int left = 0;
            int right = Count - 1;

            while (left <= right)
            {
                int m = (left + right) / 2;
                int currentOffset = _items[m].Offset;

                if (currentOffset > offset)
                    right = m - 1;
                else if (currentOffset < offset)
                    left = m + 1;
                else
                    return m;
            }

            return -1;
        }
        
        /// <summary>
        /// Searches for an instruction with the given offset.
        /// </summary>
        /// <param name="offset">The offset of the instruction to find.</param>
        /// <returns>The instruction with the provided offset, or null if none could be found.</returns>
        /// <remarks>Requires the offsets of the instructions pre-calculated. This can be done by calling
        /// <see cref="CalculateOffsets"/> prior to calling this method.</remarks>
        public CilInstruction GetByOffset(int offset)
        {
            int index = GetIndexByOffset(offset);
            return index == -1 ? null : _items[index];
        }

        /// <summary>
        /// Calculates the offsets of each instruction in the list. 
        /// </summary>
        public void CalculateOffsets()
        {
            var currentOffset = 0;
            foreach (var instruction in _items)
            {
                instruction.Offset = currentOffset;
                currentOffset += instruction.Size;
            }
        }

        /// <summary>
        /// Simplifies the CIL instructions by transforming macro instructions to their expanded form.
        /// </summary>
        /// <remarks>
        /// This method reverses any optimizations done by <see cref="OptimizeMacros"/>.
        /// </remarks>
        public void ExpandMacros()
        {
            foreach (var instruction in _items)
            {
                ExpandMacro(instruction);
            }
        }

        private void ExpandMacro(CilInstruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case CilCode.Ldc_I4_0:
                case CilCode.Ldc_I4_1:
                case CilCode.Ldc_I4_2:
                case CilCode.Ldc_I4_3:
                case CilCode.Ldc_I4_4:
                case CilCode.Ldc_I4_5:
                case CilCode.Ldc_I4_6:
                case CilCode.Ldc_I4_7:
                case CilCode.Ldc_I4_8:
                case CilCode.Ldc_I4_M1:
                case CilCode.Ldc_I4_S:
                    instruction.Operand = instruction.GetLdcI4Constant();
                    instruction.OpCode = CilOpCodes.Ldc_I4;
                    break;

                case CilCode.Ldarg_0:
                case CilCode.Ldarg_1:
                case CilCode.Ldarg_2:
                case CilCode.Ldarg_3:
                case CilCode.Ldarg_S:
                    instruction.Operand = instruction.GetParameter(Owner.Owner.Parameters);
                    instruction.OpCode = CilOpCodes.Ldarg;
                    break;

                case CilCode.Ldarga_S:
                    instruction.OpCode = CilOpCodes.Ldarga_S;
                    break;

                case CilCode.Starg_S:
                    instruction.OpCode = CilOpCodes.Starg;
                    break;

                case CilCode.Ldloc_0:
                case CilCode.Ldloc_1:
                case CilCode.Ldloc_2:
                case CilCode.Ldloc_3:
                case CilCode.Ldloc_S:
                    instruction.Operand = instruction.GetLocalVariable(Owner.LocalVariables);
                    instruction.OpCode = CilOpCodes.Ldloc;
                    break;

                case CilCode.Ldloca_S:
                    instruction.OpCode = CilOpCodes.Ldloca;
                    break;

                case CilCode.Stloc_0:
                case CilCode.Stloc_1:
                case CilCode.Stloc_2:
                case CilCode.Stloc_3:
                case CilCode.Stloc_S:
                    instruction.Operand = instruction.GetLocalVariable(Owner.LocalVariables);
                    instruction.OpCode = CilOpCodes.Stloc;
                    break;

                case CilCode.Beq_S:
                    instruction.OpCode = CilOpCodes.Beq;
                    break;
                case CilCode.Bge_S:
                    instruction.OpCode = CilOpCodes.Bge;
                    break;
                case CilCode.Bgt_S:
                    instruction.OpCode = CilOpCodes.Bgt;
                    break;
                case CilCode.Ble_S:
                    instruction.OpCode = CilOpCodes.Ble;
                    break;
                case CilCode.Blt_S:
                    instruction.OpCode = CilOpCodes.Blt;
                    break;
                case CilCode.Br_S:
                    instruction.OpCode = CilOpCodes.Br;
                    break;
                case CilCode.Brfalse_S:
                    instruction.OpCode = CilOpCodes.Brfalse;
                    break;
                case CilCode.Brtrue_S:
                    instruction.OpCode = CilOpCodes.Brtrue;
                    break;
                case CilCode.Bge_Un_S:
                    instruction.OpCode = CilOpCodes.Bge_Un_S;
                    break;
                case CilCode.Bgt_Un_S:
                    instruction.OpCode = CilOpCodes.Bgt_Un;
                    break;
                case CilCode.Ble_Un_S:
                    instruction.OpCode = CilOpCodes.Ble_Un;
                    break;
                case CilCode.Blt_Un_S:
                    instruction.OpCode = CilOpCodes.Blt_Un;
                    break;
                case CilCode.Bne_Un_S:
                    instruction.OpCode = CilOpCodes.Bne_Un;
                    break;
                case CilCode.Leave_S:
                    instruction.OpCode = CilOpCodes.Leave;
                    break;
            }
        }

        /// <summary>
        /// Optimizes all instructions to use the least amount of bytes required to encode all operations.
        /// </summary>
        /// <remarks>
        /// This method reverses any effects introduced by <see cref="ExpandMacros"/>.
        /// </remarks>
        public void OptimizeMacros()
        {
            while (OptimizeMacrosPass())
            {
                // Repeat until no more optimizations can be done.
            }
            
            CalculateOffsets();
        }

        private bool OptimizeMacrosPass()
        {
            CalculateOffsets();
            
            bool changed = false;
            foreach (var instruction in _items)
                changed |= TryOptimizeMacro(instruction);

            return changed;
        }

        private bool TryOptimizeMacro(CilInstruction instruction)
        {
            return instruction.OpCode.OperandType switch
            {
                CilOperandType.InlineBrTarget => TryOptimizeBranch(instruction),
                CilOperandType.ShortInlineBrTarget => TryOptimizeBranch(instruction),
                CilOperandType.InlineI => TryOptimizeLdc(instruction),
                CilOperandType.ShortInlineI => TryOptimizeLdc(instruction),
                CilOperandType.InlineVar => TryOptimizeVariable(instruction),
                CilOperandType.ShortInlineVar => TryOptimizeVariable(instruction),
                CilOperandType.InlineArgument => TryOptimizeArgument(instruction),
                CilOperandType.ShortInlineArgument => TryOptimizeArgument(instruction),
                _ => false
            };
        }

        private bool TryOptimizeBranch(CilInstruction instruction)
        {
            int nextOffset = instruction.Offset + instruction.Size;
            int targetOffset = ((ICilLabel) instruction.Operand).Offset;
            int delta = targetOffset - nextOffset;
            bool isShortJump = delta >= sbyte.MinValue && delta <= sbyte.MaxValue;

            CilOpCode code;
            if (isShortJump)
            {
                code = instruction.OpCode.Code switch
                {
                    CilCode.Beq => CilOpCodes.Beq_S,
                    CilCode.Bge => CilOpCodes.Bge_S,
                    CilCode.Bgt => CilOpCodes.Bgt_S,
                    CilCode.Ble => CilOpCodes.Ble_S,
                    CilCode.Blt => CilOpCodes.Blt_S,
                    CilCode.Br => CilOpCodes.Br_S,
                    CilCode.Brfalse => CilOpCodes.Brfalse_S,
                    CilCode.Brtrue => CilOpCodes.Brtrue_S,
                    CilCode.Bge_Un => CilOpCodes.Bge_Un_S,
                    CilCode.Bgt_Un => CilOpCodes.Bgt_Un_S,
                    CilCode.Ble_Un => CilOpCodes.Ble_Un_S,
                    CilCode.Blt_Un => CilOpCodes.Blt_Un_S,
                    CilCode.Bne_Un => CilOpCodes.Bne_Un_S,
                    CilCode.Leave => CilOpCodes.Leave_S,
                    _ => instruction.OpCode
                };
            }
            else
            {
                code = instruction.OpCode.Code switch
                {
                    CilCode.Beq_S => CilOpCodes.Beq,
                    CilCode.Bge_S => CilOpCodes.Bge,
                    CilCode.Bgt_S => CilOpCodes.Bgt,
                    CilCode.Ble_S => CilOpCodes.Ble,
                    CilCode.Blt_S => CilOpCodes.Blt,
                    CilCode.Br_S => CilOpCodes.Br,
                    CilCode.Brfalse_S => CilOpCodes.Brfalse,
                    CilCode.Brtrue_S => CilOpCodes.Brtrue,
                    CilCode.Bge_Un_S => CilOpCodes.Bge_Un,
                    CilCode.Bgt_Un_S => CilOpCodes.Bgt_Un,
                    CilCode.Ble_Un_S => CilOpCodes.Ble_Un,
                    CilCode.Blt_Un_S => CilOpCodes.Blt_Un,
                    CilCode.Bne_Un_S => CilOpCodes.Bne_Un,
                    CilCode.Leave_S => CilOpCodes.Leave,
                    _ => instruction.OpCode
                };
            }

            if (instruction.OpCode != code)
            {
                instruction.OpCode = code;
                return true;
            }

            return false;
        }

        private static bool TryOptimizeLdc(CilInstruction instruction)
        {
            int value = instruction.GetLdcI4Constant();
            var (code, operand) = CilInstruction.GetLdcI4OpCodeOperand(value);
            
            if (code != instruction.OpCode)
            {
                instruction.OpCode = code;
                instruction.Operand = operand;
                return true;
            }

            return false;
        }

        private bool TryOptimizeVariable(CilInstruction instruction)
        {
            var variable = instruction.GetLocalVariable(Owner.LocalVariables);
            
            CilOpCode code = instruction.OpCode;
            object operand = instruction.Operand;

            if (instruction.IsLdloc())
            {
                (code, operand) = variable.Index switch
                {
                    0 => (CilOpCodes.Ldloc_0, null),
                    1 => (CilOpCodes.Ldloc_1, null),
                    2 => (CilOpCodes.Ldloc_2, null),
                    3 => (CilOpCodes.Ldloc_3, null),
                    {} x when x >= byte.MinValue && x <= byte.MaxValue => (CilOpCodes.Ldloc_S, variable),
                    _ => (CilOpCodes.Ldloc, variable),
                };
            }
            else if (instruction.IsStloc())
            {
                (code, operand) = variable.Index switch
                {
                    0 => (CilOpCodes.Stloc_0, null),
                    1 => (CilOpCodes.Stloc_1, null),
                    2 => (CilOpCodes.Stloc_2, null),
                    3 => (CilOpCodes.Stloc_3, null),
                    {} x when x >= byte.MinValue && x <= byte.MaxValue => (CilOpCodes.Stloc_S, variable),
                    _ => (CilOpCodes.Stloc, variable),
                };
            }

            if (code != instruction.OpCode)
            {
                instruction.OpCode = code;
                instruction.Operand = operand;
                return true;
            }

            return false;
        }

        private bool TryOptimizeArgument(CilInstruction instruction)
        {
            var parameter = instruction.GetParameter(Owner.Owner.Parameters);

            CilOpCode code = instruction.OpCode;
            object operand = instruction.Operand;

            if (instruction.IsLdarg())
            {
                (code, operand) = parameter.MethodSignatureIndex switch
                {
                    0 => (CilOpCodes.Ldarg_0, null),
                    1 => (CilOpCodes.Ldarg_1, null),
                    2 => (CilOpCodes.Ldarg_2, null),
                    3 => (CilOpCodes.Ldarg_3, null),
                    {} x when x >= byte.MinValue && x <= byte.MaxValue => (CilOpCodes.Ldarg_S, parameter),
                    _ => (CilOpCodes.Ldarg, parameter),
                };
            }
            else if (instruction.IsStarg())
            {
                code = parameter.MethodSignatureIndex <= byte.MaxValue
                    ? CilOpCodes.Starg_S 
                    : CilOpCodes.Starg;
            }

            if (code != instruction.OpCode)
            {
                instruction.OpCode = code;
                instruction.Operand = operand;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Represents an enumerator that enumerates through a collection of CIL instructions.
        /// </summary>
        public struct Enumerator : IEnumerator<CilInstruction>
        {
            private List<CilInstruction>.Enumerator _enumerator;

            /// <summary>
            /// Creates a new instance of the <see cref="Enumerator"/> structure.
            /// </summary>
            /// <param name="collection">The collection to enumerate.</param>
            public Enumerator(CilInstructionCollection collection)
            {
                _enumerator = collection._items.GetEnumerator();
            }

            /// <inheritdoc />
            public bool MoveNext() => _enumerator.MoveNext();

            /// <inheritdoc />
            public void Reset() => ((IEnumerator) _enumerator).Reset();

            /// <inheritdoc />
            public CilInstruction Current => _enumerator.Current;

            /// <inheritdoc />
            object IEnumerator.Current => Current;

            /// <inheritdoc />
            public void Dispose() => _enumerator.Dispose();
        } 

    }
}