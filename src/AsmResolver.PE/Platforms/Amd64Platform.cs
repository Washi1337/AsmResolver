using AsmResolver.IO;
using AsmResolver.PE.Code;
using AsmResolver.PE.File;
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
        public override bool Is32Bit => false;

        /// <inheritdoc />
        public override uint ThunkStubAlignment => 1;

        /// <inheritdoc />
        public override RelocatableSegment CreateThunkStub(ISymbol entryPoint)
        {
            var segment = new DataSegment([
                    0x48, 0xA1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // rex.w rex.b mov rax, [&symbol]
                    0xFF, 0xE0                                                  // jmp [rax]
                ])
                .AsPatchedSegment()
                .Patch(2, AddressFixupType.Absolute64BitAddress, entryPoint);

            return new RelocatableSegment(segment, [
                new BaseRelocation(RelocationType.Dir64, segment.ToReference(2))
            ]);
        }

        /// <inheritdoc />
        public override bool TryExtractThunkAddress(PEImage image, BinaryStreamReader reader, out uint rva)
        {
            ushort opcode = reader.ReadUInt16();
            switch (opcode)
            {
                case 0xA148:
                    // 0x48, 0xA1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00  ; rex.w rex.b mov rax, [&symbol]
                    // 0xFF, 0xE0                                                  ; jmp [rax]
                    rva = (uint) (reader.ReadUInt64() - image.ImageBase);
                    return reader.ReadUInt16() == 0xE0FF;

                case 0x25FF:
                    // 0xFF, 0x25, 0x00, 0x00, 0x00, 0x00   ; jmp [rip+rel(&symbol)]
                    uint relative = reader.ReadUInt32();
                    rva = reader.Rva + relative;
                    return true;

                default:
                    rva = 0;
                    return false;
            }
        }

        /// <inheritdoc />
        public override AddressTableInitializerStub CreateAddressTableInitializer(ISymbol virtualProtect)
        {
            return new Amd64AddressTableInitializerStub(virtualProtect);
        }

        private sealed class Amd64AddressTableInitializerStub : AddressTableInitializerStub
        {
            private static readonly byte[] PrologueStub =
            [
                /* 00: */  0x48, 0x83, 0xFA, 0x01, //  cmp rdx,byte +0x1    ; dwReason == DLL_PROCESS_ATTACH
                /* 04: */  0x74, 0x01,             //  jz 0x7
                /* 06: */  0xC3                    //  ret
                /* 07: */
            ];

            private static readonly byte[] EpilogueStub =
            [
                /* 00: */  0xC3              // ret
            ];

            private static readonly byte[] SlotInitializerCode =
            [
                /* 00: */  0x55,                                                         // push rbp
                /* 01: */  0x48, 0x89, 0xE5,                                             // mov rbp,rsp
                /* 04: */  0x48, 0x83, 0xEC, 0x40,                                       // sub rsp,byte +0x40

                /* 08: */  0x48, 0x89, 0x55, 0xF0,                                       // mov [rbp-0x10],rdx          ; val
                /* 0C: */  0x48, 0x89, 0x4D, 0xF8,                                       // mov [rbp-0x8],rcx           ; dest

                /* 10: */  0x4C, 0x8D, 0x4D, 0xE8,                                       // lea r9,[rbp-0x18]           ; &old
                /* 14: */  0x41, 0xB8, 0x04, 0x00, 0x00, 0x00,                           // mov r8d,0x4                 ; PAGE_READWRITE
                /* 1A: */  0xBA, 0x08, 0x00, 0x00, 0x00,                                 // mov edx,0x8                 ; size
                /* 1F: */  0x48, 0x8B, 0x4D, 0xF8,                                       // mov rcx,[rbp-0x8]           ; addr
                /* 23: */  0x48, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,   // mov rax, &VirtualProtect    ; &VirtualProtect
                /* 2D: */  0xFF, 0x10,                                                   // call [rax]

                /* 2F: */  0x48, 0x8B, 0x45, 0xF8,                                       // mov rax,[rbp-0x8]           ; *dest = val
                /* 33: */  0x48, 0x8B, 0x4D, 0xF0,                                       // mov rcx,[rbp-0x10]
                /* 37: */  0x48, 0x89, 0x08,                                             // mov [rax],rcx

                /* 3A: */  0x4C, 0x8D, 0x4D, 0xE8,                                       // lea r9,[rbp-0x18]           ; &old
                /* 3E: */  0x45, 0x8B, 0x01,                                             // mov r8d,[r9]                ; *(&old)
                /* 41: */  0xBA, 0x08, 0x00, 0x00, 0x00,                                 // mov edx,0x8                 ; size
                /* 46: */  0x48, 0x8B, 0x4D, 0xF8,                                       // mov rcx,[rbp-0x8]           ; addr
                /* 4A: */  0x48, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,   // mov rax, &VirtualProtect    ; &VirtualProtect
                /* 54: */  0xFF, 0x10,                                                   // call [rax]

                /* 56: */  0x48, 0x89, 0xEC,                                             // mov rsp,rbp
                /* 59: */  0x5D,                                                         // pop rbp
                /* 5A: */  0xC3 // ret
            ];

            public Amd64AddressTableInitializerStub(ISymbol virtualProtect)
                : base(new DataSegment(PrologueStub), new DataSegment(EpilogueStub), CreateSlotInitializer(virtualProtect))
            {
            }

            private static RelocatableSegment CreateSlotInitializer(ISymbol virtualProtect)
            {
                var code = new DataSegment(SlotInitializerCode).AsPatchedSegment()
                    .Patch(0x25, AddressFixupType.Absolute64BitAddress, virtualProtect)
                    .Patch(0x4C, AddressFixupType.Absolute64BitAddress, virtualProtect);

                return new RelocatableSegment(code, [
                    new(RelocationType.Dir64, code.ToReference(0x25)),
                    new(RelocationType.Dir64, code.ToReference(0x4C))
                ]);
            }

            public override void AddInitializer(ISymbol originalSlot, ISymbol newSlot)
            {
                var code = new DataSegment([
                        /* 00: */ 0x48, 0xA1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // rex.w rex.b mov rax, [&VirtualProtect]
                        /* 0A: */ 0x48, 0x8B, 0xD0,                                           // mov rdx, rax
                        /* 0D: */ 0x48, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // mov rcx, &dest
                        /* 17: */ 0xE8, 0x00, 0x00, 0x00, 0x00 // call init_slot
                    ]).AsPatchedSegment()
                    .Patch(0x02, AddressFixupType.Absolute64BitAddress, newSlot)
                    .Patch(0x0F, AddressFixupType.Absolute64BitAddress, originalSlot)
                    .Patch(0x18, AddressFixupType.Relative32BitAddress, SlotInitializer);

                Body.Add(new RelocatableSegment(code, new BaseRelocation[]
                {
                    new(RelocationType.Dir64, code.ToReference(0x02)),
                    new(RelocationType.Dir64, code.ToReference(0x0F)),
                }));
            }
        }

    }
}
