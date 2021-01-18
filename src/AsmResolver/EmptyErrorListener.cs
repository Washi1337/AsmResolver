using System;

namespace AsmResolver
{
    /// <summary>
    /// Provides an empty implementation of the <see cref="IErrorListener"/> that silently consumes all reported errors.
    /// </summary>
    public class EmptyErrorListener : IErrorListener
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="EmptyErrorListener"/> class.
        /// </summary>
        public static EmptyErrorListener Instance
        {
            get;
        } = new EmptyErrorListener();
        
        private EmptyErrorListener()
        {
        }
        
        /// <inheritdoc />
        public void MarkAsFatal()
        {
        }

        /// <inheritdoc />
        public void RegisterException(Exception exception)
        {
        }
    }
}