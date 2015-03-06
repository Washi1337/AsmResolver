using System;
using System.Linq;
using AsmResolver.Builder;

namespace AsmResolver.Net.Builder
{
    public class StartupCodeSegmentBuilder : FileSegmentBuilder
    {
        private readonly DataSegment _opCodeSegment = new DataSegment(new byte[]
        {
            0xFF, 0x25
        });

        private readonly DataSegment _addressSegment = new DataSegment(new byte[4]);
       
        public StartupCodeSegmentBuilder()
        {
            Segments.Add(_opCodeSegment);
            Segments.Add(_addressSegment);
        }

        public override void UpdateReferences(BuildingContext context)
        {   
            var isDll = context.Assembly.NtHeaders.FileHeader.Characteristics.HasFlag(ImageCharacteristics.Dll);

            var mscoreeModule = context.Assembly.ImportDirectory.ModuleImports.First(x => x.Name == "mscoree.dll");
            var corMainImport = mscoreeModule.SymbolImports.First(
                x => x.HintName != null && x.HintName.Name == (isDll ? "_CorDllMain" : "_CorExeMain"));
            var address = corMainImport.GetTargetAddress(true);

            context.Assembly.NtHeaders.OptionalHeader.AddressOfEntrypoint =
                (uint)context.Assembly.FileOffsetToRva(StartOffset);

            _addressSegment.Data = BitConverter.GetBytes((uint)(address | context.Assembly.NtHeaders.OptionalHeader.ImageBase));
            base.UpdateReferences(context);
        }

        //public uint MainFunctionAddress
        //{
        //    get;
        //    set;
        //}
        //
        //public override uint GetPhysicalLength()
        //{
        //    return 6;
        //}
        //
        //public override void Write(WritingContext context)
        //{
        //    var writer = context.Writer;
        //
        //    writer.WriteUInt16(0x25FF); // jmp dword ptr [...]
        //    writer.WriteUInt32((uint)(MainFunctionAddress | context.Assembly.NtHeaders.OptionalHeader.ImageBase)); // _CorExeMain or _CorDllMain
        //}
    }
}
