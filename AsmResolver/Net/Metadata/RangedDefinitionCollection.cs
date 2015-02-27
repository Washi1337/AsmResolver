using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Collections.Generic;

namespace AsmResolver.Net.Metadata
{
    public class RangedDefinitionCollection<TMember> : Collection<TMember>
        where TMember : MetadataMember, ICollectionItem
    {
        private readonly object _owner;
        private readonly MetadataTable<TMember> _table;
        private readonly int _start;
        private readonly int _next;

        internal static RangedDefinitionCollection<TMember> Create<TOwner>(MetadataHeader header, TOwner owner, Func<TOwner, int> getListValue)
            where TOwner : MetadataMember
        {
            if (header == null)
                return new RangedDefinitionCollection<TMember>(owner);

            var tableStream = header.GetStream<TableStream>();
            var ownerTable = tableStream.GetTable<TOwner>();
            var itemTable = tableStream.GetTable<TMember>();

            return owner.MetadataToken.Rid == ownerTable.Count
                ? new RangedDefinitionCollection<TMember>(owner, itemTable, getListValue(owner),
                    itemTable.Count + 1)
                : new RangedDefinitionCollection<TMember>(owner, itemTable, getListValue(owner),
                    getListValue(ownerTable[(int)owner.MetadataToken.Rid]));
        }

        internal RangedDefinitionCollection(object owner)
        {
            _owner = owner;
        }

        internal RangedDefinitionCollection(object owner, MetadataTable<TMember> table, int start, int next)
        {
            _owner = owner;
            _table = table;
            _start = start;
            _next = next;
        }

        protected override void Initialize()
        {
            for (int i = _start; i < _next; i++)
            {
                Items.Add(_table[i - 1]);
                _table[i - 1].Owner = _owner;
            }
        }

        private void AssertHasNoOwner(TMember item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            if (item.Owner != null)
                throw new InvalidOperationException("Cannot add member when it is already present in another collection.");
        }

        public override int Count
        {
            get { return IsInitialized ? base.Count : _next - _start; }
        }

        protected override void ClearItems()
        {
            foreach (var item in Items)
                item.Owner = null;
            base.ClearItems();
        }

        protected override void InsertItem(int index, TMember item)
        {
            AssertHasNoOwner(item);
            base.InsertItem(index, item);
            item.Owner = _owner;
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            this[index].Owner = null;
        }

        protected override void SetItem(int index, TMember item)
        {
            AssertHasNoOwner(item);
            base.SetItem(index, item);
            item.Owner = _owner;
        }
    }
}
