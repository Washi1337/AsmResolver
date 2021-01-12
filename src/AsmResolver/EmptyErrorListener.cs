using System;

namespace AsmResolver
{
    public class EmptyErrorListener : IErrorListener
    {
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