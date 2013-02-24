using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver
{
    /// <summary>
    /// Specifies arguments for rebuilding an application.
    /// </summary>
    public class WritingParameters
    {
        /// <summary>
        /// Creates a new instance of the WritingParameters, and sets the arguments to their default values.
        /// </summary>
        public WritingParameters()
        {
            RebuildResources = true;
            RebuildImportExportTables = true;
            RebuildNETHeaders = true;
            BuildAsManagedApp = false;
        }
        /// <summary>
        /// Indicates the writer should rebuild the resources directory.
        /// </summary>
        public bool RebuildResources { get; set; }
        /// <summary>
        /// Indicates the writer should rebuild the import and export tables.
        /// </summary>
        public bool RebuildImportExportTables { get; set; }
        /// <summary>
        /// Indicates the writer should rebuild the .NET directory.
        /// </summary>
        public bool RebuildNETHeaders { get; set; }
        /// <summary>
        /// Indicates the writer should rebuild the application as it would be a managed application.
        /// </summary>
        public bool BuildAsManagedApp { get; set; }
        
    }
}
