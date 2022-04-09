using System;
using System.IO;
using System.IO.Compression;
using AsmResolver.Collections;
using AsmResolver.IO;

namespace AsmResolver.DotNet.Bundles
{
    /// <summary>
    /// Represents a single file in a .NET bundle manifest.
    /// </summary>
    public class BundleFile : IOwnedCollectionElement<BundleManifest>
    {
        private readonly LazyVariable<ISegment> _contents;

        /// <summary>
        /// Creates a new empty bundle file.
        /// </summary>
        /// <param name="relativePath">The path of the file, relative to the root of the bundle.</param>
        public BundleFile(string relativePath)
        {
            RelativePath = relativePath;
            _contents = new LazyVariable<ISegment>(GetContents);
        }

        /// <summary>
        /// Creates a new bundle file.
        /// </summary>
        /// <param name="relativePath">The path of the file, relative to the root of the bundle.</param>
        /// <param name="type">The type of the file.</param>
        /// <param name="contents">The contents of the file.</param>
        public BundleFile(string relativePath, BundleFileType type, byte[] contents)
            : this(relativePath, type, new DataSegment(contents))
        {
        }

        /// <summary>
        /// Creates a new empty bundle file.
        /// </summary>
        /// <param name="relativePath">The path of the file, relative to the root of the bundle.</param>
        /// <param name="type">The type of the file.</param>
        /// <param name="contents">The contents of the file.</param>
        public BundleFile(string relativePath, BundleFileType type, ISegment contents)
        {
            RelativePath = relativePath;
            Type = type;
            _contents = new LazyVariable<ISegment>(contents);
        }

        /// <summary>
        /// Gets the parent manifest this file was added to.
        /// </summary>
        public BundleManifest? ParentManifest
        {
            get;
            private set;
        }

        /// <inheritdoc />
        BundleManifest? IOwnedCollectionElement<BundleManifest>.Owner
        {
            get => ParentManifest;
            set => ParentManifest = value;
        }

        /// <summary>
        /// Gets or sets the path to the file, relative to the root directory of the bundle.
        /// </summary>
        public string RelativePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of the file.
        /// </summary>
        public BundleFileType Type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the data stored in <see cref="Contents"/> is compressed or not.
        /// </summary>
        /// <remarks>
        /// The default implementation of the application host by Microsoft only supports compressing files if it is
        /// a fully self-contained binary and the file is not the <c>.deps.json</c> nor the <c>.runtmeconfig.json</c>
        /// file. This property does not do validation on any of these conditions. As such, if the file is supposed to be
        /// compressed with any of these conditions not met, a custom application host template needs to be provided
        /// upon serializing the bundle for it to be runnable.
        /// </remarks>
        public bool IsCompressed
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the raw contents of the file.
        /// </summary>
        public ISegment Contents
        {
            get => _contents.Value;
            set => _contents.Value = value;
        }

        /// <summary>
        /// Gets a value whether the contents of the file can be read using a <see cref="BinaryStreamReader"/>.
        /// </summary>
        public bool CanRead => Contents is IReadableSegment;

        /// <summary>
        /// Obtains the raw contents of the file.
        /// </summary>
        /// <returns>The contents.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Contents"/> property.
        /// </remarks>
        protected virtual ISegment? GetContents() => null;

        /// <summary>
        /// Attempts to create a <see cref="BinaryStreamReader"/> that points to the start of the raw contents of the file.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns><c>true</c> if the reader was constructed successfully, <c>false</c> otherwise.</returns>
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

        /// <summary>
        /// Reads (and decompresses if necessary) the contents of the file.
        /// </summary>
        /// <returns>The contents.</returns>
        public byte[] GetData() => GetData(true);

        /// <summary>
        /// Reads the contents of the file.
        /// </summary>
        /// <param name="decompressIfRequired"><c>true</c> if the contents should be decompressed or not when necessary.</param>
        /// <returns>The contents.</returns>
        public byte[] GetData(bool decompressIfRequired)
        {
            if (TryGetReader(out var reader))
            {
                byte[] contents = reader.ReadToEnd();
                if (decompressIfRequired && IsCompressed)
                {
                    using var outputStream = new MemoryStream();

                    using var inputStream = new MemoryStream(contents);
                    using (var deflate = new DeflateStream(inputStream, CompressionMode.Decompress))
                    {
                        deflate.CopyTo(outputStream);
                    }

                    contents = outputStream.ToArray();
                }

                return contents;
            }

            throw new InvalidOperationException("Contents of file is not readable.");
        }

        /// <summary>
        /// Marks the file as compressed, compresses the file contents, and replaces the value of <see cref="Contents"/>
        /// with the result.
        /// </summary>
        /// <exception cref="InvalidOperationException">Occurs when the file was already compressed.</exception>
        /// <remarks>
        /// The default implementation of the application host by Microsoft only supports compressing files if it is
        /// a fully self-contained binary and the file is not the <c>.deps.json</c> nor the <c>.runtmeconfig.json</c>
        /// file. This method does not do validation on any of these conditions. As such, if the file is supposed to be
        /// compressed with any of these conditions not met, a custom application host template needs to be provided
        /// upon serializing the bundle for it to be runnable.
        /// </remarks>
        public void Compress()
        {
            if (IsCompressed)
                throw new InvalidOperationException("File is already compressed.");

            using var inputStream = new MemoryStream(GetData());

            using var outputStream = new MemoryStream();
            using (var deflate = new DeflateStream(outputStream, CompressionLevel.Optimal))
            {
                inputStream.CopyTo(deflate);
            }

            Contents = new DataSegment(outputStream.ToArray());
            IsCompressed = true;
        }

        /// <summary>
        /// Marks the file as uncompressed, decompresses the file contents, and replaces the value of
        /// <see cref="Contents"/> with the result.
        /// </summary>
        /// <exception cref="InvalidOperationException">Occurs when the file was not compressed.</exception>
        public void Decompress()
        {
            if (!IsCompressed)
                throw new InvalidOperationException("File is not compressed.");

            Contents = new DataSegment(GetData(true));
            IsCompressed = false;
        }

        /// <inheritdoc />
        public override string ToString() => RelativePath;
    }
}
