using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.NET.Specialized;
namespace TUP.AsmResolver.NET
{
    internal struct MetaDataTableGroup
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

        internal int TotalCount
        {
            get
            {
                int endresult = 0;
                foreach (MetaDataTable table in tables)
                    endresult += table.rowAmount;

                return endresult;
            }
        }

        internal bool TryGetMember(int token, out MetaDataMember member)
        {
            try
            {
                member = GetMember(token);
                return true;
            }
            catch { member = null; return false; }
        }

        internal MetaDataMember GetMember(int token)
        {
            if (token == 0)
                throw new ArgumentException("Cannot resolve a member from a zero metadata token", "token");

            int tableindex = 0;
            for (int i = tables.Length-1; i > 0 ;i--)
                if ((token & i) == i)
                {
                    tableindex = i;
                    break;
                }

            int rowindex = token >> bits;

            if (rowindex == 0)
                throw new ArgumentException("Cannot resolve a member from a zero metadata token", "token");

            return tables[tableindex].members[rowindex - 1];
        }
    }
}
