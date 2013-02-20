using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public abstract class MetaDataMember : IDisposable , ICacheProvider
    {
        internal PE.NETTableReader tablereader;
        internal int metadatatoken;
        internal MetaDataRow metadatarow;

        internal NETHeader netheader;
        
        public int MetaDataToken
        {
            get { return metadatatoken; }
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
        public object ProcessPartType(int partindex, object value)
        {
            return Convert.ChangeType(value, metadatarow.parts[partindex].GetType());
        }
        public bool HasMetaDataRow
        {
            get { return metadatarow != null; }
        }

        public void ApplyChanges()
        {
            if (HasMetaDataRow)
            {
                byte[] generatedBytes = metadatarow.GenerateBytes();
                netheader.ParentAssembly.peImage.Write((int)metadatarow.offset, generatedBytes);

            }
            else
                throw new ArgumentException("Cannot apply changes to a member without a metadata row.");
        }

        public void Dispose()
        {
            metadatarow = null;
            ClearCache();
        }

        public abstract void ClearCache();
    
    
    }
}
