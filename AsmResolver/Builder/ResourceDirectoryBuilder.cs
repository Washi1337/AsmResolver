using System;
using System.Collections.Generic;
using AsmResolver.Net.Builder;

namespace AsmResolver.Builder
{
    public class ResourceDirectoryBuilder : FileSegmentBuilder
    {
        private sealed class DirectoryTablesBuilder : FileSegmentBuilder
        {
            private readonly ImageDataDirectory _resourceDirectory;
            private readonly IOffsetConverter _offsetConverter;
            private readonly List<FileSegmentBuilder> _levelBuilders = new List<FileSegmentBuilder>();
            private readonly List<ImageResourceDirectory> _directories = new List<ImageResourceDirectory>(); 

            public DirectoryTablesBuilder(ImageDataDirectory resourceDirectory, IOffsetConverter offsetConverter)
            {
                _resourceDirectory = resourceDirectory;
                _offsetConverter = offsetConverter;
            }

            public void AddResourceDirectory(ImageResourceDirectory directory, int level)
            {
                while (_levelBuilders.Count <= level)
                {
                    _levelBuilders.Add(new FileSegmentBuilder());
                    Segments.Add(_levelBuilders[_levelBuilders.Count - 1]);
                }
                _levelBuilders[level].Segments.Add(directory);
                _directories.Add(directory);
            }

            public override void UpdateReferences(BuildingContext context)
            {
                foreach (var directory in _directories)
                    UpdateReferences(directory);
                base.UpdateReferences(context);
            }

            private void UpdateReferences(ImageResourceDirectory directory)
            {
                var resourcesFileOffset = _offsetConverter.RvaToFileOffset(_resourceDirectory.VirtualAddress);
                foreach (var entry in directory.Entries)
                {
                    if (entry.HasData)
                        entry.OffsetToData = (uint)(entry.DataEntry.StartOffset - resourcesFileOffset);
                    else
                        entry.OffsetToData = (uint)((entry.SubDirectory.StartOffset - resourcesFileOffset) | (1 << 31));
                }
            }
        }

        private sealed class DataDirectoryTablesBuilder : FileSegmentBuilder
        {
            private readonly List<ImageResourceDataEntry> _dataEntries = new List<ImageResourceDataEntry>();
            private readonly DataTableBuilder _dataTableBuilder;
            private readonly IOffsetConverter _offsetConverter;

            public DataDirectoryTablesBuilder(DataTableBuilder dataTableBuilder, IOffsetConverter offsetConverter)
            {
                _dataTableBuilder = dataTableBuilder;
                _offsetConverter = offsetConverter;
            }

            public void AddDataEntry(ImageResourceDataEntry entry)
            {
                _dataEntries.Add(entry);
                Segments.Add(entry);
            }

            public override void UpdateReferences(BuildingContext context)
            {
                base.UpdateReferences(context);
                foreach (var entry in _dataEntries)
                {
                    entry.OffsetToData =
                        (uint)_offsetConverter.FileOffsetToRva(_dataTableBuilder.GetDataSegment(entry).StartOffset);
                }
            }
        }

        private sealed class DataTableBuilder : FileSegmentBuilder
        {
            private readonly Dictionary<ImageResourceDataEntry, DataSegment> _dataMapping = new Dictionary<ImageResourceDataEntry, DataSegment>();
            
            public DataSegment GetDataSegment(ImageResourceDataEntry dataDirectory)
            {
                DataSegment segment;
                if (!_dataMapping.TryGetValue(dataDirectory, out segment))
                {
                    _dataMapping.Add(dataDirectory, segment = new DataSegment(dataDirectory.Data));
                    Segments.Add(segment);
                }
                return segment;
            }
        }

        private readonly IOffsetConverter _offsetConverter;
        private readonly ImageResourceDirectory _rootDirectory;
        private readonly DirectoryTablesBuilder _directoryTablesBuilder;
        private readonly DataDirectoryTablesBuilder _dataDirectoryTableBuilder;
        private readonly DataTableBuilder _dataTableBuilder = new DataTableBuilder();
        private readonly ImageDataDirectory _resourceDirectory;

        public ResourceDirectoryBuilder(NetAssemblyBuilder builder, IOffsetConverter offsetConverter, ImageResourceDirectory rootDirectory)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");
            if (offsetConverter == null)
                throw new ArgumentNullException("offsetConverter");
            if (rootDirectory == null)
                throw new ArgumentNullException("rootDirectory");

            _offsetConverter = offsetConverter;
            _rootDirectory = rootDirectory;

            _resourceDirectory = builder.Assembly.NtHeaders.OptionalHeader.DataDirectories[ImageDataDirectory.ResourceDirectoryIndex];

            Segments.Add(_directoryTablesBuilder = new DirectoryTablesBuilder(_resourceDirectory, offsetConverter));
            Segments.Add(_dataDirectoryTableBuilder = new DataDirectoryTablesBuilder(_dataTableBuilder, offsetConverter));
            Segments.Add(_dataTableBuilder);
        }

        public override void Build(BuildingContext context)
        {
            AddDirectory(_rootDirectory, 0);
            base.Build(context);
        }

        private void AddDirectory(ImageResourceDirectory directory, int level)
        {
            // TODO: add entry names
            _directoryTablesBuilder.AddResourceDirectory(directory, level);
            foreach (var entry in directory.Entries)
            {
                if (entry.HasData)
                {
                    _dataDirectoryTableBuilder.AddDataEntry(entry.DataEntry);
                    _dataTableBuilder.GetDataSegment(entry.DataEntry);
                }
                else
                {
                    AddDirectory(entry.SubDirectory, level + 1);
                }
            }
        }

        public override void UpdateReferences(BuildingContext context)
        {
            _resourceDirectory.VirtualAddress =
                (uint)_offsetConverter.FileOffsetToRva(_directoryTablesBuilder.StartOffset);
            _resourceDirectory.Size = GetPhysicalLength();
            base.UpdateReferences(context);
        }
    }
}
