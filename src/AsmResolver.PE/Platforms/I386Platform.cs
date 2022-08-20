using AsmResolver.IO;
using AsmResolver.PE.Code;
using AsmResolver.PE.File.Headers;
using AsmResolver.PE.Relocations;

namespace AsmResolver.PE.Platforms
{
    /// <summary>
    /// Provides information and services for the I386 target platform.
    /// </summary>
    public class I386Platform : Platform
    {
        /// <summary>
        /// Gets the singleton instance for the I386 platform.
        /// </summary>
        public static I386Platform Instance
        {
            get;
        } = new();

        /// <inheritdoc />
        public override MachineType TargetMachine => MachineType.I386;

        /// <inheritdoc />
        public override bool IsClrBootstrapperRequired => true;

        /// <inheritdoc />
        public override RelocatableSegment CreateThunkStub(ISymbol entrypoint)
        {
            var segment = new CodeSegment(new byte[]
            {
                0xFF, 0x25, 0x00, 0x00, 0x00, 0x00 // jmp [&symbol]
            });
            segment.AddressFixups.Add(new AddressFixup(2, AddressFixupType.Absolute32BitAddress, entrypoint));

            return new RelocatableSegment(segment, new[]
            {
                new BaseRelocation(RelocationType.HighLow, segment.ToReference(2))
            });
        }

        /// <inheritdoc />
        public override bool TryExtractThunkAddress(IPEImage image, BinaryStreamReader reader, out uint rva)
        {
            if (reader.ReadUInt16() != 0x25FF)
            {
                rva = 0;
                return false;
            }

            rva = (uint) (reader.ReadUInt32() - image.ImageBase);
            return true;
        }
    }
}
