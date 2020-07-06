using System;
using AsmResolver.Collections;

namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Provides an implementation for a single data entry in a Win32 resource directory.
    /// </summary>
    public class ResourceData : IResourceData
    {
        private readonly LazyVariable<ISegment> _contents;

        /// <summary>
        /// Initializes a new resource data entry.
        /// </summary>
        protected ResourceData()
        {
            _contents = new LazyVariable<ISegment>(() => GetContents());
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
        public IResourceDirectory ParentDirectory
        {
            get;
            private set;
        }

        IResourceDirectory IOwnedCollectionElement<IResourceDirectory>.Owner
        {
            get => ParentDirectory;
            set => ParentDirectory = value;
        }
        
        /// <inheritdoc />
        public string Name
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
        
        /// <inheritdoc />
        public ISegment Contents
        {
            get => _contents.Value;
            set => _contents.Value = value;
        }
        
        /// <inheritdoc />
        public uint CodePage
        {
            get;
            set;
        }

        /// <inheritdoc />
        public bool CanRead => Contents is IReadableSegment;

        /// <inheritdoc />
        public IBinaryStreamReader CreateReader()
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
        protected virtual ISegment GetContents() => null;

        /// <inheritdoc />
        public override string ToString() => $"Data ({Name ?? Id.ToString()})";

    }
}