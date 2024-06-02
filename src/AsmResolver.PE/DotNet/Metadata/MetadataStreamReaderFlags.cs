using System;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Special flags used to provide information about metadata to <see cref="IMetadataStreamReader"/>.
    /// </summary>
    [Flags]
    public enum MetadataStreamReaderFlags
    {
        /// <summary>
        /// No special flags are set.
        /// </summary>
        None = 0,
        /// <summary>
        /// EnC metadata is being used.
        /// </summary>
        IsEnc = 1,
        /// <summary>
        /// A #JTD stream is present.
        /// </summary>
        /// <remarks>
        /// If the #JTD stream is present and EnC metadata is being used, all indices in the tables stream are 4 bytes.
        /// </remarks>
        HasJtdStream = 2,
    }
}
