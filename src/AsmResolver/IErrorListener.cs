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

    public static class ErrorListenerExtensions
    {
        /// <summary>
        /// Registers an error, and returns a default value for the provided type.
        /// </summary>
        /// <param name="exception">The error.</param>
        /// <typeparam name="T">The type of value to return.</typeparam>
        public static T RegisterExceptionAndReturnDefault<T>(this IErrorListener self, Exception exception)
        {
            self.RegisterException(exception);
            return default;
        }
    }
}