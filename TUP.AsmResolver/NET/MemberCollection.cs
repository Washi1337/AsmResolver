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
        internal MetaDataMember[] _internalArray;

        public MemberCollection(MetaDataTable table)
        {
            _table = table;
            _internalArray = new MetaDataMember[table.AmountOfRows];
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
                if (_internalArray[index] == null)
                    _internalArray[index] = _table.TablesHeap._tablereader.ReadMember(_table, index + 1);
                return _internalArray[index];
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
            get { return _internalArray.Length; }
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
