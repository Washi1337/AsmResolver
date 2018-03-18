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
            Segments.Add(MethodBodyTable = new MethodBodyTableBuffer());

            if (assembly.NetDirectory.ResourcesManifest != null)
                Segments.Add(assembly.NetDirectory.ResourcesManifest);

            Segments.Add(FieldDataTable = new SimpleFileSegmentBuilder());
            Segments.Add(MetadataDirectory = new MetadataDirectoryBuffer(assembly.NetDirectory.MetadataHeader));

            if (assembly.DebugDirectory != null)
            {
                Segments.Add(DebugDirectory = assembly.DebugDirectory);
                Segments.Add(assembly.DebugDirectory.Data);
            }

            if (NetDirectory.StrongNameData != null)
                Segments.Add(NetDirectory.StrongNameData);

            if (NetDirectory.VTablesDirectory != null)
            {
                VTableFixups = new VTableFixupsBuffer(NetDirectory.VTablesDirectory);
                Segments.Add(VTableFixups.Directory);
                Segments.Add(VTableFixups.EntriesTable);
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
                    MethodBodyTable.Segments.Add(method.Column1);
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

        public MethodBodyTableBuffer MethodBodyTable
        {
            get;
            private set;
        }

        public SimpleFileSegmentBuilder FieldDataTable
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

        public VTableFixupsBuffer VTableFixups
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
            if (VTableFixups != null)
                VTableFixups.UpdateTableRvas(context);
            if (DebugDirectory != null)
            {
                DebugDirectory.PointerToRawData = (uint) DebugDirectory.Data.StartOffset;
                DebugDirectory.AddressOfRawData =
                    (uint) context.Builder.Assembly.FileOffsetToRva(DebugDirectory.PointerToRawData);
                DebugDirectory.SizeOfData = DebugDirectory.Data.GetPhysicalLength();
            }
           
            UpdateNetDirectory(context);
            base.UpdateReferences(context);
        }

        private void UpdateNetDirectory(EmitContext context)
        {
            var assembly = NetDirectory.Assembly;
            NetDirectory.MetadataDirectory.VirtualAddress = (uint) assembly.FileOffsetToRva(MetadataDirectory.StartOffset);
            NetDirectory.MetadataDirectory.Size = MetadataDirectory.GetPhysicalLength();

            if (NetDirectory.ResourcesManifest != null)
            {
                NetDirectory.ResourcesDirectory.VirtualAddress = (uint) assembly.FileOffsetToRva(NetDirectory.ResourcesManifest.StartOffset);
                NetDirectory.ResourcesDirectory.Size = NetDirectory.ResourcesManifest.GetPhysicalLength();
            }
            
            if (NetDirectory.StrongNameData != null)
            {
                NetDirectory.StrongNameSignatureDirectory.VirtualAddress =
                    (uint) assembly.FileOffsetToRva(NetDirectory.StrongNameData.StartOffset);
                NetDirectory.StrongNameSignatureDirectory.Size = NetDirectory.StrongNameData.GetPhysicalLength();
            }
            
            if (VTableFixups != null)
            {
                NetDirectory.VTableFixupsDirectory.VirtualAddress =
                    (uint) assembly.FileOffsetToRva(VTableFixups.Directory.StartOffset);
                NetDirectory.VTableFixupsDirectory.Size = VTableFixups.Directory.GetPhysicalLength();
            }
        }

    }
}
