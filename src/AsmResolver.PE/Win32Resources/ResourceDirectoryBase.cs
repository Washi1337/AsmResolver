using System.Collections.Generic;
using System.Threading;

namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// When overriden from this class, represents a Win32 resources directory. 
    /// </summary>
    public abstract class ResourceDirectoryBase : IResourceDirectoryEntry
    {
        private IList<IResourceDirectoryEntry> _entries;

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
        bool IResourceDirectoryEntry.IsDirectory => true;

        /// <inheritdoc />
        bool IResourceDirectoryEntry.IsData => false;

        /// <summary>
        /// Gets or sets the flags of the directory.
        /// </summary>
        /// <remarks>
        /// This field is reserved and is usually set to zero. 
        /// </remarks>
        public uint Characteristics
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time that the resource data was created by the compiler.
        /// </summary>
        public uint TimeDateStamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the major version number of the directory.
        /// </summary>
        public ushort MajorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minor version number of the directory.
        /// </summary>
        public ushort MinorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of entries that are stored in the directory.
        /// </summary>
        public IList<IResourceDirectoryEntry> Entries
        {
            get
            {
                if (_entries is null)
                    Interlocked.CompareExchange(ref _entries, GetEntries(), null);
                return _entries;
            }
        }

        /// <summary>
        /// Obtains the list of entries in the directory.
        /// </summary>
        /// <returns>The list of entries.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Entries"/> property.
        /// </remarks> 
        protected abstract IList<IResourceDirectoryEntry> GetEntries();

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Directory ({Name ?? Id.ToString()})";
        }
        
    }
}