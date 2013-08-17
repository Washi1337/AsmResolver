using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.PE.Readers;

namespace TUP.AsmResolver.NET.Specialized
{
    public interface IMetaDataMember : ICacheProvider, IDisposable, IImageProvider, ICloneable
    {
        uint MetaDataToken { get; }
        uint TableIndex { get; }
        MetaDataRow MetaDataRow { get; set; }
        NETHeader NETHeader { get; }
        MetaDataTableType TableType { get; }
        MetaDataTable Table { get; }
        bool UpdateRowOnRebuild { get; set; }
        bool HasSavedMetaDataRow { get; }

        void ApplyChanges();
    }

    public abstract class MetaDataMember : IMetaDataMember
    {
        public MetaDataMember(MetaDataRow row)
        {
            UpdateRowOnRebuild = true;
            _metadatarow = row;
        }

        internal uint _metadatatoken;
        internal MetaDataRow _metadatarow;
        internal MetaDataTableType _table;
        internal NETHeader _netheader;

        public uint MetaDataToken
        {
            get { return _metadatatoken; }
        }

        public uint TableIndex
        {
            get { return (uint)((_metadatatoken | (0xFF << 24)) - (0xFF << 24)); }
        }

        public MetaDataRow MetaDataRow
        {
            get 
            { 
                return _metadatarow; 
            }
            set
            { 
                _metadatarow = value;
                _metadatarow._netHeader = this.NETHeader;
            }
        }

        public NETHeader NETHeader
        {
            get { return _netheader; }
        }

        public MetaDataTableType TableType
        {
            get { return _table; }
        }

        public MetaDataTable Table
        {
            get
            {
                return _netheader.TablesHeap.GetTable(TableType, false);
            }
        }

        public ValueType ProcessPartType(int partindex, object value)
        {
            return Convert.ChangeType(value, _metadatarow._parts[partindex].GetType()) as ValueType;
        }

        public bool HasImage
        {
            get { return _netheader != null; }
        }

        public bool HasSavedMetaDataRow
        {
            get { return HasImage && _metadatarow._offset != 0; }
        }

        public bool UpdateRowOnRebuild
        {
            get;
            set;
        }

        public void ApplyChanges()
        {
            if (HasSavedMetaDataRow && _metadatarow._offset != 0)
            {
                byte[] generatedBytes = _metadatarow.GenerateBytes();
                _netheader.ParentAssembly._peImage.SetOffset(_metadatarow._offset);
                _netheader.ParentAssembly._peImage.Writer.Write(generatedBytes);

            }
           // else
           //     throw new ArgumentException("Cannot apply changes to a member without a metadata row.");
        }

        public void Dispose()
        {
            _metadatarow = default(MetaDataRow);
            ClearCache();
        }

        public abstract void ClearCache();

        public abstract void LoadCache();

        public object Clone()
        {
            return this.MemberwiseClone();
        }

    }
}
