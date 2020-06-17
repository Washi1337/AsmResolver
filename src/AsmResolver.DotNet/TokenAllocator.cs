using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Metadata.Tables;
using System;
using System.Collections.Generic;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides a mechanism to reassign metadata tokens
    /// </summary>
    public class TokenAllocator
    {
        private readonly Dictionary<TableIndex, MetadataToken> _nextTokens = new Dictionary<TableIndex, MetadataToken>();
        internal TokenAllocator(ModuleDefinition module)
        {
            if (module is null)
                throw new ArgumentNullException(nameof(module));
            Initialize(module.DotNetDirectory);
        }

        private void Initialize(IDotNetDirectory netDirectory)
        {
            var tableIndexes = Enum.GetValues(typeof(TableIndex));
            if (netDirectory is null)
                InitailizeDefault(tableIndexes);
            else
                InitializeTable(netDirectory, tableIndexes);
        }
        private void InitailizeDefault(Array tableIndexes)
        {
            foreach (TableIndex index in tableIndexes)
                _nextTokens[index] = new MetadataToken(index, 1);
        }
        private void InitializeTable(IDotNetDirectory netDirectory, Array tableIndexes)
        {
            var tableStream = netDirectory.Metadata.GetStream<TablesStream>();
            foreach (TableIndex index in tableIndexes)
            {
                var table = tableStream.GetTable(index);
                var rid = (uint)table.Count + 1;
                _nextTokens[index] = new MetadataToken(index, rid);
            }
        }

        /// <summary>
        /// Obtains next unused <see cref="MetadataToken"/>
        /// </summary>
        /// <param name="index">Type of <see cref="MetadataToken"/></param>
        /// <remarks>This method is pure. That is, it only returns the next available metadata token and does not claim any metadata token</remarks>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when an invalid <see cref="TableIndex"/> is provided</exception>
        /// <returns>The next unused <see cref="MetadataToken"/></returns>
        public MetadataToken GetNextAvailableToken(TableIndex index)
        {
            if (!_nextTokens.ContainsKey(index))
                throw new ArgumentOutOfRangeException(nameof(index));
            return _nextTokens[index];
        }
        /// <summary>
        /// Determines the next metadata token for provided member and asigns it
        /// </summary>
        /// <remarks>This method only succeeds when new or copied memeber is provided</remarks>
        /// <exception cref="ArgumentNullException">Occurs when <paramref name="member"/> is null</exception>
        /// <exception cref="ArgumentException">Occurs when <paramref name="member"/> is already assigned a <see cref="MetadataToken"/></exception>
        /// <param name="member"></param>
        public void AssignNextAvailableToken(IMetadataMember member)
        {
            if (member is null)
                throw new ArgumentNullException(nameof(member));
            if (member.MetadataToken.Rid != 0)
                throw new ArgumentException("Only new members can be assigned a new metadata token");
            var index = member.MetadataToken.Table;
            var token = GetNextAvailableToken(index);
            member.MetadataToken = token;
            var nextToken = new MetadataToken(index, token.Rid + 1);
            _nextTokens[index] = nextToken;
        }
    }
} 
