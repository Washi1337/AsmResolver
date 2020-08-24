using System;
using System.Collections.Generic;

namespace AsmResolver
{
    /// <summary>
    /// Provides a container for collecting exceptions during a process.
    /// </summary>
    public class DiagnosticBag
    {
        /// <summary>
        /// Gets a collection of all the exceptions that were thrown during the process.
        /// </summary>
        public IList<Exception> Exceptions
        {
            get;
        } = new List<Exception>();

        /// <summary>
        /// Gets a value indicating whether the diagnostic bag contains any errors.
        /// </summary>
        public bool HasErrors => Exceptions.Count > 0;

        /// <summary>
        /// Gets a value indicating whether the process had thrown an exception that was fatal and could not be
        /// recovered from.
        /// </summary>
        public bool IsFatal
        {
            get;
            private set;
        }

        /// <summary>
        /// Marks the process to have failed.
        /// </summary>
        public void MarkAsFatal() => IsFatal = true;

        /// <summary>
        /// Registers an error in the diagnostic bag.
        /// </summary>
        /// <param name="exception">The error.</param>
        public void RegisterException(Exception exception)
        {
            Exceptions.Add(exception);
        }
        
        /// <summary>
        /// Registers an error in the diagnostic bag, and returns a default value for the provided type.
        /// </summary>
        /// <param name="exception">The error.</param>
        /// <typeparam name="T">The type of value to return.</typeparam>
        public T RegisterExceptionAndReturnDefault<T>(Exception exception)
        {
            Exceptions.Add(exception);
            return default;
        }
    }
}