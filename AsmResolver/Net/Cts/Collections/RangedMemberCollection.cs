using System;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts.Collections
{
    public class RangedMemberCollection<TOwner, TMember> : DelegatedMemberCollection<TOwner, TMember>
        where TOwner : class, IMetadataMember
        where TMember : IMetadataMember
    {
        private readonly MetadataTable _itemTable;
        private readonly int _start;
        private readonly int _next;

        public RangedMemberCollection(TOwner owner, MetadataTokenType itemTable, int listColumnIndex, Func<TMember, TOwner> getOwner, Action<TMember, TOwner> setOwner)
            : this(owner, owner.Image.Header.GetStream<TableStream>().GetTable(itemTable), listColumnIndex, getOwner, setOwner)
        {
        }

        public RangedMemberCollection(TOwner owner, MetadataTable itemTable, int listColumnIndex, Func<TMember, TOwner> getOwner, Action<TMember, TOwner> setOwner)
            : base(owner, getOwner, setOwner)
        {
            if (itemTable == null)
                throw new ArgumentNullException("itemTable");
            if (getOwner == null)
                throw new ArgumentNullException("getOwner");
            if (setOwner == null)
                throw new ArgumentNullException("setOwner");

            _itemTable = itemTable;

            var ownerTable = owner.Image.Header.GetStream<TableStream>().GetTable(owner.MetadataToken.TokenType);
            var ownerRow = ownerTable.GetRow((int)(owner.MetadataToken.Rid - 1));

            _start = Math.Min(Convert.ToInt32(ownerRow.GetAllColumns()[listColumnIndex]), _itemTable.Count + 1);

            if (owner.MetadataToken.Rid == ownerTable.Count)
            {
                _next = itemTable.Count + 1;
            }
            else
            {
                var nextRow = ownerTable.GetRow((int) owner.MetadataToken.Rid);
                _next = Math.Min(Convert.ToInt32(nextRow.GetAllColumns()[listColumnIndex]), _itemTable.Count + 1);
            }
        }

        public override int Count
        {
            get { return IsInitialized ? base.Count : _next - _start; }
        }

        protected override void Initialize()
        {
            if (_start == 0 || _start > _itemTable.Count)
                return;

            for (int i = _start; i < _next; i++)
            {
                var member = (TMember) _itemTable.GetMemberFromRow(Owner.Image, _itemTable.GetRow(i - 1));
                Items.Add(member);
                SetOwner(member, Owner);
            }
        }
    }
}
