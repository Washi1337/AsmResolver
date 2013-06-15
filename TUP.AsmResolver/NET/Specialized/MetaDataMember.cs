using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.PE.Readers;

namespace TUP.AsmResolver.NET.Specialized
{
    public abstract class MetaDataMember : IDisposable , ICacheProvider, IImageProvider
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
            get { return metadatarow; }
            set { metadatarow = value; }
        }

        public NETHeader NETHeader
        {
            get { return netheader; }
        }

        public MetaDataTableType Table
        {
            get { return table; }
        }

        public object ProcessPartType(int partindex, object value)
        {
            return Convert.ChangeType(value, metadatarow.parts[partindex].GetType());
        }

        public bool HasImage
        {
            get { return netheader != null; }
        }

        public bool HasSavedMetaDataRow
        {
            get { return HasImage && metadatarow != null && metadatarow.offset != 0; }
        }

        public bool UpdateRowOnRebuild
        {
            get;
            set;
        }

        public void ApplyChanges()
        {
            if (HasSavedMetaDataRow && metadatarow.offset != 0)
            {
                byte[] generatedBytes = metadatarow.GenerateBytes();
                netheader.ParentAssembly.peImage.SetOffset(metadatarow.offset);
                netheader.ParentAssembly.peImage.Writer.Write(generatedBytes);

            }
           // else
           //     throw new ArgumentException("Cannot apply changes to a member without a metadata row.");
        }

        public void Dispose()
        {
            metadatarow = null;
            ClearCache();
        }

        public abstract void ClearCache();
        
    }
}
