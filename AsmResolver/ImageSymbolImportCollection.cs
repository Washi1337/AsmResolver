using System;
using AsmResolver.Collections.Generic;

namespace AsmResolver
{
    public class ImageSymbolImportCollection : Collection<ImageSymbolImport>
    {
        private readonly ImageModuleImport _owner;

        public ImageSymbolImportCollection(ImageModuleImport owner)
        {
            _owner = owner;
        }

        protected override void ClearItems()
        {
            foreach (var item in Items)
                item.Module = null;
            base.ClearItems();
        }

        protected override void InsertItem(int index, ImageSymbolImport item)
        {
            AssertItemHasNoOwner(item);
            base.InsertItem(index, item);
            item.Module = _owner;
        }

        protected override void RemoveItem(int index)
        {
            Items[index].Module = null;
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, ImageSymbolImport item)
        {
            AssertItemHasNoOwner(item);
            base.SetItem(index, item);
            item.Module = _owner;
        }
        
        private static void AssertItemHasNoOwner(ImageSymbolImport item)
        {
            if (item.Module != null)
                throw new InvalidOperationException(
                    "Cannot add a symbol to the module that has already been added to another module.");
        }
    }
}
