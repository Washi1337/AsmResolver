using AsmResolver.IO;
using AsmResolver.PE.Code;
using AsmResolver.PE.File;
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
        public override bool Is32Bit => true;

        /// <inheritdoc />
        public override uint ThunkStubAlignment => 1;

        /// <inheritdoc />
        public override RelocatableSegment CreateThunkStub(ISymbol entryPoint)
        {
            var segment = new DataSegment([
                    0xFF, 0x25, 0x00, 0x00, 0x00, 0x00 // jmp [&symbol]
                ])
                .AsPatchedSegment()
                .Patch(2, AddressFixupType.Absolute32BitAddress, entryPoint);

            return new RelocatableSegment(segment, [
                new BaseRelocation(RelocationType.HighLow, segment.ToReference(2))
            ]);
        }

        /// <inheritdoc />
        public override bool TryExtractThunkAddress(PEImage image, BinaryStreamReader reader, out uint rva)
        {
            if (reader.ReadUInt16() != 0x25FF)
            {
                rva = 0;
                return false;
            }

            rva = (uint) (reader.ReadUInt32() - image.ImageBase);
            return true;
        }

        /// <inheritdoc />
        public override AddressTableInitializerStub CreateAddressTableInitializer(ISymbol virtualProtect)
        {
            return new I386AddressTableInitializerStub(virtualProtect);
        }

        private sealed class I386AddressTableInitializerStub : AddressTableInitializerStub
        {
            private static readonly byte[] PrologueStub =
            [
                /* 00: */ 0x83, 0x7C, 0x24, 0x08, 0x01, // cmp dword [esp+0x8],byte +0x1   ; dwReason == DLL_PROCESS_ATTACH
                /* 05: */ 0x74, 0x03,                   // jz 0xA
                /* 07: */ 0xC2, 0x0C, 0x00              // ret 0xC
                /* 0A: */
            ];

            private static readonly byte[] EpilogueStub =
            [
                /* 00: */ 0xC2, 0x0C, 0x00              // ret 0xc
            ];

            private static readonly byte[] SlotInitializerCode =
            [
                /* 00: */  0x55,                                  //   push ebp
                /* 01: */  0x89, 0xE5,                            //   mov ebp,esp
                /* 03: */  0x83, 0xEC, 0x04,                      //   sub esp, 4                   ; DWORD old;

                /* 06: */  0x8D, 0x45, 0xFC,                      //   lea eax,[ebp-0x4]            ; &old
                /* 09: */  0x50,                                  //   push eax
                /* 0A: */  0x6A, 0x04,                            //   push byte +0x4               ; PAGE_READWRITE
                /* 0C: */  0x6A, 0x04,                            //   push byte +0x4               ; size
                /* 0E: */  0xFF, 0x75, 0x08,                      //   push dword [ebp+0x8]         ; addr
                /* 11: */  0xFF, 0x15, 0x00, 0x00, 0x00, 0x00,    //   call [dword &VirtualProtect] ; VirtualProtect

                /* 17: */  0x8B, 0x55, 0x08,                      //   mov edx,[ebp+0x8]            ; *addr = value;
                /* 1A: */  0x8B, 0x45, 0x0C,                      //   mov eax,[ebp+0xc]
                /* 1D: */  0x89, 0x02,                            //   mov [edx],eax

                /* 1F: */  0x8D, 0x55, 0xFC,                      //   lea edx,[ebp-0x4]            ; &old
                /* 22: */  0x52,                                  //   push edx
                /* 23: */  0xFF, 0x32,                            //   push dword [edx]             ; old
                /* 25: */  0x6A, 0x04,                            //   push byte +0x4               ; size
                /* 27: */  0xFF, 0x75, 0x08,                      //   push dword [ebp+0x8]         ; addr
                /* 2A: */  0xFF, 0x15, 0x00, 0x00, 0x00, 0x00,    //   call [dword &VirtualProtect] ; VirtualProtect

                /* 30: */  0x89, 0xEC,                            //   mov esp,ebp
                /* 32: */  0x5D,                                  //   pop ebp
                /* 33: */  0xC2, 0x08, 0x00                       //   ret 8
            ];

            public I386AddressTableInitializerStub(ISymbol virtualProtect)
                : base(new DataSegment(PrologueStub), new DataSegment(EpilogueStub), CreateSlotInitializer(virtualProtect))
            {
            }

            private static RelocatableSegment CreateSlotInitializer(ISymbol virtualProtect)
            {
                var code = new DataSegment(SlotInitializerCode).AsPatchedSegment()
                    .Patch(0x13, AddressFixupType.Absolute32BitAddress, virtualProtect)
                    .Patch(0x2C, AddressFixupType.Absolute32BitAddress, virtualProtect);

                return new RelocatableSegment(code, [
                    new(RelocationType.HighLow, code.ToReference(0x13)),
                    new(RelocationType.HighLow, code.ToReference(0x2C))
                ]);
            }

            public override void AddInitializer(ISymbol originalSlot, ISymbol newSlot)
            {
                var code = new DataSegment([
                        /* 00: */ 0xFF, 0x35, 0x00, 0x00, 0x00, 0x00, // push dword [&src]
                        /* 06: */ 0x68, 0x00, 0x00, 0x00, 0x00,       // push dword &dest
                        /* 0B: */ 0xE8, 0x00, 0x00, 0x00, 0x00 // call init_slot
                    ]).AsPatchedSegment()
                    .Patch(0x02, AddressFixupType.Absolute32BitAddress, newSlot)
                    .Patch(0x07, AddressFixupType.Absolute32BitAddress, originalSlot)
                    .Patch(0x0C, AddressFixupType.Relative32BitAddress, SlotInitializer);

                Body.Add(new RelocatableSegment(code, [
                    new(RelocationType.HighLow, code.ToReference(0x02)),
                    new(RelocationType.HighLow, code.ToReference(0x07))
                ]));
            }
        }
    }
}
