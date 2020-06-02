using System;
using System.Runtime.Serialization;

namespace AsmResolver.DotNet.Builder
{
    /// <summary>
    /// Represents the exception that occurs when the .NET metadata builder transitions into an illegal state.
    /// </summary>
    [Serializable]
    public class MetadataBuilderException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="MetadataBuilderException"/> class.
        /// </summary>
        public MetadataBuilderException()
            : base("An error occurred during the assembly of a .NET data directory.")
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="MetadataBuilderException"/> class with the provided message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public MetadataBuilderException(string message)
            : base(message)
        {
        }
        
        /// <summary>
        /// Creates a new instance of the <see cref="MetadataBuilderException"/> class with the provided message.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="inner">The inner exception causing the exception.</param>
        public MetadataBuilderException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <inheritdoc />
        protected MetadataBuilderException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}