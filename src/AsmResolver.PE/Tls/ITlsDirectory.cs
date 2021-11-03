using System.Collections.Generic;

namespace AsmResolver.PE.Tls
{
    /// <summary>
    /// Represents the data directory containing Thread-Local Storage (TLS) data.
    /// </summary>
    public interface ITlsDirectory
    {
        /// <summary>
        /// Gets or sets the block of data that is used as a template to initialize TLS data.  The system copies all
        /// of this data each time a thread is created.
        /// </summary>
        IReadableSegment? TemplateData
        {
            get;
            set;
        }

        /// <summary>
        /// The location to receive the TLS index, which the loader assigns
        /// </summary>
        ISegmentReference Index
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a table of function callbacks that need to be called upon every thread creation.
        /// </summary>
        IList<ISegmentReference> CallbackFunctions
        {
            get;
        }

        /// <summary>
        /// Gets or sets the number of zero bytes that need to be appended after the template data referenced by
        /// <see cref="TemplateData"/>.
        /// </summary>
        uint SizeOfZeroFill
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the characteristics that are assigned to this directory.
        /// </summary>
        TlsCharacteristics Characteristics
        {
            get;
            set;
        }
    }
}
