using System;

namespace AsmResolver
{
    /// <summary>
    /// Provides a mechanism for capturing errors that occur during a process.
    /// </summary>
    public interface IErrorListener
    {
        /// <summary>
        /// Marks the process to have failed.
        /// </summary>
        void MarkAsFatal();

        /// <summary>
        /// Registers an error.
        /// </summary>
        /// <param name="exception">The error.</param>
        void RegisterException(Exception exception);
    }

    /// <summary>
    /// Provides extension methods for instances of the <see cref="IErrorListener"/> interface. 
    /// </summary>
    public static class ErrorListenerExtensions
    {
        /// <summary>
        /// Registers an instance of a <see cref="BadImageFormatException"/> class.
        /// </summary>
        /// <param name="self">The error listener.</param>
        /// <param name="message">The message of the error.</param>
        public static void BadImage(this IErrorListener self, string message)
        {
            self.RegisterException(new BadImageFormatException(message));
        }

        /// <summary>
        /// Registers an instance of a <see cref="NotSupportedException"/> class.
        /// </summary>
        /// <param name="self">The error listener.</param>
        /// <param name="message">The message of the error.</param>
        public static void NotSupported(this IErrorListener self, string message)
        {
            self.RegisterException(new NotSupportedException(message));
        }

        /// <summary>
        /// Registers an error, and returns a default value for the provided type.
        /// </summary>
        /// <param name="self">The error listener.</param>
        /// <param name="exception">The error.</param>
        /// <typeparam name="T">The type of value to return.</typeparam>
        public static T RegisterExceptionAndReturnDefault<T>(this IErrorListener self, Exception exception)
        {
            self.RegisterException(exception);
            return default;
        }
    }
}