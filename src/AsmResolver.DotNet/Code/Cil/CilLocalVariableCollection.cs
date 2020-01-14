using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.DotNet.Code.Cil
{
    /// <summary>
    /// Represents a collection of local variables stored in a CIL method body.
    /// </summary>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class CilLocalVariableCollection : Collection<CilLocalVariable>
    {
        private static void AssertHasNoOwner(CilLocalVariable item)
        {
            if (item.Index != -1)
                throw new ArgumentException("Variable is already added to another list of variables.");
        }

        /// <inheritdoc />
        protected override void ClearItems()
        {
            lock (Items)
            {
                foreach (var item in Items)
                    item.Index = -1;
                base.ClearItems();
            }
        }

        /// <inheritdoc />
        protected override void InsertItem(int index, CilLocalVariable item)
        {
            lock (Items)
            {
                AssertHasNoOwner(item);
                for (int i = index; i < Count; i++)
                    Items[i].Index++;
                item.Index = index;
                base.InsertItem(index, item);
            }
        }

        /// <inheritdoc />
        protected override void RemoveItem(int index)
        {
            lock (Items)
            {
                for (int i = index; i < Count; i++)
                    Items[i].Index--;
                Items[index].Index = -1;
                base.RemoveItem(index);
            }
        }

        /// <inheritdoc />
        protected override void SetItem(int index, CilLocalVariable item)
        {
            lock (Items)
            {
                AssertHasNoOwner(item);
                Items[index].Index = -1;
                base.SetItem(index, item);
                item.Index = index;
            }
        }
    }
}