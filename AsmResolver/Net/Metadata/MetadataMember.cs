namespace AsmResolver.Net.Metadata
{
    public abstract class MetadataMember : IMetadataMember
    {
        protected internal MetadataMember(MetadataHeader header, MetadataToken token, MetadataRow row)
        {
            Header = header;
            MetadataToken = token;
            MetadataRow = row;
        }

        public MetadataRow MetadataRow
        {
            get;
            private set;
        }

        public MetadataToken MetadataToken
        {
            get;
            internal set;
        }

        public MetadataHeader Header
        {
            get;
            internal set;
        }

        public MetadataTable Table
        {
            get
            {
                if (Header == null)
                    return null;
                var stream = Header.GetStream<TableStream>();
                if (stream == null)
                    return null;
                return stream.GetTable(MetadataToken.TokenType);
            }
        }
    }

    public abstract class MetadataMember<TRow> : MetadataMember
        where TRow : MetadataRow
    {
        protected MetadataMember(MetadataHeader header, MetadataToken token, TRow row)
            : base(header, token, row)
        {
        }

        public new TRow MetadataRow
        {
            get { return (TRow)base.MetadataRow; }
        }
    }
}