using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUP.AsmResolver.NET.Specialized;

namespace TUP.AsmResolver.NET
{
    public class MemberCollection : IEnumerable<MetaDataMember>, IList<MetaDataMember>
    {
        private MetaDataTable _table;
        private MetaDataMember[] _members;

        public MemberCollection(MetaDataTable table)
        {
            _table = table;
            _members = new MetaDataMember[table.AmountOfRows];
        }

        public int IndexOf(MetaDataMember item)
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, MetaDataMember item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public MetaDataMember this[int index]
        {
            get
            {
                if (_members[index] == null)
                    _members[index] = _table.TablesHeap._tablereader.ReadMember(_table, index + 1);
                return _members[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public void Add(MetaDataMember item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(MetaDataMember item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(MetaDataMember[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public int Count
        {
            get { return _members.Length; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(MetaDataMember item)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<MetaDataMember> GetEnumerator()
        {
            for (int i = 0; i < this.Count; i++)
                yield return this[i];
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
