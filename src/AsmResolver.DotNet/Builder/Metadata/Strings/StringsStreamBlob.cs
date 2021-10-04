using AsmResolver.IO;

namespace AsmResolver.DotNet.Builder.Metadata.Strings
{
    internal readonly struct StringsStreamBlob : IWritable
    {
        public StringsStreamBlob(Utf8String blob, bool isFixed)
        {
            Blob = blob.GetBytesUnsafe();
            Flags = isFixed
                ? StringsStreamBlobFlags.ZeroTerminated | StringsStreamBlobFlags.Fixed
                : StringsStreamBlobFlags.ZeroTerminated;
        }

        public StringsStreamBlob(Utf8String blob, StringsStreamBlobFlags flags)
        {
            Blob = blob;
            Flags = flags;
        }

        public Utf8String Blob
        {
            get;
        }

        public StringsStreamBlobFlags Flags
        {
            get;
        }

        public bool IsZeroTerminated => (Flags & StringsStreamBlobFlags.ZeroTerminated) != 0;

        public bool IsFixed => (Flags & StringsStreamBlobFlags.Fixed) != 0;

        /// <inheritdoc />
        public uint GetPhysicalSize() => (uint) (Blob.ByteCount + (IsZeroTerminated ? 1 : 0));

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
            writer.WriteBytes(Blob.GetBytesUnsafe());
            if (IsZeroTerminated)
                writer.WriteByte(0);
        }
    }
}
