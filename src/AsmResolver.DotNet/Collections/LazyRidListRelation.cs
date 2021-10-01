using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Collections
{
    internal class LazyRidListRelation<TAssociationRow>
        where TAssociationRow : struct, IMetadataRow
    {
        public delegate uint GetOwnerRidDelegate(uint rid, TAssociationRow row);
        public delegate MetadataRange GetMemberListDelegate(uint rid);

        private readonly IMetadata _metadata;
        private readonly TableIndex _associationTable;
        private readonly GetOwnerRidDelegate _getOwnerRid;
        private readonly GetMemberListDelegate _getMemberList;
        private readonly object _lock = new();

        private IDictionary<uint, MetadataRange>? _memberLists;
        private IDictionary<uint, uint>? _memberOwners;

        public LazyRidListRelation(
            IMetadata metadata,
            TableIndex associationTable,
            GetOwnerRidDelegate getOwnerRid,
            GetMemberListDelegate getMemberList)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _getOwnerRid = getOwnerRid ?? throw new ArgumentNullException(nameof(getOwnerRid));
            _getMemberList = getMemberList ?? throw new ArgumentNullException(nameof(getMemberList));
            _associationTable = associationTable;
        }

        [MemberNotNull(nameof(_memberLists))]
        [MemberNotNull(nameof(_memberOwners))]
        private void EnsureIsInitialized()
        {
            if (_memberLists is null || _memberOwners is null)
                Initialize();
        }

        [MemberNotNull(nameof(_memberLists))]
        [MemberNotNull(nameof(_memberOwners))]
        private void Initialize()
        {
            lock (_lock)
            {
                if (_memberLists is not null && _memberOwners is not null)
                    return;

                var tablesStream = _metadata.GetStream<TablesStream>();
                var associationTable = tablesStream.GetTable<TAssociationRow>(_associationTable);

                _memberLists = new Dictionary<uint, MetadataRange>();
                _memberOwners = new Dictionary<uint, uint>();

                for (int i = 0; i < associationTable.Count; i++)
                {
                    uint rid = (uint) (i + 1);
                    InitializeMemberList(_getOwnerRid(rid, associationTable[i]), _getMemberList(rid));
                }
            }
        }

        private void InitializeMemberList(uint ownerRid, MetadataRange memberRange)
        {
            _memberLists![ownerRid] = memberRange;
            foreach (var token in memberRange)
                _memberOwners![token.Rid] = ownerRid;
        }

        public MetadataRange GetMemberRange(uint ownerRid)
        {
            EnsureIsInitialized();
            return _memberLists.TryGetValue(ownerRid, out var range)
                ? range
                : MetadataRange.Empty;
        }

        public uint GetMemberOwner(uint memberRid)
        {
            EnsureIsInitialized();
            return _memberOwners.TryGetValue(memberRid, out uint ownerRid)
                ? ownerRid
                : 0;
        }
    }
}
