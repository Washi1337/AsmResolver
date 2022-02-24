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
        /// <param name="owner">The owner of the method body.</param>
        protected MethodBody(MethodDefinition owner)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        /// <summary>
        /// Gets the method that owns the method body.
        /// </summary>
        public MethodDefinition Owner
        {
            get;
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
