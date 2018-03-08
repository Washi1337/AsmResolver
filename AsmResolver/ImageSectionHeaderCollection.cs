using System;
using AsmResolver.Collections.Generic;

namespace AsmResolver
{
    public class ImageSectionHeaderCollection : Collection<ImageSectionHeader>
    {
        public ImageSectionHeaderCollection(WindowsAssembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");
            Assembly = assembly;
        }

        public WindowsAssembly Assembly
        {
            get;
            private set;
        }

        private void AssertNoOwner(ImageSectionHeader header)
        {
            if (header.Assembly != null)
                throw new InvalidOperationException("Cannot add a section header to more than one assembly.");
        }

        protected override void InsertItem(int index, ImageSectionHeader item)
        {
            AssertNoOwner(item);
            base.InsertItem(index, item);
            item.Assembly = Assembly;
        }

        protected override void RemoveItem(int index)
        {
            this[index].Assembly = null;
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, ImageSectionHeader item)
        {
            AssertNoOwner(item);
            this[index].Assembly = null;
            base.SetItem(index, item);
            item.Assembly = Assembly;
        }

        protected override void ClearItems()
        {
            foreach (var item in Items)
                item.Assembly = null;
            base.ClearItems();
        }
    }
}
