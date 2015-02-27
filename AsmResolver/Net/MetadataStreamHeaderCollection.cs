using System;
using System.Threading.Tasks;
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
                throw new ArgumentNullException("item");
            if (item.MetaDataHeader != null)
                throw new InvalidOperationException(
                    "Cannot add a stream header to the assembly that has already been added to another assembly.");
        }

        protected override void ClearItems()
        {
            foreach (var item in Items)
                item.MetaDataHeader = null;
            base.ClearItems();
        }

        protected override void InsertItem(int index, MetadataStreamHeader item)
        {
            AssertItemHasNoOwner(item);
            base.InsertItem(index, item);
            item.MetaDataHeader = _owner;
        }

        protected override void RemoveItem(int index)
        {
            Items[index].MetaDataHeader = null;
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, MetadataStreamHeader item)
        {
            AssertItemHasNoOwner(item);
            base.SetItem(index, item);
            item.MetaDataHeader = _owner;
        }
    }
}
