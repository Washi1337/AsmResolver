using System;
using System.Linq;
using AsmResolver.Emit;

namespace AsmResolver.Net.Emit
{
    public class BootstrapperSegment : FileSegmentBuilder
    {
        private readonly DataSegment _opCodeSegment = new DataSegment(new byte[]
        {
            0xFF, 0x25
        });

        private readonly DataSegment _addressSegment = new DataSegment(new byte[4]);
       
        public BootstrapperSegment()
        {
            Segments.Add(_opCodeSegment);
            Segments.Add(_addressSegment);
        }

        public override void UpdateReferences(EmitContext context)
        {   
            bool isDll = context.Builder.Assembly.NtHeaders.FileHeader.Characteristics.HasFlag(ImageCharacteristics.Dll);

            var mscoreeModule = context.Builder.Assembly.ImportDirectory.ModuleImports.First(x => x.Name == "mscoree.dll");
            var corMainImport = mscoreeModule.SymbolImports.FirstOrDefault(
                x => x.HintName != null && x.HintName.Name == (isDll ? "_CorDllMain" : "_CorExeMain"));

            if (corMainImport == null)
                throw new ArgumentException(".NET bootstrapper requires a reference to mscoree.dll!_CorDllMain or mscoree.dll!_CorExeMain.");
            
            ulong address = corMainImport.GetTargetAddress(true);

            _addressSegment.Data = BitConverter.GetBytes((uint)(address | context.Builder.Assembly.NtHeaders.OptionalHeader.ImageBase));
            base.UpdateReferences(context);
        }
    }
}