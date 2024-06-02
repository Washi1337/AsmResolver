using System;

namespace AsmResolver.DotNet.Memory
{
    /// <summary>
    /// Represents the exception that occurs when a structure contains cyclic dependencies. That is, it defines at
    /// least one field of which the field type constructs a dependency cycle.
    /// </summary>
    [Serializable]
    public class CyclicStructureException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CyclicStructureException"/>.
        /// </summary>
        public CyclicStructureException()
            : base("The structure defines a field which introduces a cyclic dependency.")
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CyclicStructureException"/>.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public CyclicStructureException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CyclicStructureException"/>.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="inner">The inner cause of the exception.</param>
        public CyclicStructureException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
