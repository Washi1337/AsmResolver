using System;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts.Collections
{
    public class TableMemberCollection<TOwner, TMember> : MemberCollection<TOwner, TMember>
        where TOwner : class, IMetadataMember
        where TMember : IMetadataMember
    {
        private readonly MetadataTable _table;
        private readonly Func<TMember, TOwner> _getOwner;
        private readonly Action<TMember, TOwner> _setOwner;

        public TableMemberCollection(TOwner owner, MetadataTokenType table, Func<TMember, TOwner> getOwner, Action<TMember, TOwner> setOwner)
            : this(owner, owner.Image.Header.GetStream<TableStream>().GetTable(table), getOwner, setOwner)
        {
        }

        public TableMemberCollection(TOwner owner, MetadataTable table, Func<TMember, TOwner> getOwner, Action<TMember, TOwner> setOwner)
            : base(owner)
        {
            if (getOwner == null)
                throw new ArgumentNullException("getOwner");
            if (setOwner == null)
                throw new ArgumentNullException("setOwner");
            _table = table;
            _getOwner = getOwner;
            _setOwner = setOwner;
        }
        
        public override int Count
        {
            get { return IsInitialized ? base.Count : _table.Count; }
        }
        
        protected override TOwner GetOwner(TMember item)
        {
            return _getOwner != null ? _getOwner(item) : null;
        }

        protected override void SetOwner(TMember item, TOwner owner)
        {
            if (_setOwner != null)
                _setOwner(item, owner);
        }

        protected override void Initialize()
        {
            base.Initialize();
            
            // TODO: remove and lazy initialize members upon access this[index] instead.
            
            foreach (var row in _table)
            {
                var member = (TMember) _table.GetMemberFromRow(Owner.Image, row);
                SetOwner(member, Owner);
                Items.Add(member);
            }
        }
        
    }
}
