using System.Collections.Generic;

namespace AsmResolver.PE.Exports
{
    /// <summary>
    /// Represents the data directory containing exported symbols that other images can access through dynamic linking.
    /// </summary>
    public interface IExportDirectory
    {
        /// <summary>
        /// Gets or sets the flags associated to the export directory.
        /// </summary>
        /// <remarks>
        /// This field is reserved and should be zero.
        /// </remarks>
        uint ExportFlags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time and date that the exports data was created.
        /// </summary>
        uint TimeDateStamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the user major version number.
        /// </summary>
        ushort MajorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the user minor version number.
        /// </summary>
        ushort MinorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the exports directory.
        /// </summary>
        string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets an ordered list of symbols that are exported by the portable executable (PE) image.
        /// </summary>
        IList<ExportedSymbol> Entries
        {
            get;
        }
    }
}