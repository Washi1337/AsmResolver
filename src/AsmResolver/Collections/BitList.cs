using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.Collections
{
    /// <summary>
    /// Represents a bit vector that can be resized dynamically.
    /// </summary>
    public class BitList : IList<bool>, IReadOnlyList<bool>
    {
        private const int WordSize = sizeof(int) * 8;
        private uint[] _words;
        private int _version;

        /// <summary>
        /// Creates a new bit list.
        /// </summary>
        public BitList()
        {
            _words = new uint[1];
        }

        /// <summary>
        /// Creates a new bit list.
        /// </summary>
        /// <param name="capacity">The initial number of bits that the buffer should at least be able to store.</param>
        public BitList(int capacity)
        {
            _words = new uint[((uint) capacity).Align(WordSize)];
        }

        /// <inheritdoc cref="ICollection{T}.Count" />
        public int Count
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc cref="IList{T}.Item" />
        public bool this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException();

                (int wordIndex, int bitIndex) = SplitWordBitIndex(index);
                return (_words[wordIndex] >> bitIndex & 1) != 0;
            }
            set
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException();

                (int wordIndex, int bitIndex) = SplitWordBitIndex(index);
                _words[wordIndex] = (_words[wordIndex] & ~(1u << bitIndex)) | (value ? 1u << bitIndex : 0u);
                _version++;
            }
        }

        private static (int wordIndex, int bitIndex) SplitWordBitIndex(int index)
        {
            int wordIndex = Math.DivRem(index, WordSize, out int offset);
            return (wordIndex, offset);
        }

        /// <inheritdoc />
        public void Add(bool item)
        {
            EnsureCapacity(Count + 1);
            Count++;
            this[Count - 1] = item;
            _version++;
        }

        /// <inheritdoc />
        public void Clear() => Count = 0;

        /// <inheritdoc />
        public bool Contains(bool item) => IndexOf(item) != -1;

        /// <inheritdoc />
        public void CopyTo(bool[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; i++)
                array[arrayIndex + i] = this[i];
        }

        /// <inheritdoc />
        public bool Remove(bool item)
        {
            int index = IndexOf(item);
            if (index == -1)
                return false;

            RemoveAt(index);
            return true;
        }

        /// <inheritdoc />
        public int IndexOf(bool item)
        {
            for (int i = 0; i < Count; i++)
            {
                (int wordIndex, int bitIndex) = SplitWordBitIndex(i);
                if ((_words[wordIndex] >> bitIndex & 1) != 0 == item)
                    return i;
            }

            return -1;
        }

        /// <inheritdoc />
        public void Insert(int index, bool item)
        {
            if (index < 0 || index > Count)
                throw new IndexOutOfRangeException();

            EnsureCapacity(Count++);
            (int wordIndex, int bitIndex) = SplitWordBitIndex(index);

            uint carry = _words[wordIndex] & (1u << (WordSize - 1));

            // Insert bit into current word.
            uint lowerMask = (1u << bitIndex) - 1;
            uint upperMask = ~lowerMask;
            _words[wordIndex] = (_words[wordIndex] & upperMask) << 1 // Shift left-side of the bit index by one
                                | (item ? 1u << bitIndex : 0u)       // Insert bit.
                                | (_words[wordIndex] & lowerMask);   // Keep right-side of the bit.

            for (int i = wordIndex + 1; i < _words.Length; i++)
            {
                uint nextCarry = _words[i] & (1u << (WordSize - 1));
                _words[i] = (_words[i] << 1) | (carry >> (WordSize - 1));
                carry = nextCarry;
            }

            _version++;
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            Count--;
            (int wordIndex, int bitIndex) = SplitWordBitIndex(index);

            // Note we check both word count and actual bit count. Words in the buffer might contain garbage data for
            // every bit index i >= Count. Also, there might be exactly enough words allocated for Count bits, i.e.
            // there might not be a "next" word.
            uint borrow = wordIndex + 1 < _words.Length && ((uint) index).Align(WordSize) < Count
                ? _words[wordIndex + 1] & 1
                : 0;

            uint lowerMask = (1u << bitIndex) - 1;
            uint upperMask = ~((1u << (bitIndex + 1)) - 1);
            _words[wordIndex] = (_words[wordIndex] & upperMask) >> 1 // Shift left-side of the bit index by one
                                | (_words[wordIndex] & lowerMask)    // Keep right-side of the bit.
                                | borrow << (WordSize - 1);          // Copy first bit of next word into last bit of current.

            for (int i = wordIndex + 1; i < _words.Length; i++)
            {
                uint nextBorrow = i + 1 < _words.Length && ((uint) index).Align(WordSize) < Count
                    ? _words[i + 1] & 1
                    : 0;

                _words[i] = (_words[i] >> 1) | (borrow << (WordSize - 1));
                borrow = nextBorrow;
            }

            _version++;
        }

        /// <summary>
        /// Ensures the provided number of bits can be stored in the bit list.
        /// </summary>
        /// <param name="capacity">The number of bits to store in the list.</param>
        public void EnsureCapacity(int capacity)
        {
            if (capacity < WordSize * _words.Length)
                return;

            int newWordCount = (int) (((uint) capacity).Align(WordSize) / 8);
            Array.Resize(ref _words, newWordCount);
        }

        /// <summary>
        /// Returns an enumerator for all bits in the bit vector.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public Enumerator GetEnumerator() => new(this);

        /// <inheritdoc />
        IEnumerator<bool> IEnumerable<bool>.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Represents an enumerator that iterates over all bits in a bit list.
        /// </summary>
        public struct Enumerator : IEnumerator<bool>
        {
            private readonly BitList _list;
            private readonly int _version;
            private int _index = -1;

            /// <summary>
            /// Creates a new bit enumerator.
            /// </summary>
            /// <param name="list">The list to enumerate.</param>
            public Enumerator(BitList list)
            {
                _version = list._version;
                _list = list;
            }

            /// <inheritdoc />
            public bool MoveNext()
            {
                if (_version != _list._version)
                    throw new InvalidOperationException("Collection was modified.");

                if (_index >= _list.Count)
                    return false;

                _index++;
                return true;
            }

            /// <inheritdoc />
            public void Reset() => _index = -1;

            /// <inheritdoc />
            public bool Current => _list[_index];

            /// <inheritdoc />
            object IEnumerator.Current => Current;

            /// <inheritdoc />
            public void Dispose()
            {
            }
        }
    }
}
