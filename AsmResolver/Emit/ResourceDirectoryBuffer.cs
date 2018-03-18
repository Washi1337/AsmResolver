using System;
using System.Collections.Generic;
namespace AsmResolver.Emit
{
    public class ResourceDirectoryBuffer 
    {
        public sealed class DirectoryTableBuffer : FileSegmentBuilder
        {
            private readonly IOffsetConverter _offsetConverter;
            private readonly IList<SimpleFileSegmentBuilder> _levelBuilders = new List<SimpleFileSegmentBuilder>();
            private readonly IList<ImageResourceDirectory> _directories = new List<ImageResourceDirectory>();

            public DirectoryTableBuffer(IOffsetConverter offsetConverter)
            {
                _offsetConverter = offsetConverter;
            }

            public void AddResourceDirectory(ImageResourceDirectory directory, int level)
            {
                while (_levelBuilders.Count <= level)
                {
                    _levelBuilders.Add(new SimpleFileSegmentBuilder());
                    Segments.Add(_levelBuilders[_levelBuilders.Count - 1]);
                }
                _levelBuilders[level].Segments.Add(directory);
                _directories.Add(directory);
            }

            public override void UpdateReferences(EmitContext context)
            {
                foreach (var directory in _directories)
                {
                    UpdateReferences(directory);
                }
                base.UpdateReferences(context);
            }

            private void UpdateReferences(ImageResourceDirectory directory)
            {
                long resourcesFileOffset = StartOffset;
                foreach (var entry in directory.Entries)
                {
                    if (entry.HasData)
                        entry.OffsetToData = (uint)(entry.DataEntry.StartOffset - resourcesFileOffset);
                    else
                        entry.OffsetToData = (uint)((entry.SubDirectory.StartOffset - resourcesFileOffset) | (1 << 31));
                }
            }
        }

        public sealed class DataDirectoryTableBuffer : FileSegmentBuilder
        {
            private readonly List<ImageResourceDataEntry> _dataEntries = new List<ImageResourceDataEntry>();
            private readonly DataTableBuilder _dataTableBuilder;
            private readonly IOffsetConverter _offsetConverter;

            public DataDirectoryTableBuffer(DataTableBuilder dataTableBuilder, IOffsetConverter offsetConverter)
            {
                _dataTableBuilder = dataTableBuilder;
                _offsetConverter = offsetConverter;
            }

            public void AddDataEntry(ImageResourceDataEntry entry)
            {
                _dataEntries.Add(entry);
                Segments.Add(entry);
            }

            public override void UpdateReferences(EmitContext context)
            {
                base.UpdateReferences(context);
                foreach (var entry in _dataEntries)
                {
                    entry.OffsetToData =
                        (uint)_offsetConverter.FileOffsetToRva(_dataTableBuilder.GetDataSegment(entry).StartOffset);
                    entry.Size = (uint) entry.Data.Length;
                }
            }
        }

        public sealed class DataTableBuilder : FileSegmentBuilder
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
        
        public ResourceDirectoryBuffer(IOffsetConverter offsetConverter)
        {
            if (offsetConverter == null)
                throw new ArgumentNullException("offsetConverter");
            
            DirectoryTable = new DirectoryTableBuffer(offsetConverter);
            DataTable = new DataTableBuilder();
            DataDirectoryTable = new DataDirectoryTableBuffer(DataTable, offsetConverter);
        }

        public ResourceDirectoryBuffer(WindowsAssembly assembly)
            : this((IOffsetConverter) assembly)
        {
            if (assembly.RootResourceDirectory != null)
                AddDirectory(assembly.RootResourceDirectory, 0);
        }

        public DirectoryTableBuffer DirectoryTable
        {
            get;
            private set;
        }

        public DataDirectoryTableBuffer DataDirectoryTable
        {
            get;
            private set;
        }

        public DataTableBuilder DataTable
        {
            get;
            private set;
        }

        public void AddDirectory(ImageResourceDirectory directory, int level)
        {
            // TODO: add entry names
            DirectoryTable.AddResourceDirectory(directory, level);
            foreach (var entry in directory.Entries)
            {
                if (entry.HasData)
                {
                    DataDirectoryTable.AddDataEntry(entry.DataEntry);
                    DataTable.GetDataSegment(entry.DataEntry);
                }
                else
                {
                    AddDirectory(entry.SubDirectory, level + 1);
                }
            }
        }
    }
}
