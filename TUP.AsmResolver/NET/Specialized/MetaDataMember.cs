using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.PE.Readers;

namespace TUP.AsmResolver.NET.Specialized
{
    public interface IMetaDataMember
    {
        uint MetaDataToken { get; }
    }

    public abstract class MetaDataMember : IMetaDataMember, IDisposable, ICacheProvider, IImageProvider, ICloneable
    {
        public MetaDataMember(MetaDataRow row)
        {
            UpdateRowOnRebuild = true;
            metadatarow = row;
        }

        internal uint metadatatoken;
        internal MetaDataRow metadatarow;
        internal MetaDataTableType table;
        internal NETHeader netheader;

        public uint MetaDataToken
        {
            get { return metadatatoken; }
        }

        public uint TableIndex
        {
            get { return (uint)((metadatatoken | (0xFF << 24)) - (0xFF << 24)); }
        }

        public MetaDataRow MetaDataRow
        {
            get 
            { 
                return metadatarow; 
            }
            set
            { 
                metadatarow = value;
                metadatarow._netHeader = this.NETHeader;
            }
        }

        public NETHeader NETHeader
        {
            get { return netheader; }
        }

        public MetaDataTableType TableType
        {
            get { return table; }
        }

        public MetaDataTable Table
        {
            get
            {
                return netheader.TablesHeap.GetTable(TableType, false);
            }
        }

        public ValueType ProcessPartType(int partindex, object value)
        {
            return Convert.ChangeType(value, metadatarow._parts[partindex].GetType()) as ValueType;
        }

        public bool HasImage
        {
            get { return netheader != null; }
        }

        public bool HasSavedMetaDataRow
        {
            get { return HasImage && metadatarow._offset != 0; }
        }

        public bool UpdateRowOnRebuild
        {
            get;
            set;
        }

        public void ApplyChanges()
        {
            if (HasSavedMetaDataRow && metadatarow._offset != 0)
            {
                byte[] generatedBytes = metadatarow.GenerateBytes();
                netheader.ParentAssembly.peImage.SetOffset(metadatarow._offset);
                netheader.ParentAssembly.peImage.Writer.Write(generatedBytes);

            }
           // else
           //     throw new ArgumentException("Cannot apply changes to a member without a metadata row.");
        }

        public void Dispose()
        {
            metadatarow = default(MetaDataRow);
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
