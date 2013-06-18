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
        }

       /// <summary>
       /// Indicates the writer should rebuild the application as it would be a managed application.
       /// </summary>
        public bool BuildAsManagedApp { get { return true; } set { throw new NotImplementedException(); } }
        
    }
}
