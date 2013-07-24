using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{

    /// <summary>
    /// A class that is able to look up Members or strings by its metadata token.
    /// </summary>
    public class MetaDataTokenResolver
    {
        NETHeader _netheader;

        public MetaDataTokenResolver(NETHeader netheader)
        {
            this._netheader = netheader;
        }

        public object ResolveToken(uint metadataToken)
        {
            byte rowIndex = (byte)(metadataToken >> 0x18);
            if (rowIndex == 0x70)
                return ResolveString(metadataToken);
            else
                return ResolveMember(metadataToken);
        }

        /// <summary>
        /// Resolves a member by its metadata token.
        /// </summary>
        /// <param name="metadataToken">The token of the member to look up.</param>
        /// <returns></returns>
        public MetaDataMember ResolveMember(uint metadataToken)
        {
            if (metadataToken == 0)
                throw new ArgumentException("Cannot resolve a member from a zero metadata token", "metadataToken");

            MetaDataTableType tabletype = (MetaDataTableType)(metadataToken >> 0x18);

            if (!_netheader.TablesHeap.HasTable(tabletype))
                throw new ArgumentException("Table is not present in tables heap.");

            uint rowindex = metadataToken & 0x00FFFFFF;
            return _netheader.TablesHeap.GetTable(tabletype).Members[(int)rowindex - 1];
        }

        /// <summary>
        /// Resolves a string value by its metadata token.
        /// </summary>
        /// <param name="metadataToken">The token of the string value to look up.</param>
        /// <returns></returns>
        public string ResolveString(uint metadataToken)
        {
            uint actualindex = metadataToken & 0x00FFFFFF;
            return _netheader.UserStringsHeap.GetStringByOffset(actualindex);
            
        }


    }
}
