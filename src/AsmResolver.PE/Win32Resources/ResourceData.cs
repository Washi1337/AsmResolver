using System;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.Collections;
using AsmResolver.IO;

namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Represents a single data entry in a Win32 resource directory.
    /// </summary>
    public partial class ResourceData : IResourceEntry
    {
        private readonly object _lock = new();

        /// <summary>
        /// Initializes a new resource data entry.
        /// </summary>
        protected ResourceData()
        {
        }

        /// <summary>
        /// Creates a new named data entry.
        /// </summary>
        /// <param name="name">The name of the entry.</param>
        /// <param name="contents">The data to store in the entry.</param>
        public ResourceData(string name, ISegment contents)
            : this()
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Contents = contents ?? throw new ArgumentNullException(nameof(contents));
        }

        /// <summary>
        /// Creates a new data entry defined by a numerical identifier..
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="contents">The data to store in the entry.</param>
        public ResourceData(uint id, ISegment contents)
            : this()
        {
            Id = id;
            Contents = contents ?? throw new ArgumentNullException(nameof(contents));
        }

        /// <inheritdoc />
        public ResourceDirectory? ParentDirectory
        {
            get;
            private set;
        }

        ResourceDirectory? IOwnedCollectionElement<ResourceDirectory>.Owner
        {
            get => ParentDirectory;
            set => ParentDirectory = value;
        }

        /// <inheritdoc />
        public string? Name
        {
            get;
            set;
        }

        /// <inheritdoc />
        public uint Id
        {
            get;
            set;
        }

        /// <inheritdoc />
        bool IResourceEntry.IsDirectory => false;

        /// <inheritdoc />
        bool IResourceEntry.IsData => true;

        /// <summary>
        /// Gets or sets the raw contents of the data entry.
        /// </summary>
        [LazyProperty]
        public partial ISegment? Contents
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the code page that is used to decode code point values within the resource data.
        /// </summary>
        /// <remarks>
        /// Typically, the code page would be the Unicode code page.
        /// </remarks>
        public uint CodePage
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Contents"/> is readable using a binary stream reader.
        /// </summary>
        [MemberNotNullWhen(true, nameof(Contents))]
        public bool CanRead => Contents is IReadableSegment;

        /// <summary>
        /// Creates a new binary stream reader that reads the raw contents of the resource file.
        /// </summary>
        /// <returns>The reader.</returns>
        public BinaryStreamReader CreateReader()
        {
            return Contents is IReadableSegment readableSegment
                ? readableSegment.CreateReader()
                : throw new InvalidOperationException("Resource file is not readable.");
        }

        /// <summary>
        /// Obtains the contents of the data entry.
        /// </summary>
        /// <returns>The contents.</returns>
        /// <remarks>
        /// This method is called upon initializing the value for the <see cref="Contents"/> property.
        /// </remarks>
        protected virtual ISegment? GetContents() => null;

        /// <inheritdoc />
        public override string ToString() => $"Data ({Name ?? Id.ToString()})";

    }
}
