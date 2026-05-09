using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Provides a lazy-initialized implementation of <see cref="MethodEntryPointsSection"/> that is read from an
    /// input .NET executable file.
    /// </summary>
    public class SerializedMethodEntryPointsSection : MethodEntryPointsSection
    {
        private readonly BinaryStreamReaderState _readerState;

        /// <summary>
        /// Reads a method entry points section from the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        public SerializedMethodEntryPointsSection(ref BinaryStreamReader reader)
        {
            Offset = reader.Offset;
            Rva = reader.Rva;

            _readerState = reader.GetState();
        }

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override BinaryStreamReader CreateReader() => _readerState.CreateReader();

        /// <inheritdoc />
        protected override NativeArray<MethodEntryPoint> GetEntryPoints()
        {
            return NativeArray<MethodEntryPoint>.FromReader(
                _readerState.CreateReader(),
                reader => MethodEntryPoint.FromReader(ref reader)
            );
        }

    }
}
