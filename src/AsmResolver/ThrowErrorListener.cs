using System;

namespace AsmResolver
{
    /// <summary>
    /// Provides a singleton implementation for the <see cref="IErrorListener"/> interface, that throws
    /// every reported exception.
    /// </summary>
    public sealed class ThrowErrorListener : IErrorListener
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="ThrowErrorListener"/> class.
        /// </summary>
        public static ThrowErrorListener Instance
        {
            get;
        } = new ThrowErrorListener();

        private ThrowErrorListener()
        {
        }
        
        /// <inheritdoc />
        public void MarkAsFatal()
        {
        }

        /// <inheritdoc />
        public void RegisterException(Exception exception) => throw exception;
    }
}