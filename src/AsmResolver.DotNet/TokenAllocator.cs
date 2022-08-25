using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides a mechanism to assign metadata tokens
    /// </summary>
    public sealed class TokenAllocator
    {
        private readonly TokenBucket[] _buckets = new TokenBucket[(int) TableIndex.Max];

        internal TokenAllocator(ModuleDefinition module)
        {
            if (module is null)
                throw new ArgumentNullException(nameof(module));
            Initialize(module.DotNetDirectory);
        }

        private void Initialize(IDotNetDirectory? netDirectory)
        {
            if (netDirectory is null)
                InitializeDefault();
            else
                InitializeTable(netDirectory);
        }

        private void InitializeDefault()
        {
            for (TableIndex index = 0; index < TableIndex.Max; index++)
                _buckets[(int) index] = new TokenBucket(new MetadataToken(index, 1));
        }

        private void InitializeTable(IDotNetDirectory netDirectory)
        {
            var tableStream = netDirectory.Metadata!.GetStream<TablesStream>();
            for (TableIndex index = 0; index < TableIndex.Max; index++)
            {
                if (!index.IsValidTableIndex())
                    continue;

                var table = tableStream.GetTable(index);
                _buckets[(int) index] = new TokenBucket(new MetadataToken(index, (uint) table.Count + 1));
            }
        }

        /// <summary>
        /// Obtains the next unused <see cref="MetadataToken"/> for the provided table.
        /// </summary>
        /// <param name="index">Type of <see cref="MetadataToken"/></param>
        /// <remarks>
        /// This method is pure. That is, it only returns the next available metadata token and does not claim
        /// any metadata token.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when an invalid <see cref="TableIndex"/> is provided.
        /// </exception>
        /// <returns>The next unused <see cref="MetadataToken"/>.</returns>
        public MetadataToken GetNextAvailableToken(TableIndex index)
        {
            return _buckets[(int) index].GetNextAvailableToken();
        }

        /// <summary>
        /// Determines the next metadata token for provided member and assigns it.
        /// </summary>
        /// <remarks>This method only succeeds when new or copied member is provided</remarks>
        /// <exception cref="ArgumentNullException">Occurs when <paramref name="member"/> is null</exception>
        /// <exception cref="ArgumentException">
        /// Occurs when <paramref name="member"/> is already assigned a <see cref="MetadataToken"/>
        /// </exception>
        /// <param name="member">The member to assign a new metadata token.</param>
        public void AssignNextAvailableToken(MetadataMember member)
        {
            if (member is null)
                throw new ArgumentNullException(nameof(member));
            if (member.MetadataToken.Rid != 0)
                throw new ArgumentException("Only new members can be assigned a new metadata token");

            var index = member.MetadataToken.Table;
            member.MetadataToken = GetNextAvailableToken(index);

            _buckets[(int) index].AssignedMembers.Add(member);
        }

        /// <summary>
        /// Obtains the members that were manually assigned a new metadata token using this token allocator.
        /// </summary>
        /// <param name="table">The table for which to get the assignees from.</param>
        /// <returns>The assignees.</returns>
        public IEnumerable<IMetadataMember> GetAssignees(TableIndex table) => _buckets[(int) table].AssignedMembers;

        private readonly struct TokenBucket
        {
            public TokenBucket(MetadataToken baseToken)
            {
                BaseToken = baseToken;
                AssignedMembers = new List<IMetadataMember>();
            }

            public MetadataToken BaseToken
            {
                get;
            }

            public List<IMetadataMember> AssignedMembers
            {
                get;
            }

            public MetadataToken GetNextAvailableToken() =>
                new(BaseToken.Table, (uint) (BaseToken.Rid + AssignedMembers.Count));
        }
    }
}
