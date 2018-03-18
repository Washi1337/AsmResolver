namespace AsmResolver.Net
{
    public class ResourcesManifest : FileSegment
    {
        private readonly IBinaryStreamReader _reader;

        public ResourcesManifest()
        {
        }

        public ResourcesManifest(IBinaryStreamReader reader)
        {
            _reader = reader;
        }

        public static ResourcesManifest FromReadingContext(ReadingContext context)
        {
            return new ResourcesManifest(context.Reader);
        }

        /// <summary>
        /// Gets the managed resource data at the given offset.
        /// </summary>
        /// <param name="offset">The offset of the managed resource to get.</param>
        /// <returns>The raw data of the managed resource.</returns>
        public byte[] GetResourceData(uint offset)
        {
            if (_reader == null)
                return null;

            var reader = _reader.CreateSubReader(_reader.StartPosition + offset);
            int length = reader.ReadInt32();
            return reader.ReadBytes(length);
        }

        public override uint GetPhysicalLength()
        {
            return (uint) (_reader != null ? _reader.Length : 0);
        }

        public override void Write(WritingContext context)
        {
            var reader = _reader.CreateSubReader(_reader.StartPosition);
            context.Writer.WriteBytes(reader.ReadBytes((int) reader.Length));
        }
    }
}