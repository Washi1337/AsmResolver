using System;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.IO;
using AsmResolver.PE.Exceptions;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Represents a platform-agnostic ReadyToRun section containing RUNTIME_FUNCTION structures describing all code
    /// blocks in the image with pointers to their unwind info.
    /// </summary>
    public abstract class RuntimeFunctionsSection : SegmentBase, IReadyToRunSection
    {
        /// <inheritdoc />
        public ReadyToRunSectionType Type => ReadyToRunSectionType.RuntimeFunctions;

        /// <inheritdoc />
        public virtual bool CanRead => false;

        /// <inheritdoc />
        public virtual BinaryStreamReader CreateReader() => throw new InvalidOperationException();

        /// <summary>
        /// Obtains the collection of functions stored in the section.
        /// </summary>
        /// <returns>The functions.</returns>
        public abstract IEnumerable<IRuntimeFunction> GetFunctions();
    }

    /// <summary>
    /// Represents a platform-specific ReadyToRun section containing RUNTIME_FUNCTION structures describing all code
    /// blocks in the image with pointers to their unwind info.
    /// </summary>
    /// <typeparam name="TFunction">The type of function to store in the section.</typeparam>
    public abstract class RuntimeFunctionsSection<TFunction> : RuntimeFunctionsSection
        where TFunction : IRuntimeFunction, IWritable
    {
        private IList<TFunction>? _functions;

        /// <summary>
        /// Gets a collection of functions stored in the section.
        /// </summary>
        public IList<TFunction> Functions
        {
            get
            {
                if (_functions is null)
                    Interlocked.CompareExchange(ref _functions, GetEntries(), null);
                return _functions;
            }
        }

        /// <summary>
        /// Obtains the entries stored in the section.
        /// </summary>
        /// <returns>The entries.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Functions"/> property.
        /// </remarks>
        protected virtual IList<TFunction> GetEntries() => new List<TFunction>();

        /// <inheritdoc />
        public sealed override IEnumerable<IRuntimeFunction> GetFunctions() => (IEnumerable<IRuntimeFunction>) Functions;

        /// <inheritdoc />
        public override uint GetPhysicalSize() => Functions.Count > 0
            ? (uint) Functions.Count * Functions[0].GetPhysicalSize()
            : 0;

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            for (int i = 0; i < Functions.Count; i++)
                Functions[i].Write(writer);
        }
    }
}
