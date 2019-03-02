using System;
using AsmResolver.Collections.Generic;

namespace AsmResolver.Net
{
    public class MetadataStreamHeaderCollection : Collection<MetadataStreamHeader>
    {
        private readonly MetadataHeader _owner;

        internal MetadataStreamHeaderCollection(MetadataHeader owner)
        {
            _owner = owner;
        }
        
        private void AssertItemHasNoOwner(MetadataStreamHeader item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (item.MetadataHeader != null)
                throw new InvalidOperationException(
                    "Cannot add a stream header to the assembly that has already been added to another assembly.");
        }

        private void AssertIsNotLocked()
        {            
            if (_owner.IsLocked)
                throw new MetadataLockedException("edit stream headers");
        }

        protected override void ClearItems()
        {
            AssertIsNotLocked();
            foreach (var item in Items)
                item.MetadataHeader = null;
            base.ClearItems();
        }

        protected override void InsertItem(int index, MetadataStreamHeader item)
        {
            AssertIsNotLocked();
            AssertItemHasNoOwner(item);
            base.InsertItem(index, item);
            item.MetadataHeader = _owner;
        }

        protected override void RemoveItem(int index)
        {
            AssertIsNotLocked();
            Items[index].MetadataHeader = null;
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, MetadataStreamHeader item)
        {
            AssertIsNotLocked();
            AssertItemHasNoOwner(item);
            base.SetItem(index, item);
            item.MetadataHeader = _owner;
        }
    }
}
