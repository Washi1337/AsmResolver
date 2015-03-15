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
            
            Segments.Add(_directory);
            Segments.Add(NetResourceDirectoryBuilder = new NetResourceDirectoryBuilder());
            Segments.Add(DataBuilder = new NetDataTableBuilder());
            // strongname
            Segments.Add(Metadata = new MetadataBuilder(directory.MetadataHeader));
            Segments.Add(MethodBodyTableBuilder = new MethodBodyTableBuilder());
            Segments.Add(new StartupCodeSegmentBuilder());
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

        public override void UpdateOffsets(BuildingContext context)
        {
            if (NetResourceDirectoryBuilder.Segments.Count == 0)
                Segments.Remove(NetResourceDirectoryBuilder);
            base.UpdateOffsets(context);
        }

        public override void UpdateReferences(BuildingContext context)
        {
            UpdateMetaDataDirectories();
            base.UpdateReferences(context);
        }

        private void UpdateMetaDataDirectories()
        {
            _directory.MetaDataDirectory.VirtualAddress =
                (uint)_directory.Assembly.FileOffsetToRva(Metadata.StartOffset);
            _directory.MetaDataDirectory.Size = Metadata.GetPhysicalLength();

            if (NetResourceDirectoryBuilder.Segments.Count > 0)
            {
                _directory.ResourcesDirectory.VirtualAddress =
                    (uint)_directory.Assembly.FileOffsetToRva(NetResourceDirectoryBuilder.StartOffset);
                _directory.ResourcesDirectory.Size = NetResourceDirectoryBuilder.GetPhysicalLength();
            }
        }
    }
}
