using System;
using System.IO;
using System.IO.Compression;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Bundles
{
    public class BundleFile
    {
        private readonly LazyVariable<ISegment> _contents;

        public BundleFile()
        {
            _contents = new LazyVariable<ISegment>(GetContents);
        }

        public string RelativePath
        {
            get;
            set;
        }

        public BundleFileType Type
        {
            get;
            set;
        }

        public bool IsCompressed
        {
            get;
            set;
        }

        public ISegment Contents
        {
            get => _contents.Value;
            set => _contents.Value = value;
        }

        public bool CanRead => Contents is IReadableSegment;

        protected virtual ISegment? GetContents() => null;

        public bool TryGetReader(out BinaryStreamReader reader)
        {
            if (Contents is IReadableSegment segment)
            {
                reader = segment.CreateReader();
                return true;
            }

            reader = default;
            return false;
        }

        public byte[] GetData() => GetData(true);

        public byte[] GetData(bool decompressIfRequired)
        {
            if (TryGetReader(out var reader))
            {
                byte[] contents = reader.ReadToEnd();
                if (decompressIfRequired && IsCompressed)
                {
                    using var outputStream = new MemoryStream();

                    using var inputStream = new MemoryStream(contents);
                    using var deflate = new DeflateStream(inputStream, CompressionMode.Decompress);
                    {
                        deflate.CopyTo(outputStream);
                    }

                    contents = outputStream.ToArray();
                }

                return contents;
            }

            throw new InvalidOperationException("Contents of file is not readable.");
        }

        public override string ToString() => RelativePath;
    }
}
