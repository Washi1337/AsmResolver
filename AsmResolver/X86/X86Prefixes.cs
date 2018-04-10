using System.Collections.Generic;

namespace AsmResolver.X86
{
    public static class X86Prefixes
    {
        public static readonly IDictionary<byte, IList<X86Prefix>> PrefixesByByte = new Dictionary<byte, IList<X86Prefix>>();
        
        private static readonly X86OperandType[] SegmentOverrideOperands =
        {
            X86OperandType.DirectAddress,
            X86OperandType.MemoryAddress,
            X86OperandType.RegisterOrMemoryAddress
        };
        
        public static readonly X86Prefix EsOverride = new X86Prefix(0x26, SegmentOverrideOperands);
        public static readonly X86Prefix CsOverride = new X86Prefix(0x2E, SegmentOverrideOperands);
        public static readonly X86Prefix SsOverride = new X86Prefix(0x36, SegmentOverrideOperands);
        public static readonly X86Prefix DsOverride = new X86Prefix(0x3E, SegmentOverrideOperands);
        public static readonly X86Prefix FsOverride = new X86Prefix(0x64, SegmentOverrideOperands);
        public static readonly X86Prefix GsOverride = new X86Prefix(0x65, SegmentOverrideOperands);

        public static readonly X86Prefix OperandSizeOverride = new X86Prefix(0x66, X86OperandSize.WordOrDword);
        public static readonly X86Prefix AddressSizeOverride = new X86Prefix(0x66, X86OperandSize.WordOrDword);
     
        // 0x66 -> Wait 

        public static readonly X86Prefix Repnz = new X86Prefix(0xF2, 
            X86Mnemonic.Cmpsb, X86Mnemonic.Cmpsd,
            X86Mnemonic.Scasb, X86Mnemonic.Scasd);
        
        // 0xF2 -> ScalarDoublePrecision 
        
        public static readonly X86Prefix Repz = new X86Prefix(0xF3, 
            X86Mnemonic.Cmpsb, X86Mnemonic.Cmpsd,
            X86Mnemonic.Scasb, X86Mnemonic.Scasd);

        public static readonly X86Prefix Rep = new X86Prefix(0xF3, 
            X86Mnemonic.Ins, 
            X86Mnemonic.Lodsb, X86Mnemonic.Lodsd,
            X86Mnemonic.Movsb, X86Mnemonic.Movsd, X86Mnemonic.Movsx,
            X86Mnemonic.Outs, 
            X86Mnemonic.Stosb, X86Mnemonic.Stosd);
        
        // 0xF3 -> ScalarSinglePrecision

        public static readonly X86Prefix WaitPause = new X86Prefix(0xF3,
            X86Mnemonic.Pause);
    }
}