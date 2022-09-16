using AsmResolver.IO;
using AsmResolver.PE.Code;
using AsmResolver.PE.File.Headers;
using AsmResolver.PE.Relocations;

namespace AsmResolver.PE.Platforms
{
    /// <summary>
    /// Provides information and services for the AMD64 target platform.
    /// </summary>
    public class Amd64Platform : Platform
    {
        /// <summary>
        /// Gets the singleton instance for the AMD64 platform.
        /// </summary>
        public static Amd64Platform Instance
        {
            get;
        } = new();

        /// <inheritdoc />
        public override MachineType TargetMachine => MachineType.Amd64;

        /// <inheritdoc />
        public override bool IsClrBootstrapperRequired => false;

        /// <inheritdoc />
        public override RelocatableSegment CreateThunkStub(ISymbol entryPoint)
        {
            var segment = new CodeSegment(new byte[]
            {
                0x48, 0xA1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // rex.w rex.b mov rax, [&symbol]
                0xFF, 0xE0                                                  // jmp [rax]
            });
            segment.AddressFixups.Add(new AddressFixup(2, AddressFixupType.Absolute64BitAddress, entryPoint));

            return new RelocatableSegment(segment, new[]
            {
                new BaseRelocation(RelocationType.Dir64, segment.ToReference(2))
            });
        }

        /// <inheritdoc />
        public override bool TryExtractThunkAddress(IPEImage image, BinaryStreamReader reader, out uint rva)
        {
            if (reader.ReadUInt16() != 0xA148)
            {
                rva = 0;
                return false;
            }

            rva = (uint) (reader.ReadUInt64() - image.ImageBase);
            return reader.ReadUInt16() == 0xE0FF;
        }
    }
}
