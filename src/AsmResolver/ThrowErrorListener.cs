using System;

namespace AsmResolver
{
    public sealed class ThrowErrorListener : IErrorListener
    {
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