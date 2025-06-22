using System.Collections.Generic;
using System.Threading;

namespace AsmResolver.PE.Exceptions
{
    /// <summary>
    /// Provides a basic implementation of the <see cref="IExceptionDirectory"/> directory.
    /// </summary>
    /// <typeparam name="TFunction">The type of functions to store.</typeparam>
    public class ExceptionDirectory<TFunction> : IExceptionDirectory
        where TFunction : IRuntimeFunction
    {
        private IList<TFunction>? _entries;

        /// <summary>
        /// Gets a collection of functions that are stored in the table.
        /// </summary>
        public IList<TFunction> Functions
        {
            get
            {
                if (_entries is null)
                    Interlocked.CompareExchange(ref _entries, GetFunctions(), null);
                return _entries;
            }
        }

        /// <summary>
        /// Obtains the entries in the exception handler table.
        /// </summary>
        /// <returns>The entries.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Functions"/> property.
        /// </remarks>
        protected virtual IList<TFunction> GetFunctions() => new List<TFunction>();

        /// <inheritdoc />
        IEnumerable<IRuntimeFunction> IExceptionDirectory.GetFunctions() => (IEnumerable<IRuntimeFunction>) Functions;
    }
}
