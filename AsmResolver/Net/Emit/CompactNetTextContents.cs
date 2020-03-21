using AsmResolver.Emit;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Emit
{
    /// <summary>
    /// Represents a structure of the .text section of a typical .NET assembly.  
    /// </summary>
    public class CompactNetTextContents : FileSegmentBuilder
    {
        private readonly SimpleFileSegmentBuilder _importDirectory;
        
        public CompactNetTextContents(WindowsAssembly assembly)
        {
            if (assembly.NtHeaders.OptionalHeader.Magic == OptionalHeaderMagic.Pe32)
            {
                ImportBuffer = new ImportDirectoryBuffer(assembly);
                _importDirectory = new SimpleFileSegmentBuilder();
                
                // IAT
                Segments.Add(ImportBuffer.AddressTables);
            }

            // .NET Directory
            Segments.Add(NetDirectory = assembly.NetDirectory);
            
            // Method bodies.
            Segments.Add(MethodBodyTable = new MethodBodyTableBuffer());

            // Manifest resource data.
            if (assembly.NetDirectory.ResourcesManifest != null)
                Segments.Add(assembly.NetDirectory.ResourcesManifest);

            // Field data (FieldRVAs).
            Segments.Add(FieldDataTable = new SimpleFileSegmentBuilder());
            
            // Metadata directory header.
            Segments.Add(MetadataDirectory = new MetadataDirectoryBuffer(assembly.NetDirectory.MetadataHeader));

            // Debug directory.
            if (assembly.DebugDirectory != null)
            {
                Segments.Add(DebugDirectory = assembly.DebugDirectory);
                Segments.Add(assembly.DebugDirectory.Data);
            }

            // Strong name.
            if (NetDirectory.StrongNameData != null)
                Segments.Add(NetDirectory.StrongNameData);

            // VTables.
            if (NetDirectory.VTablesDirectory != null)
            {
                VTableFixups = new VTableFixupsBuffer(NetDirectory.VTablesDirectory);
                Segments.Add(VTableFixups.Directory);
                Segments.Add(VTableFixups.EntriesTable);
            }

            // Export directory.
            if (assembly.ExportDirectory != null)
                Segments.Add(ExportDirectory = new ExportDirectoryBuffer(assembly.ExportDirectory, assembly));

            if (ImportBuffer != null)
            {
                // Remaining bits of the import tables.
                _importDirectory.Segments.Add(ImportBuffer.ModuleImportTable);
                _importDirectory.Segments.Add(ImportBuffer.LookupTables);
                _importDirectory.Segments.Add(ImportBuffer.NameTable);
                Segments.Add(_importDirectory);
            }

            if (assembly.NtHeaders.OptionalHeader.Magic == OptionalHeaderMagic.Pe32)
            {
                // Bootstrapper (Call to _CorExeMain / _CorDllMain).
                Segments.Add(Bootstrapper = new BootstrapperSegment());
            }

            var tableStream = assembly.NetDirectory.MetadataHeader.GetStream<TableStream>();
            
            // Initialize field data segment.
            foreach (var fieldRva in tableStream.GetTable<FieldRvaTable>())
            {
                if (fieldRva.Column1 != null)
                    FieldDataTable.Segments.Add(fieldRva.Column1);
            }
            
            // Initialize method body table.
            foreach (var method in tableStream.GetTable<MethodDefinitionTable>())
            {
                if (method.Column1 != null)
                    MethodBodyTable.Segments.Add(method.Column1);
            }
        }

        public ImportDirectoryBuffer ImportBuffer
        {
            get;
        }

        public FileSegment ImportDirectory => _importDirectory;

        public ExportDirectoryBuffer ExportDirectory
        {
            get;
        }

        public ImageNetDirectory NetDirectory
        {
            get;
        }

        public MethodBodyTableBuffer MethodBodyTable
        {
            get;
        }

        public SimpleFileSegmentBuilder FieldDataTable
        {
            get;
        }

        public MetadataDirectoryBuffer MetadataDirectory
        {
            get;
        }

        public ImageDebugDirectory DebugDirectory
        {
            get;
        }

        public VTableFixupsBuffer VTableFixups
        {
            get;
        }

        public BootstrapperSegment Bootstrapper
        {
            get;
        }

        public override void UpdateReferences(EmitContext context)
        {
            ImportBuffer?.UpdateTableRvas();
            VTableFixups?.UpdateTableRvas(context);
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
