using System;
using System.Linq;
using AsmResolver.Builder;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Msil;

namespace AsmResolver.Net.Builder
{
    public class NetTextBuilder : FileSegmentBuilder
    {
        private sealed class MethodBodyTableBuilder : FileSegmentBuilder
        {
            public override void Build(BuildingContext context)
            {
                foreach (var method in context.Assembly.NetDirectory.MetadataHeader.GetStream<TableStream>().GetTable<MethodDefinition>())
                {
                    if (method.MethodBody != null)
                        Segments.Add(method.MethodBody);
                }
                base.Build(context);
            }

            public override void UpdateOffsets(BuildingContext context)
            {
                for (int i = 0; i < Segments.Count; i++)
                {
                    if (i == 0)
                        Segments[i].StartOffset = StartOffset;
                    else
                        Segments[i].StartOffset = Segments[i - 1].StartOffset + Segments[i - 1].GetPhysicalLength();

                    var methodBody = Segments[i] as MethodBody;
                    if (methodBody != null && methodBody.IsFat)
                        methodBody.StartOffset = Align((uint)methodBody.StartOffset, 4);
                }
            }
        }

        private readonly ImageNetDirectory _directory;
        private readonly MethodBodyTableBuilder _methodBodyTableBuilder;
        private readonly FileSegmentBuilder _resourceDirectoryBuilder;
        private readonly StartupCodeSegment _startupCode;

        public NetTextBuilder(ImageNetDirectory directory)
        {
            _directory = directory;
            
            Segments.Add(_directory);
            Segments.Add(_resourceDirectoryBuilder = new FileSegmentBuilder());
            // data
            // strongname
            Segments.Add(MetadataBuilder = new MetadataBuilder(directory.MetadataHeader));
            Segments.Add(_methodBodyTableBuilder = new MethodBodyTableBuilder());
            Segments.Add(_startupCode = new StartupCodeSegment());
        }

        public MetadataBuilder MetadataBuilder
        {
            get;
            private set;
        }

        public uint AppendResourceData(byte[] data)
        {
            var offset = _resourceDirectoryBuilder.GetPhysicalLength();
            _resourceDirectoryBuilder.Segments.Add(new DataSegment(BitConverter.GetBytes(data.Length)));
            _resourceDirectoryBuilder.Segments.Add(new DataSegment(data));
            return offset;
        }

        public override void UpdateOffsets(BuildingContext context)
        {
            if (_resourceDirectoryBuilder.Segments.Count == 0)
                Segments.Remove(_resourceDirectoryBuilder);
            base.UpdateOffsets(context);
        }

        public override void UpdateReferences(BuildingContext context)
        {
            base.UpdateReferences(context);

            UpdateStartupCode();
            UpdateMetaDataDirectories();
        }

        private void UpdateMetaDataDirectories()
        {
            _directory.MetaDataDirectory.VirtualAddress =
                (uint)_directory.Assembly.FileOffsetToRva(MetadataBuilder.StartOffset);
            _directory.MetaDataDirectory.Size = MetadataBuilder.GetPhysicalLength();

            if (_resourceDirectoryBuilder.Segments.Count > 0)
            {
                _directory.ResourcesDirectory.VirtualAddress =
                    (uint)_directory.Assembly.FileOffsetToRva(_resourceDirectoryBuilder.StartOffset);
                _directory.ResourcesDirectory.Size = _resourceDirectoryBuilder.GetPhysicalLength();
            }
        }

        private void UpdateStartupCode()
        {
            var isDll = _directory.Assembly.NtHeaders.FileHeader.Characteristics.HasFlag(ImageCharacteristics.Dll);

            _startupCode.MainFunctionAddress = (uint)_directory.Assembly.ImportDirectory.ModuleImports.First(
                x => x.Name == "mscoree.dll").SymbolImports.First(
                    x => x.HintName != null && x.HintName.Name == (isDll ? "_CorDllMain" : "_CorExeMain"))
                .GetTargetAddress(true);

            _directory.Assembly.NtHeaders.OptionalHeader.AddressOfEntrypoint =
                (uint)_directory.Assembly.FileOffsetToRva(_startupCode.StartOffset);
        }
    }

}
