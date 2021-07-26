using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AsmResolver.Collections
{
    /// <summary>
    /// Represents an indexed collection where each element is owned by some object, and prevents the element from being
    /// added to any other instance of the collection.
    /// </summary>
    /// <typeparam name="TOwner">The type of the owner object.</typeparam>
    /// <typeparam name="TItem">The type of elements to store.</typeparam>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class OwnedCollection<TOwner, TItem> : LazyList<TItem>
        where TItem : class, IOwnedCollectionElement<TOwner>
        where TOwner : class
    {
        /// <summary>
        /// Creates a new empty collection that is owned by an object.
        /// </summary>
        /// <param name="owner">The owner of the collection.</param>
        public OwnedCollection(TOwner owner)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        /// <summary>
        /// Gets the owner of the collection.
        /// </summary>
        public TOwner Owner
        {
            get;
        }

        /// <summary>
        /// Verifies that the provided item is not null and not added to another list.
        /// </summary>
        /// <param name="item">The item to verify.</param>
        /// <exception cref="ArgumentNullException">Occurs when the item is null.</exception>
        /// <exception cref="ArgumentException">Occurs when the item is already owned by another collection.</exception>
        protected void AssertNotNullAndHasNoOwner(TItem? item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));
            if (item.Owner != null && item.Owner != Owner)
                throw new ArgumentException("Item is already added to another collection.");
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
        }

        /// <inheritdoc />
        protected override void OnSetItem(int index, TItem item)
        {
            AssertNotNullAndHasNoOwner(item);
            item.Owner = Owner;
            Items[index].Owner = null;
            base.OnSetItem(index, item);
        }

        /// <inheritdoc />
        protected override void OnInsertItem(int index, TItem item)
        {
            AssertNotNullAndHasNoOwner(item);
            item.Owner = Owner;
            base.OnInsertItem(index, item);
        }

        /// <inheritdoc />
        protected override void OnInsertRange(int index, IEnumerable<TItem> items)
        {
            var elements = items.ToList();
            foreach (var item in elements)
                AssertNotNullAndHasNoOwner(item);
            base.OnInsertRange(index, elements);
            foreach (var item in elements)
                item.Owner = Owner;
        }

        /// <inheritdoc />
        protected override void OnRemoveItem(int index)
        {
            var item = this[index];
            item.Owner = null;
            base.OnRemoveItem(index);
        }

        /// <inheritdoc />
        protected override void OnClearItems()
        {
            foreach (var item in Items)
                item.Owner = null;
            base.OnClearItems();
        }

    }
}
