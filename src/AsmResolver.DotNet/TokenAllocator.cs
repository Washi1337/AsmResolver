using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Metadata.Tables;
using System;
using System.Collections.Generic;

namespace AsmResolver.DotNet
{
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

        public MetadataToken GetNextAvailableToken(TableIndex index)
        {
            if (!_nextTokens.ContainsKey(index))
                throw new ArgumentOutOfRangeException(nameof(index));
            return _nextTokens[index];
        }

        public void AssignNextAvailableToken(IMetadataMember member)
        {
            if (member is null)
                throw new ArgumentNullException(nameof(member));
            var index = member.MetadataToken.Table;
            var token = GetNextAvailableToken(index);
            member.MetadataToken = token;
            var nextToken = new MetadataToken(index, token.Rid + 1);
            _nextTokens[index] = nextToken;
        }
    }
} 
