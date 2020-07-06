using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Resources;

namespace AsmResolver.PE.DotNet
{
    /// <summary>
    /// Represents a data directory containing the CLR 2.0 header and data directories of a .NET binary.
    /// </summary>
    public interface IDotNetDirectory : ISegment
    {
        /// <summary>
        /// Gets or sets the major runtime version of the directory format.
        /// </summary>
        /// <remarks>
        /// This field is set to 2 in most .NET binaries.
        /// </remarks>
        ushort MajorRuntimeVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minor runtime version of the directory format.
        /// </summary>
        /// <remarks>
        /// This field is set to 5 in most .NET binaries.
        /// </remarks>
        ushort MinorRuntimeVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the data directory containing the metadata of the .NET binary.
        /// </summary>
        IMetadata Metadata
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the flags associated to the .NET binary.
        /// </summary>
        DotNetDirectoryFlags Flags
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the metadata token or entrypoint virtual address, depending on whether
        /// <see cref="DotNetDirectoryFlags.NativeEntrypoint"/> is set in <see cref="Flags" />. 
        /// </summary>
        uint Entrypoint
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the data directory containing the embedded resources data of the .NET binary (if available).
        /// </summary>
        DotNetResourcesDirectory DotNetResources
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the data directory containing the strong name signature of the .NET binary (if available).
        /// </summary>
        IReadableSegment StrongName
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the data directory containing the code manager table of the .NET binary (if available).
        /// </summary>
        IReadableSegment CodeManagerTable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the data directory containing the VTable fixups that need to be applied when executing mixed
        /// mode applications (if available).
        /// </summary>
        IReadableSegment VTableFixups
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the data directory containing the addresses to native stubs of exports defined in the
        /// .NET binary (if available). 
        /// </summary>
        IReadableSegment ExportAddressTable
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the data directory containing the managed native header of a mixed mode application (if available).
        /// </summary>
        IReadableSegment ManagedNativeHeader
        {
            get;
            set;
        }
    }
}