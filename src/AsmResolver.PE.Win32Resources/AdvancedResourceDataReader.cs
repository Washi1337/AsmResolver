using System;
using AsmResolver.PE.Win32Resources.Version;

namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Provides an implementation for the <see cref="IWin32ResourceDataReader"/> interface, that attempts to
    /// interpret the data stored in the resource file to higher level structures. 
    /// </summary>
    public class AdvancedResourceDataReader : IWin32ResourceDataReader
    {
        /// <summary>
        /// Gets or sets a value indicating whether the reader should catch any errors that occur during the reading
        /// process of a resource data entry. 
        /// </summary>
        /// <remarks>
        /// When this value is set to <c>false</c> and an error occurs during the reading process, then
        /// see cref="ReadResourceData"/> will return <c>null</c> instead.
        /// </remarks>
        public bool ThrowOnInvalidFormat
        {
            get;
            set;
        } = false;
        
        /// <inheritdoc />
        public ISegment ReadResourceData(IResourceData dataEntry, IBinaryStreamReader reader)
        {
            if (dataEntry.ParentDirectory is null)
                throw new ArgumentException("Data entry is not added to a directory.");
         
            // First sub directory in the resources tree determines the resource type.
            
            var directory = dataEntry.ParentDirectory;
            while (directory.ParentDirectory.ParentDirectory is {})
                directory = directory.ParentDirectory;

            try
            {
                // Match the resource directory type.
                switch (directory.Type)
                {
                    case ResourceType.Version:
                        return VersionInfoResource.FromReader(reader);

                    // TODO: support remaining resource types.
                    
                    default:
                        // If unknown or unsupported data type, just return the raw data.
                        return DataSegment.FromReader(reader);
                }
            }
            catch (Exception) when (!ThrowOnInvalidFormat)
            {
                return null;
            }
        }
        
    }
}