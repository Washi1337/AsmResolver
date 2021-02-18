using System;
using System.IO;

namespace AsmResolver.PE.Debug.CodeView
{
    /// <summary>
    /// Represents a debug data stream using the CodeView format, wrapping an instance of <see cref="SegmentBase"/>
    /// into a <see cref="IDebugDataSegment"/>.
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
        /// <param name="reader"></param>
        /// <returns></returns>
        public static CodeViewDataSegment FromReader(IBinaryStreamReader reader)
        {
            var signature = (CodeViewSignature) reader.ReadUInt32();

            return signature switch
            {
                CodeViewSignature.Rsds => RsdsDataSegment.FromReader(reader),
                CodeViewSignature.Nb10 => throw new NotImplementedException(),
                _ => throw new InvalidDataException()
            };
        }
    }
}
