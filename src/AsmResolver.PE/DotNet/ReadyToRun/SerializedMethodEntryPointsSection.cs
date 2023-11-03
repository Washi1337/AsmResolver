using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Provides a lazy-initialized implementation of <see cref="MethodEntryPointsSection"/> that is read from an
    /// input .NET executable file.
    /// </summary>
    public class SerializedMethodEntryPointsSection : MethodEntryPointsSection
    {
        private readonly BinaryStreamReader _reader;

        /// <summary>
        /// Reads a method entry points section from the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        public SerializedMethodEntryPointsSection(ref BinaryStreamReader reader)
        {
            _reader = reader;
        }

        /// <inheritdoc />
        protected override IList<MethodEntryPoint> GetEntryPoints()
        {
            var result = base.GetEntryPoints();

            var array = new NativeArrayView(_reader);
            for (int i = 0; i < array.Count; i++)
            {
                if (array.TryGet(i, out var elementReader))
                    result.Add(MethodEntryPoint.FromReader(ref elementReader));
            }

            return result;
        }
    }
}
