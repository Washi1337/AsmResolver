using System;
using System.IO;

namespace AsmResolver.PE.Debug.CodeView
{
    /// <summary>
    /// Represents a debug data stream using the CodeView format
    /// </summary>
    public abstract class CodeViewDataSegment : SegmentBase, IDebugDataSegment
    {
        /// <summary>
        /// Get uniquely identifying signature for PDB format
        /// </summary>
        public abstract CodeViewSignature Signature
        {
            get;
        }

        /// <inheritdoc />
        public DebugDataType Type => DebugDataType.CodeView;

        /// <summary>
        /// Creates a new CodeViewDataSegment depending on CodeView Signature
        /// </summary>
        /// <param name="reader">The input stream to read from.</param>
        /// <param name="context">Context for the reader</param>
        /// <returns></returns>
        public static CodeViewDataSegment FromReader(IBinaryStreamReader reader, PEReaderContext context)
        {
            var signature = (CodeViewSignature) reader.ReadUInt32();

            return signature switch
            {
                CodeViewSignature.Rsds => RsdsDataSegment.FromReader(reader, context),
                CodeViewSignature.Nb05 => throw new NotSupportedException(),
                CodeViewSignature.Nb09 => throw new NotSupportedException(),
                CodeViewSignature.Nb10 => throw new NotSupportedException(),
                CodeViewSignature.Nb11 => throw new NotSupportedException(),
                _ => throw new InvalidDataException()
            };
        }
    }
}
