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
        private readonly TableIndex _memberTable;
        private readonly GetOwnerRidDelegate _getOwnerRid;
        private readonly GetMemberListDelegate _getMemberRange;
        private readonly object _lock = new();

        // We use a dictionary here instead of an array, as not all owners might have an association.
        // e.g. not all TypeDefs have a PropertyMap assigned to them.
        private Dictionary<uint, MetadataRange>? _memberRanges;

        // We can use an array instead of a dictionary here since all members will have an owner.
        // _memberOwnerRids[memberRid - 1] = ownerRid.
        private uint[]? _memberOwnerRids;

        public LazyRidListRelation(
            IMetadata metadata,
            TableIndex memberTable,
            TableIndex associationTable,
            GetOwnerRidDelegate getOwnerRid,
            GetMemberListDelegate getMemberList)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _getOwnerRid = getOwnerRid ?? throw new ArgumentNullException(nameof(getOwnerRid));
            _getMemberRange = getMemberList ?? throw new ArgumentNullException(nameof(getMemberList));
            _associationTable = associationTable;
            _memberTable = memberTable;
        }

        [MemberNotNull(nameof(_memberRanges))]
        [MemberNotNull(nameof(_memberOwnerRids))]
        private void EnsureIsInitialized()
        {
            // Note: This is a hot path, thus we don't lock here until we're absolutely sure we need to initialize.
            if (_memberRanges is null || _memberOwnerRids is null)
                Initialize();
        }

        [MemberNotNull(nameof(_memberRanges))]
        [MemberNotNull(nameof(_memberOwnerRids))]
        private void Initialize()
        {
            lock (_lock)
            {
                // Check if some other thread was also initializing it in the mean time.
                if (_memberRanges is not null && _memberOwnerRids is not null)
                    return;

                var tablesStream = _metadata.GetStream<TablesStream>();
                var associationTable = tablesStream.GetTable<TAssociationRow>(_associationTable);
                var memberTable = tablesStream.GetTable(_memberTable);

                // Note: we don't assign the _memberRanges and _memberOwnerRids directly here.
                // This is to prevent a very nasty but subtle race condition (and also a data race) from happening, where
                // EnsureIsInitialized might return prematurely before the lists are fully initialized.
                // See: https://github.com/Washi1337/AsmResolver/issues/299
                var memberRanges = new Dictionary<uint, MetadataRange>();
                uint[] memberOwnerRids = new uint[memberTable.Count];

                for (int i = 0; i < associationTable.Count; i++)
                {
                    uint associationRid = (uint) (i + 1);

                    uint ownerRid = _getOwnerRid(associationRid, associationTable[i]);
                    var memberRange = _getMemberRange(associationRid);

                    memberRanges[ownerRid] = memberRange;
                    foreach (var memberToken in memberRange)
                        memberOwnerRids[memberToken.Rid - 1] = ownerRid;
                }

                // Assign in reverse order. This is again to ensure a data race does not happen with the conditions
                // happening in EnsureIsInitialized.
                _memberOwnerRids = memberOwnerRids;
                _memberRanges = memberRanges;
            }
        }

        public MetadataRange GetMemberRange(uint ownerRid)
        {
            EnsureIsInitialized();
            return _memberRanges.TryGetValue(ownerRid, out var range)
                ? range
                : MetadataRange.Empty;
        }

        public uint GetMemberOwner(uint memberRid)
        {
            EnsureIsInitialized();
            return memberRid - 1 < _memberOwnerRids.Length
                ? _memberOwnerRids[memberRid - 1]
                : 0;
        }
    }
}
