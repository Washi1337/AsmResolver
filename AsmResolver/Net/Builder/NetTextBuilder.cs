using System;
using System.Linq;
using AsmResolver.Builder;

namespace AsmResolver.Net.Builder
{
    public class NetTextBuilder : FileSegmentBuilder
    {
        private readonly ImageNetDirectory _directory;

        public NetTextBuilder(ImageNetDirectory directory)
        {
            _directory = directory;

            ImportBuilder = new ImageImportDirectoryBuilder(directory.Assembly, directory.Assembly.ImportDirectory);

            Segments.Add(ImportBuilder.AddressTablesBuilder);
            Segments.Add(directory);
            Segments.Add(MethodBodyTableBuilder = new MethodBodyTableBuilder());
            Segments.Add(NetResourceDirectoryBuilder = new NetResourceDirectoryBuilder());
            Segments.Add(DataBuilder = new NetDataTableBuilder());

            if (directory.StrongNameData != null)
                Segments.Add(directory.StrongNameData);

            Segments.Add(Metadata = new MetadataBuilder(directory.MetadataHeader));

            if (directory.Assembly.DebugDirectory != null)
            {
                Segments.Add(directory.Assembly.DebugDirectory);
                Segments.Add(directory.Assembly.DebugDirectory.Data);
            }

            Segments.Add(ImportBuilder);
            Segments.Add(StartupCode = new StartupCodeSegmentBuilder());
        }

        public MetadataBuilder Metadata
        {
            get;
            private set;
        }

        public NetResourceDirectoryBuilder NetResourceDirectoryBuilder
        {
            get;
            private set;
        }

        public MethodBodyTableBuilder MethodBodyTableBuilder
        {
            get;
            private set;
        }

        public NetDataTableBuilder DataBuilder
        {
            get;
            private set;
        }

        public ImageImportDirectoryBuilder ImportBuilder
        {
            get;
            private set;
        }

        public StartupCodeSegmentBuilder StartupCode
        {
            get;
            private set;
        }

        public override void Build(BuildingContext context)
        {
            foreach (var segment in Segments.OfType<FileSegmentBuilder>().Reverse())
                segment.Build(context);
        }

        public override void UpdateOffsets(BuildingContext context)
        {
            if (NetResourceDirectoryBuilder.Segments.Count == 0)
                Segments.Remove(NetResourceDirectoryBuilder);
            base.UpdateOffsets(context);
        }

        public override void UpdateReferences(BuildingContext context)
        {
            UpdateDebugDirectory();
            UpdateMetaDataDirectories();
            base.UpdateReferences(context);
        }

        private void UpdateDebugDirectory()
        {
            var assembly = _directory.Assembly;

            var debugDataDirectory =
                assembly.NtHeaders.OptionalHeader.DataDirectories[ImageDataDirectory.DebugDirectoryIndex];

            if (assembly.DebugDirectory == null)
            {
                debugDataDirectory.VirtualAddress = 0;
                debugDataDirectory.Size = 0;
            }
            else
            {
                debugDataDirectory.VirtualAddress = (uint)assembly.FileOffsetToRva(assembly.DebugDirectory.StartOffset);
                debugDataDirectory.Size = assembly.DebugDirectory.GetPhysicalLength();
                assembly.DebugDirectory.PointerToRawData = (uint)assembly.DebugDirectory.Data.StartOffset;
                assembly.DebugDirectory.AddressOfRawData =
                    (uint)assembly.FileOffsetToRva(assembly.DebugDirectory.Data.StartOffset);
            }
        }

        private void UpdateMetaDataDirectories()
        {
            _directory.MetadataDirectory.VirtualAddress =
                (uint)_directory.Assembly.FileOffsetToRva(Metadata.StartOffset);
            _directory.MetadataDirectory.Size = Metadata.GetPhysicalLength();

            if (_directory.StrongNameData != null)
            {
                _directory.StrongNameSignatureDirectory.VirtualAddress =
                    (uint) _directory.Assembly.FileOffsetToRva(_directory.StrongNameData.StartOffset);
                _directory.StrongNameSignatureDirectory.Size = _directory.StrongNameData.GetPhysicalLength();
            }

            if (NetResourceDirectoryBuilder.Segments.Count > 0)
            {
                _directory.ResourcesDirectory.VirtualAddress =
                    (uint)_directory.Assembly.FileOffsetToRva(NetResourceDirectoryBuilder.StartOffset);
                _directory.ResourcesDirectory.Size = NetResourceDirectoryBuilder.GetPhysicalLength();
            }
        }
    }
}
