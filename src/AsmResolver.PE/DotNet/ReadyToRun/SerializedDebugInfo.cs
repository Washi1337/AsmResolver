using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Provides a lazy-initialized implementation of a <see cref="DebugInfo"/> that is read from a file.
    /// </summary>
    public class SerializedDebugInfo : DebugInfo
    {
        private readonly PEReaderContext _context;
        private readonly BinaryStreamReaderState _boundsReaderState;
        private readonly BinaryStreamReaderState _variablesReaderState;

        /// <summary>
        /// Reads debug information from the provided input stream.
        /// </summary>
        /// <param name="context">The context the reader is situated in.</param>
        /// <param name="reader">The input stream.</param>
        public SerializedDebugInfo(PEReaderContext context, in BinaryStreamReader reader)
        {
            _context = context;

            var state = reader.GetState();

            var lookahead = reader;
            uint lookBack = NativeFormat.DecodeUnsigned(ref lookahead);
            if (lookBack != 0)
                state.CurrentOffset -= lookBack;

            var nibbleReader = new NibbleReader(reader);
            uint boundsByteCount = nibbleReader.Read3BitEncodedUInt();
            uint variablesByteCount = nibbleReader.Read3BitEncodedUInt();

            _boundsReaderState = state.WithRelativeOffsetSize(nibbleReader.BaseReader.RelativeOffset, boundsByteCount);
            _variablesReaderState = state.WithRelativeOffsetSize(nibbleReader.BaseReader.RelativeOffset + boundsByteCount, variablesByteCount);
        }

        /// <inheritdoc />
        protected override IList<DebugInfoBounds> GetBounds()
        {
            var result = base.GetBounds();
            if (_boundsReaderState.Length == 0)
                return result;

            var reader = new NibbleReader(_boundsReaderState.CreateReader());
            uint count = reader.Read3BitEncodedUInt();

            uint nativeOffset = 0;
            for (int i = 0; i < count; i++)
            {
                nativeOffset += reader.Read3BitEncodedUInt();

                result.Add(new DebugInfoBounds(
                    nativeOffset,
                    reader.Read3BitEncodedUInt() + DebugInfoBounds.EpilogOffset,
                    (DebugInfoAttributes) reader.Read3BitEncodedUInt()
                ));
            }

            return result;
        }

        /// <inheritdoc />
        protected override IList<DebugInfoVariable> GetVariables()
        {
            var result = base.GetVariables();
            if (_variablesReaderState.Length == 0)
                return result;

            var reader = new NibbleReader(_variablesReaderState.CreateReader());
            uint count = reader.Read3BitEncodedUInt();

            for (int i = 0; i < count; i++)
                result.Add(DebugInfoVariable.FromReader(_context, ref reader));

            return result;
        }
    }
}
