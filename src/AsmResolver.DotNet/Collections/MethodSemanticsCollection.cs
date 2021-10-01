using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AsmResolver.Collections;

namespace AsmResolver.DotNet.Collections
{
    /// <summary>
    /// Provides an implementation of a list of method semantics that are associated to a property or event.
    /// </summary>
    public class MethodSemanticsCollection : OwnedCollection< IHasSemantics, MethodSemantics>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="MethodSemanticsCollection"/> class.
        /// </summary>
        /// <param name="owner">The owner of the collection.</param>
        public MethodSemanticsCollection(IHasSemantics owner)
            : base(owner)
        {
        }

        internal bool ValidateMembership
        {
            get;
            set;
        } = true;

        private void AssertSemanticsValidity([NotNull] MethodSemantics item)
        {
            if (!ValidateMembership)
                return;

            if (item.Method is null)
                throw new ArgumentException("Method semantics is not assigned to a method.");

            if (item.Method.Semantics is { })
                throw new ArgumentException($"Method {item.Method} is already assigned semantics.");
        }

        /// <inheritdoc />
        protected override void OnInsertItem(int index, MethodSemantics item)
        {
            AssertSemanticsValidity(item);
            base.OnInsertItem(index, item);
            item.Method!.Semantics = item;
        }

        /// <inheritdoc />
        protected override void OnSetItem(int index, MethodSemantics item)
        {
            AssertSemanticsValidity(item);
            Items[index].Method!.Semantics = null;
            base.OnSetItem(index, item);
            item.Method!.Semantics = item;
        }

        /// <inheritdoc />
        protected override void OnInsertRange(int index, IEnumerable<MethodSemantics> items)
        {
            var list = items as MethodSemantics[] ?? items.ToArray();
            foreach (var item in list)
                AssertSemanticsValidity(item);
            base.OnInsertRange(index, list);
            foreach (var item in list)
                item.Method!.Semantics = item;
        }

        /// <inheritdoc />
        protected override void OnRemoveItem(int index)
        {
            Items[index].Method!.Semantics = null;
            base.OnRemoveItem(index);
        }

        /// <inheritdoc />
        protected override void OnClearItems()
        {
            foreach (var item in Items)
                item.Method!.Semantics = null;
            base.OnClearItems();
        }
    }
}
