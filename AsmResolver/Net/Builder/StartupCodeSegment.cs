namespace AsmResolver.Net.Builder
{
    public class StartupCodeSegment : FileSegment
    {
        public uint MainFunctionAddress
        {
            get;
            set;
        }

        public override uint GetPhysicalLength()
        {
            return 6;
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;

            writer.WriteUInt16(0x25FF); // jmp dword ptr [...]
            writer.WriteUInt32((uint)(MainFunctionAddress | context.Assembly.NtHeaders.OptionalHeader.ImageBase)); // _CorExeMain or _CorDllMain
        }
    }
}
