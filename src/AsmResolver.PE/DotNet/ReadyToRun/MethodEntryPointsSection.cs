using System;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Represents a section in a ReadyToRun directory of a .NET module containing the native entry points and fixups
    /// of all the ahead-of-time compiled managed methods.
    /// </summary>
    public class MethodEntryPointsSection : SegmentBase, IReadyToRunSection
    {
        private IList<MethodEntryPoint>? _entryPoints;

        /// <inheritdoc />
        public ReadyToRunSectionType Type => ReadyToRunSectionType.MethodDefEntryPoints;

        /// <inheritdoc />
        public virtual bool CanRead => false;

        /// <summary>
        /// Gets a collection of entry points stored in the section. The index of the element corresponds to the method
        /// as specified in the method definition table in the tables stream of the .NET module.
        /// </summary>
        public IList<MethodEntryPoint> EntryPoints
        {
            get
            {
                if (_entryPoints is null)
                    Interlocked.CompareExchange(ref _entryPoints, GetEntryPoints(), null);
                return _entryPoints;
            }
        }

        /// <summary>
        /// Obtains the entry points stored in the section.
        /// </summary>
        /// <returns>The entry points.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="EntryPoints"/> property.
        /// </remarks>
        protected virtual IList<MethodEntryPoint> GetEntryPoints() => new List<MethodEntryPoint>();

        /// <inheritdoc />
        public virtual BinaryStreamReader CreateReader() => throw new InvalidOperationException();

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            // TODO: nativearray needs to be made measurable.
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            // TODO: nativearray needs to be made writeable.
            throw new NotImplementedException();
        }
    }
}
