using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Builder;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public abstract class MetadataTable : FileSegment
    {
        public TableStream TableStream
        {
            get;
            internal set;
        }

        public MetadataHeader Header
        {
            get { return TableStream != null ? TableStream.StreamHeader.MetaDataHeader : null; }
        }

        public abstract MetadataTokenType TokenType
        {
            get;
        }

        public IndexSize IndexSize
        {
            get { return Count > ushort.MaxValue ? IndexSize.Long : IndexSize.Short; }
        }

        public abstract int Count
        {
            get;
        }

        public abstract uint GetElementByteCount();

        public bool TryGetMember(int index, out MetadataMember member)
        {
            if (index >= 0 && index < Count)
            {
                member = GetMember(index);
                return true;
            }
            member = null;
            return false;
        }

        public abstract MetadataMember GetMember(int index);

        internal abstract void SetMemberCount(uint capacity);

        internal abstract void SetReadingContext(ReadingContext readingContext);

        public override uint GetPhysicalLength()
        {
            return (uint)(GetElementByteCount() * Count);
        }

        public void UpdateTokens()
        {
            for (int i = 0; i < Count; i++)
                GetMember(i).MetadataToken = new MetadataToken(TokenType, (uint)(i + 1));
        }

        public abstract void UpdateRows(NetBuildingContext context);

    }

    public abstract class MetadataTable<TMember> : MetadataTable, ICollection<TMember>
        where TMember: MetadataMember 
    {
        private readonly List<TMember> _members;
        private ReadingContext _readingContext;

        protected MetadataTable()
        {
            _members = new List<TMember>();
        }

        public TMember this[int index]
        {
            get
            {
                if (_members[index] == null && _readingContext != null)
                {
                    var context =
                        _readingContext.CreateSubContext(_readingContext.Reader.StartPosition + (index * GetElementByteCount()));
                    _members[index] = ReadMember(new MetadataToken(TokenType, (uint)(index + 1)), context);
                }
                return _members[index];
            }
            set { _members[index] = value; }
        }

        public override int Count
        {
            get { return _members.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        internal override void SetMemberCount(uint capacity)
        {
            _members.AddRange(new TMember[capacity]);
        }

        internal override void SetReadingContext(ReadingContext readingContext)
        {
            _readingContext = readingContext;
        }
        
        public void Add(TMember item)
        {
            _members.Add(item);
            item.Header = TableStream.StreamHeader.MetaDataHeader;
        }

        public void Clear()
        {
            _members.Clear();
        }

        public bool Contains(TMember item)
        {
            return _members.Contains(item);
        }

        public void CopyTo(TMember[] array, int arrayIndex)
        {
            _members.CopyTo(array, arrayIndex);
        }

        public bool Remove(TMember item)
        {
            if (_members.Remove(item))
            {
                item.Header = null;
                return true;
            }
            return false;
        }

        public bool TryGetMember(int index, out TMember member)
        {
            if (index >= 0 && index < Count)
            {
                member = this[index];
                return true;
            }
            member = null;
            return false;
        }

        public override MetadataMember GetMember(int index)
        {
            return this[index];
        }

        public override void UpdateRows(NetBuildingContext context)
        {
            foreach (var member in this)
                UpdateMember(context, member);
        }

        protected abstract TMember ReadMember(MetadataToken token, ReadingContext context);

        protected abstract void UpdateMember(NetBuildingContext context, TMember member);
        
        protected abstract void WriteMember(WritingContext context, TMember member);

        public override void Write(WritingContext context)
        {
            foreach (var member in this)
                WriteMember(context, member);
        }

        public IEnumerator<TMember> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
