using System;

namespace AsmResolver.DotNet.Code
{
    /// <summary>
    /// Represents a body of a method defined in a .NET assembly.
    /// </summary>
    public abstract class MethodBody
    {
        /// <summary>
        /// Initializes a new empty method body.
        /// </summary>
        protected MethodBody()
        {
        }

        /// <summary>
        /// Gets the method that owns the method body.
        /// </summary>
        public MethodDefinition? Owner
        {
            get;
            internal set;
        }

        /// <summary>
        /// When this method is stored in a serialized module, gets or sets the reference to the beginning of the
        /// raw contents of the body.
        /// </summary>
        public ISegmentReference? Address
        {
            get;
            set;
        }
    }
}
