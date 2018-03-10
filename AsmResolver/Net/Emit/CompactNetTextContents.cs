using AsmResolver.Emit;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Emit
{
    public class CompactNetTextContents : FileSegmentBuilder
    {
        private readonly SimpleFileSegmentBuilder _importDirectory = new SimpleFileSegmentBuilder();
        
        public CompactNetTextContents(WindowsAssembly assembly)
        {
            ImportBuffer = new ImportDirectoryBuffer(assembly);
            
            Segments.Add(ImportBuffer.AddressTables);
            Segments.Add(NetDirectory = assembly.NetDirectory);
            Segments.Add(MethodBodyTable = new RvaDataSegmentTableBuffer(assembly));
            
            if (assembly.NetDirectory.ResourcesManifest != null)
            {
                Segments.Add(ResourcesManifest = assembly.NetDirectory.ResourcesManifest);
            }

//            Segments.Add(FieldDataTable = new RvaDataSegmentTableBuffer(assembly));
            Segments.Add(MetadataDirectory = new MetadataDirectoryBuffer(assembly.NetDirectory.MetadataHeader));

            if (assembly.DebugDirectory != null)
            {
                Segments.Add(assembly.DebugDirectory);
                Segments.Add(assembly.DebugDirectory.Data);
            }
            
            _importDirectory.Segments.Add(ImportBuffer.ModuleImportTable);
            _importDirectory.Segments.Add(ImportBuffer.LookupTables);
            _importDirectory.Segments.Add(ImportBuffer.NameTable);
            Segments.Add(_importDirectory);

            Segments.Add(Bootstrapper = new BootstrapperSegment());

            foreach (var method in assembly.NetDirectory.MetadataHeader.GetStream<TableStream>()
                .GetTable<MethodDefinitionTable>())
            {
                if (method.Column1 != null)
                    MethodBodyTable.AddSegment(method.Column1);
            }
        }

        public ImportDirectoryBuffer ImportBuffer
        {
            get;
            private set;
        }

        public FileSegment ImportDirectory
        {
            get { return _importDirectory; }
        }

        public ImageNetDirectory NetDirectory
        {
            get;
            private set;
        }

        public RvaDataSegmentTableBuffer MethodBodyTable
        {
            get;
            private set;
        }

        public RvaDataSegmentTableBuffer FieldDataTable
        {
            get;
            private set;
        }

        public ResourcesManifest ResourcesManifest
        {
            get;
            private set;
        }

        public MetadataDirectoryBuffer MetadataDirectory
        {
            get;
            private set;
        }

        public ImageDebugDirectory DebugDirectory
        {
            get;
            private set;
        }

        public BootstrapperSegment Bootstrapper
        {
            get;
            private set;
        }

        public override void UpdateReferences(EmitContext context)
        {
            ImportBuffer.UpdateTableRvas();
            UpdateNetDirectory(context);
            base.UpdateReferences(context);
        }

        private void UpdateNetDirectory(EmitContext context)
        {
            var assembly = NetDirectory.Assembly;
            NetDirectory.MetadataDirectory.VirtualAddress = (uint) assembly.FileOffsetToRva(MetadataDirectory.StartOffset);
            NetDirectory.MetadataDirectory.Size = MetadataDirectory.GetPhysicalLength();

            if (ResourcesManifest != null)
            {
                NetDirectory.ResourcesDirectory.VirtualAddress = (uint) assembly.FileOffsetToRva(ResourcesManifest.StartOffset);
                NetDirectory.ResourcesDirectory.Size = ResourcesManifest.GetPhysicalLength();
            }
            
            if (NetDirectory.StrongNameData != null)
            {
                NetDirectory.StrongNameSignatureDirectory.VirtualAddress =
                    (uint) assembly.FileOffsetToRva(NetDirectory.StrongNameData.StartOffset);
                NetDirectory.StrongNameSignatureDirectory.Size = NetDirectory.StrongNameData.GetPhysicalLength();
            }
        }

    }
}
