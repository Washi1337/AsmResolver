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
        public void Add(CilInstruction item)
        {
            _items.Add(item);
        }

        /// <summary>
        /// Adds a collection of CIL instructions to the end of the list.
        /// </summary>
        /// <param name="items">The instructions to add.</param>
        public void AddRange(IEnumerable<CilInstruction> items)
        {
            _items.AddRange(items);
        }

        /// <summary>
        /// Inserts a collection of CIL instructions at the provided index.
        /// </summary>
        /// <param name="index">The index to insert the instructions into.</param>
        /// <param name="items">The instructions to insert.</param>
        public void InsertRange(int index, IEnumerable<CilInstruction> items)
        {
            _items.InsertRange(index, items);
        }

        /// <inheritdoc />
        public void Clear()
        {
            _items.Clear();
        }

        /// <inheritdoc />
        public bool Contains(CilInstruction item)
        {
            return _items.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(CilInstruction[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(CilInstruction item)
        {
            return _items.Remove(item);
        }

        /// <inheritdoc />
        public int IndexOf(CilInstruction item)
        {
            return _items.IndexOf(item);
        }

        /// <inheritdoc />
        public void Insert(int index, CilInstruction item)
        {
            _items.Insert(index, item);
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }

        /// <inheritdoc />
        public IEnumerator<CilInstruction> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _items).GetEnumerator();
        }

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

    }
}