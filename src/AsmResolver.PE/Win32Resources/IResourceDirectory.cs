using System.Collections.Generic;

namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Represents a single directory containing Win32 resources of a PE image. 
    /// </summary>
    public interface IResourceDirectory : IResourceEntry
    {
        /// <summary>
        /// Gets the type of resources stored in the directory.
        /// </summary>
        ResourceType Type
        {
            get;
        }

        /// <summary>
        /// Gets or sets the flags of the directory.
        /// </summary>
        /// <remarks>
        /// This field is reserved and is usually set to zero. 
        /// </remarks>
        uint Characteristics
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time that the resource data was created by the compiler.
        /// </summary>
        uint TimeDateStamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the major version number of the directory.
        /// </summary>
        ushort MajorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minor version number of the directory.
        /// </summary>
        ushort MinorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of entries that are stored in the directory.
        /// </summary>
        IList<IResourceEntry> Entries
        {
            get;
        }
    }
}