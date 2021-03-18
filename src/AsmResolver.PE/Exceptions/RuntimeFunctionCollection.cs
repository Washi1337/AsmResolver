using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Collections;

namespace AsmResolver.PE.Exceptions
{
    /// <summary>
    /// Represents a strongly-typed collection of runtime functions in an exceptions data directory.
    /// </summary>
    /// <typeparam name="TFunction">The type of runtime functions to store.</typeparam>
    public class RuntimeFunctionCollection<TFunction> : LazyList<TFunction>, IList<IRuntimeFunction>
        where TFunction : class, IRuntimeFunction
    {
        /// <inheritdoc />
        IRuntimeFunction IList<IRuntimeFunction>.this[int index]
        {
            get => Items[index];
            set
            {
                if (value is TFunction function)
                    Add(function);
                else
                    ThrowInvalidEntry();
            }
        }

        private static void ThrowInvalidEntry()
        {
            throw new ArgumentException("Function entry is not supported by this architecture.");
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
        }

        /// <inheritdoc />
        void ICollection<IRuntimeFunction>.Add(IRuntimeFunction item)
        {
            if (item is TFunction function)
                Add(function);
            else
                ThrowInvalidEntry();
        }

        /// <inheritdoc />
        bool ICollection<IRuntimeFunction>.Contains(IRuntimeFunction item)
        {
            return Items.Contains(item);
        }

        /// <inheritdoc />
        void ICollection<IRuntimeFunction>.CopyTo(IRuntimeFunction[] array, int arrayIndex)
        {
            if (Count > array.Length - arrayIndex)
                throw new ArgumentException("Insufficient space.");

            for (int i = 0; i < Count; i++)
                array[arrayIndex + i] = Items[i];
        }

        /// <inheritdoc />
        bool ICollection<IRuntimeFunction>.Remove(IRuntimeFunction item)
        {
            return item is TFunction function && Items.Remove(function);
        }

        /// <inheritdoc />
        int IList<IRuntimeFunction>.IndexOf(IRuntimeFunction item)
        {
            return item is TFunction function
                ? Items.IndexOf(function)
                : -1;
        }

        /// <inheritdoc />
        void IList<IRuntimeFunction>.Insert(int index, IRuntimeFunction item)
        {
            if (item is TFunction function)
                Insert(index, function);
            else
                ThrowInvalidEntry();
        }

        /// <inheritdoc />
        IEnumerator<IRuntimeFunction> IEnumerable<IRuntimeFunction>.GetEnumerator() => GetEnumerator();
    }
}
