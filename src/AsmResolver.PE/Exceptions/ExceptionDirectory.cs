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
        public IList<TFunction> Entries
        {
            get
            {
                if (_entries is null)
                    Interlocked.CompareExchange(ref _entries, GetEntries(), null);
                return _entries;
            }
        }

        /// <summary>
        /// Obtains the entries in the exception handler table.
        /// </summary>
        /// <returns>The entries.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Entries"/> property.
        /// </remarks>
        protected virtual IList<TFunction> GetEntries() => new List<TFunction>();

        /// <inheritdoc />
        IEnumerable<IRuntimeFunction> IExceptionDirectory.GetEntries() => (IEnumerable<IRuntimeFunction>) Entries;
    }
}
