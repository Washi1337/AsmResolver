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
            Offset = reader.Offset;
            Rva = reader.Rva;

            _reader = reader;
        }

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override BinaryStreamReader CreateReader() => _reader.Fork();

        /// <inheritdoc />
        protected override NativeArray<MethodEntryPoint> GetEntryPoints()
        {
            return NativeArray<MethodEntryPoint>.FromReader(
                _reader,
                reader => MethodEntryPoint.FromReader(ref reader)
            );
        }

    }
}
