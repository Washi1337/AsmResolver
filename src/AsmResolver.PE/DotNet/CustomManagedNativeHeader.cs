using AsmResolver.IO;

namespace AsmResolver.PE.DotNet
{
    /// <summary>
    /// Represents a managed native header of a .NET Portable Executable that is in an unsupported or unknown file format.
    /// </summary>
    public class CustomManagedNativeHeader : SegmentBase, IManagedNativeHeader
    {
        /// <summary>
        /// Creates a new custom managed native header.
        /// </summary>
        /// <param name="signature">The signature to use.</param>
        /// <param name="contents">The contents of the header, excluding the signature.</param>
        public CustomManagedNativeHeader(ManagedNativeHeaderSignature signature, ISegment contents)
        {
            Signature = signature;
            Contents = contents;
        }

        /// <inheritdoc />
        public ManagedNativeHeaderSignature Signature
        {
            get;
        }

        /// <summary>
        /// Gets the contents of the header, excluding the signature.
        /// </summary>
        public ISegment Contents
        {
            get;
        }

        /// <inheritdoc />
        public override void UpdateOffsets(in RelocationParameters parameters)
        {
            base.UpdateOffsets(parameters);
            Contents.UpdateOffsets(parameters.WithAdvance(sizeof(ManagedNativeHeaderSignature)));
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize() => sizeof(ManagedNativeHeaderSignature) + Contents.GetPhysicalSize();

        /// <inheritdoc />
        public override void Write(BinaryStreamWriter writer)
        {
            writer.WriteUInt32((uint) Signature);
            Contents.Write(writer);
        }
    }
}
