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
        /// <param name="context">Context for the reader</param>
        /// <param name="reader">The input stream to read from.</param>
        /// <returns></returns>
        public static CodeViewDataSegment FromReader(PEReaderContext context, IBinaryStreamReader reader)
        {
            var signature = (CodeViewSignature) reader.ReadUInt32();

            return signature switch
            {
                CodeViewSignature.Rsds => RsdsDataSegment.FromReader(context, reader),
                CodeViewSignature.Nb05 => context.NotSupportedAndReturn<CodeViewDataSegment>(),
                CodeViewSignature.Nb09 => context.NotSupportedAndReturn<CodeViewDataSegment>(),
                CodeViewSignature.Nb10 => context.NotSupportedAndReturn<CodeViewDataSegment>(),
                CodeViewSignature.Nb11 => context.NotSupportedAndReturn<CodeViewDataSegment>(),
                _ => context.BadImageAndReturn<CodeViewDataSegment>("Invalid code view debug data signature.")
            };
        }
    }
}
