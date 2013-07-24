using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUP.AsmResolver.NET.Specialized;

namespace TUP.AsmResolver.NET
{

    public abstract class MemberRange
    {
        public MetaDataTable TargetTable { get; private set; }
        public int Start { get; private set; }
        public int Length { get; private set; }

        public static MemberRange<T> CreateRange<T>(MetaDataMember member, int mdrowIndex, MetaDataTable targetTable) where T : MetaDataMember
        {
            MemberRange<T> range = new MemberRange<T>();
            range.Start = Convert.ToInt32(member.MetaDataRow.Parts[mdrowIndex]) - 1;

            if (targetTable == null)
            {
                range.Length = 0;
                return range;
            }

            range.TargetTable = targetTable;

            int memberIndex = (int)(member._metadatatoken | (0xFF << 24)) - (0xFF << 24);
            if (member.Table != null)
            {
                if (memberIndex == member.Table.AmountOfRows)
                    range.Length = targetTable.AmountOfRows - range.Start;
                else
                {
                    int nextIndex = Convert.ToInt32(member.Table.Members[memberIndex].MetaDataRow.Parts[mdrowIndex]) - 1;
                    range.Length = nextIndex - range.Start;
                }
            }

            if (range.Length > targetTable.AmountOfRows - range.Start)
                range.Length = 0;

            return range;

        }

        public override string ToString()
        {
            return string.Format("{{Table={0}, Start={1}, Length={2}}}", TargetTable.Type, Start, Length);
        }
    }

    public class MemberRange<T> : MemberRange, ICacheProvider  where T : MetaDataMember
    {
        T[] members;

        public T[] Members
        {
            get
            {
                if (this.members == null)
                {
                    var members = new T[Length];
                    for (int i = 0; i < Length; i++)
                        members[i] = (T)TargetTable.Members[Start + i];

                    this.members = members;
                }
                return this.members;
            }
        }

        public void ClearCache()
        {
            members = null;
        }

        public void LoadCache()
        {
            members = Members;
        }
    }
}
