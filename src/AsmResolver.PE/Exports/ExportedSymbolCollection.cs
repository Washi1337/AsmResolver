using System.Collections.Generic;
using System.Linq;
using AsmResolver.Collections;

namespace AsmResolver.PE.Exports
{
    /// <summary>
    /// Represents a collection of exported symbols within an export data directory.
    /// </summary>
    public class ExportedSymbolCollection : OwnedCollection<IExportDirectory, ExportedSymbol>
    {
        /// <summary>
        /// Creates a new instance of the exported symbol collection.
        /// </summary>
        /// <param name="owner"></param>
        public ExportedSymbolCollection(IExportDirectory owner)
            : base(owner)
        {
        }

        private void UpdateIndicesAfterIndex(int index)
        {
            for (int i = index; i < Count; i++)
                Items[i].Index = i;
        }

        /// <inheritdoc />
        protected override void OnClearItems()
        {
            lock (Items)
            {
                foreach (var item in Items)
                    item.Index = -1;
                base.OnClearItems();
            }
        }

        /// <inheritdoc />
        protected override void OnInsertItem(int index, ExportedSymbol item)
        {
            lock (Items)
            {
                base.OnInsertItem(index, item);
                UpdateIndicesAfterIndex(index);
            }
        }

        /// <inheritdoc />
        protected override void OnInsertRange(int index, IEnumerable<ExportedSymbol> items)
        {
            lock (Items)
            {
                var exportedSymbols = items as ExportedSymbol[] ?? items.ToArray();
                base.OnInsertRange(index, exportedSymbols);
                for (int i = index; i < Count; i++)
                    Items[i].Index = i;
            }
        }

        /// <inheritdoc />
        protected override void OnRemoveItem(int index)
        {
            lock (Items)
            {
                Items[index].Index = -1;
                base.OnRemoveItem(index);
                UpdateIndicesAfterIndex(index);
            }
        }

        /// <inheritdoc />
        protected override void OnSetItem(int index, ExportedSymbol item)
        {
            lock (Items)
            {
                AssertNotNullAndHasNoOwner(item);
                Items[index].Index = -1;
                base.OnSetItem(index, item);
                item.Index = index;
            }
        }
    }
}