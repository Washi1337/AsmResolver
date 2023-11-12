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
        private readonly BinaryStreamReader _boundsReader;
        private readonly BinaryStreamReader _variablesReader;

        /// <summary>
        /// Reads debug information from the provided input stream.
        /// </summary>
        /// <param name="context">The context the reader is situated in.</param>
        /// <param name="reader">The input stream.</param>
        public SerializedDebugInfo(PEReaderContext context, BinaryStreamReader reader)
        {
            _context = context;

            var original = reader;
            uint lookBack = NativeFormat.DecodeUnsigned(ref reader);
            if (lookBack != 0)
                reader.Offset = original.Offset - lookBack;

            var nibbleReader = new NibbleReader(reader);
            uint boundsByteCount = nibbleReader.Read3BitEncodedUInt();
            uint variablesByteCount = nibbleReader.Read3BitEncodedUInt();

            _boundsReader = reader.ForkRelative(nibbleReader.BaseReader.RelativeOffset, boundsByteCount);
            _variablesReader = reader.ForkRelative(nibbleReader.BaseReader.RelativeOffset + boundsByteCount, variablesByteCount);
        }

        /// <inheritdoc />
        protected override IList<DebugInfoBounds> GetBounds()
        {
            var result = base.GetBounds();
            if (_boundsReader.Length == 0)
                return result;

            var reader = new NibbleReader(_boundsReader);
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
            if (_variablesReader.Length == 0)
                return result;

            var reader = new NibbleReader(_variablesReader);
            uint count = reader.Read3BitEncodedUInt();

            for (int i = 0; i < count; i++)
                result.Add(DebugInfoVariable.FromReader(_context, ref reader));

            return result;
        }
    }
}
