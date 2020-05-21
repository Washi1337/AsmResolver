using System;
using System.Collections.ObjectModel;

namespace AsmResolver.PE.File
{
    /// <summary>
    /// Represents a collection of sections stored in a portable executable file.
    /// </summary>
    public class PESectionCollection : Collection<PESection>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PESectionCollection"/> class.
        /// </summary>
        /// <param name="owner">The owner of the sections.</param>
        public PESectionCollection(PEFile owner)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }
        
        /// <summary>
        /// Gets the PE file containing the sections.
        /// </summary>
        public PEFile Owner
        {
            get;
        }
        
        private void AssertHasNoOwner(PESection section)
        {
            if (section.ContainingFile != null)
                throw new ArgumentException("Section was already added to another portable executable file.");
        }

        /// <inheritdoc />
        protected override void ClearItems()
        {
            foreach (var section in Items)
                section.ContainingFile = null;
            base.ClearItems();
        }

        /// <inheritdoc />
        protected override void InsertItem(int index, PESection item)
        {
            AssertHasNoOwner(item);
            base.InsertItem(index, item);
            item.ContainingFile = Owner;
        }

        /// <inheritdoc />
        protected override void SetItem(int index, PESection item)
        {
            AssertHasNoOwner(item);
            RemoveItem(index);
            InsertItem(index, item);
        }

        /// <inheritdoc />
        protected override void RemoveItem(int index)
        {
            Items[index].ContainingFile = null;
            base.RemoveItem(index);
        }
    }
}