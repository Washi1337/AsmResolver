using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.Exceptions
{
    /// <summary>
    /// Represents an exception that is occured when a resolving process has failed.
    /// </summary>
    public class ResolveException :Exception
    {
        /// <summary>
        /// Initializes a new ResolveException with a specific message.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        public ResolveException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new ResolveException with a specific inner exception.
        /// </summary>
        /// <param name="inner">The inner exception of the exception.</param>
        public ResolveException(Exception inner) : base("Failed to resolve the object.", inner) { }
        /// <summary>
        /// Initializes a new ResolveException with a specific message and inner exception.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="inner">The inner exception of the exception.</param>
        public ResolveException(string message, Exception inner) : base(message, inner) { }

    }
}
