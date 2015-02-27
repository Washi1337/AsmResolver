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
            set;
        }

        public MetadataToken MetadataToken
        {
            get;
            set;
        }

        public MetadataHeader Header
        {
            get;
            set;
        }
    }

    public abstract class MetadataMember<TRow> : MetadataMember
        where TRow : MetadataRow
    {
        protected MetadataMember(MetadataHeader header, MetadataToken token, TRow row)
            : base(header, token, row)
        {
            Header = header;
        }

        public new TRow MetadataRow
        {
            get { return (TRow)base.MetadataRow; }
        }
    }
}