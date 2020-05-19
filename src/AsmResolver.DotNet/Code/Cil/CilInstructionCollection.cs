using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        /// <inheritdoc />
        public int IndexOf(CilInstruction item) => _items.IndexOf(item);

        /// <inheritdoc />
        public void Insert(int index, CilInstruction item) => _items.Insert(index, item);

        /// <inheritdoc />
        public void RemoveAt(int index) => _items.RemoveAt(index);

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