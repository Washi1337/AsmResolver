using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.NET.Specialized;
namespace TUP.AsmResolver.NET
{
    public struct MetaDataTableGroup
    {
        internal MetaDataTable[] tables;
        internal int bits;

        internal MetaDataTable this[int index]
        {
            get { return tables[index]; }
            set { tables[index] = value; }
        }

        internal MetaDataTableGroup(int tables, int bits)
        {
            this.tables = new MetaDataTable[tables];
            this.bits = bits;
        }

        public bool IsLarge
        {
            get
            {
                return TotalCount > 0xffff;
            }
        }

        public int TotalCount
        {
            get
            {
                int endresult = 0;
                foreach (MetaDataTable table in tables)
                    if (table != null)
                        endresult += table.rowAmount;

                return endresult;
            }
        }

        public bool TryGetMember(int codedIndex, out IMetaDataMember member)
        {
            member = null;
            if (codedIndex == 0)
                return false;

            int tableindex = 0;
            for (int i = tables.Length - 1; i > 0; i--)
                if ((codedIndex & i) == i)
                {
                    tableindex = i;
                    break;
                }

            if (tableindex >= tables.Length)
                return false;

            int rowindex = codedIndex >> bits;

            if (rowindex == 0 || rowindex > tables[tableindex].AmountOfRows)
                return false;

            member = tables[tableindex].Members[rowindex - 1];
            return true;
        }

        public bool TryGetMember<T>(int codedIndex, out T member) where T:IMetaDataMember
        {
            IMetaDataMember uncastedMember;
            if (TryGetMember(codedIndex, out uncastedMember) && uncastedMember is T)
            {
                member = (T)uncastedMember;
                return true;
            }
            member = default(T);
            return false;
        }
        
        public MetaDataMember GetMember(int codedIndex)
        {
            MetaDataMember member;
            if (!TryGetMember(codedIndex, out member))
            {
                if (codedIndex == 0)
                    throw new ArgumentException("Cannot resolve a member from a zero coded index.");
                throw new ArgumentException("Invalid coded index.");
            }
            return member;
        }

        public uint GetCodedIndex(MetaDataMember member)
        {
            if (member == null)
                return 0;

            MetaDataTable table = tables.FirstOrDefault(t => t != null && t.Type == member.TableType);
            if (table == null)
                throw new ArgumentException("Member does not belong to the metadata table group.");

            uint rowIndex = ((uint)(member._metadatatoken - ((uint)table.Type << 24)));
            uint tableIndex = (uint)Array.IndexOf(tables, table);

            return (rowIndex << bits) | tableIndex;
        }
    }
}
