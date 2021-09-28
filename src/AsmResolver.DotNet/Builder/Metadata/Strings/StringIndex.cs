namespace AsmResolver.DotNet.Builder.Metadata.Strings
{
    internal readonly struct StringIndex
    {
        public StringIndex(int blobIndex, uint offset)
        {
            BlobIndex = blobIndex;
            Offset = offset;
        }

        public int BlobIndex
        {
            get;
        }

        public uint Offset
        {
            get;
        }

        #if DEBUG
        /// <inheritdoc />
        public override string ToString() => $"{nameof(BlobIndex)}: {BlobIndex}, {nameof(Offset)}: {Offset}";
        #endif
    }
}
