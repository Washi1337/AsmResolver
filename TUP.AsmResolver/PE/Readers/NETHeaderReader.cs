using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using TUP.AsmResolver.NET;
namespace TUP.AsmResolver.PE.Readers
{
    internal unsafe class NETHeaderReader
    {
        internal NTHeader ntHeader;
        internal Structures.IMAGE_COR20_HEADER netHeader;
        internal Structures.METADATA_HEADER_1 metadataHeader1;
        internal Structures.METADATA_HEADER_2 metadataHeader2;
        internal string metadataVersionString;
        internal List<MetaDataStream> metadatastreams = new List<MetaDataStream>();
        internal uint metadataFileOffset;
        internal uint metadataRva;
        internal uint metadataStreamOffset;
        internal uint netHeaderOffset;

        PeImage image;
        OffsetConverter offsetConverter;

        internal NETHeaderReader(NTHeader header, NETHeader parent)
        {
                header.assembly.netHeader = parent;
                image = header.assembly.peImage;
                this.ntHeader = header;
            
        }

        public void LoadData()
        {

            if (ntHeader.IsManagedAssembly)
            {

                image.Stream.Seek(ntHeader.OptionalHeader.DataDirectories[(int)DataDirectoryName.Clr].targetOffset.FileOffset, SeekOrigin.Begin);

                netHeaderOffset = (uint)image.Position;
                netHeader = image.ReadStructure<Structures.IMAGE_COR20_HEADER>();

                Section targetsection = Section.GetSectionByFileOffset(ntHeader.Sections, netHeaderOffset);
                offsetConverter = new OffsetConverter(targetsection);

                LoadDirectories();

            }

        }

        void LoadDirectories()
        {
            ConstructDirectories();
            LoadMetaData();
        }

        void ConstructDirectories()
        {
            ntHeader.assembly.netHeader.DataDirectories = new DataDirectory[] {
             new DataDirectory(DataDirectoryName.NETMetadata, offsetConverter.TargetSection, 0, netHeader.MetaData),
             new DataDirectory(DataDirectoryName.NETResource, offsetConverter.TargetSection, 0, netHeader.Resources),
             new DataDirectory(DataDirectoryName.NETStrongName, offsetConverter.TargetSection, 0, netHeader.StrongNameSignature),
             new DataDirectory(DataDirectoryName.NETCodeManager, offsetConverter.TargetSection, 0, netHeader.CodeManagerTable),
             new DataDirectory(DataDirectoryName.NETVTableFixups, offsetConverter.TargetSection, 0, netHeader.VTableFixups),
             new DataDirectory(DataDirectoryName.NETExport, offsetConverter.TargetSection, 0, netHeader.ExportAddressTableJumps),
             new DataDirectory(DataDirectoryName.NETNativeHeader, offsetConverter.TargetSection, 0, netHeader.ManagedNativeHeader),
            };

       }
        
        void LoadMetaData()
        {

            metadataRva = netHeader.MetaData.RVA;
            Section section = Section.GetSectionByRva(ntHeader.assembly, metadataRva);
            offsetConverter = new OffsetConverter(section);
            metadataFileOffset = offsetConverter.RvaToFileOffset(metadataRva);//= (uint)new CodeOffsetConverter(header.oheader).RVirtualToFileOffset((int)metadatavirtualoffset);

            metadataHeader1 = ntHeader.assembly.peImage.ReadStructure<Structures.METADATA_HEADER_1>(metadataFileOffset);

            byte[] versionBytes = image.ReadBytes((int)metadataFileOffset + sizeof(Structures.METADATA_HEADER_1), (int)metadataHeader1.VersionLength);
            metadataVersionString = Encoding.ASCII.GetString(versionBytes).Trim();

            metadataHeader2 = ntHeader.assembly.peImage.ReadStructure<Structures.METADATA_HEADER_2>((int)metadataFileOffset + sizeof(Structures.METADATA_HEADER_1) + metadataHeader1.VersionLength);

            metadataStreamOffset = (uint)metadataFileOffset + (uint)sizeof(Structures.METADATA_HEADER_1) + (uint)metadataHeader1.VersionLength + (uint)sizeof(Structures.METADATA_HEADER_2);
            LoadMetaDataStreams();
        }

        void LoadMetaDataStreams()
        {
            int add = 0;
            for (int i = 0; i < metadataHeader2.NumberOfStreams; i++)
            {
                long offset = metadataStreamOffset + add + (i * 4);

                Structures.METADATA_STREAM_HEADER streamHeader = ntHeader.assembly.peImage.ReadStructure<Structures.METADATA_STREAM_HEADER>(offset);

                long stringOffset = offset + 8;
                string name = ntHeader.assembly.peImage.ReadASCIIString(stringOffset).Replace("\0","");

                if (name.Length >= 8)
                    add += 16;
                else if (name.Length >= 4)
                    add += 12;
                else
                    add += 8;
  
                metadatastreams.Add(GetHeap(ntHeader.assembly.netHeader, (int)offset, streamHeader, name));

            }

            foreach (MetaDataStream stream in metadatastreams)
                stream.Initialize();

        }

        private MetaDataStream GetHeap(NETHeader netheader, int headeroffset, Structures.METADATA_STREAM_HEADER rawHeader, string name)
        {
            switch (name)
            {
                case "#~":
                case "#-":
                    return new TablesHeap(netheader, headeroffset, rawHeader, name);
                case "#Strings":
                    return new StringsHeap(netheader, headeroffset, rawHeader, name);
                case "#US":
                    return new UserStringsHeap(netheader, headeroffset, rawHeader, name);
                case "#GUID":
                    return new GuidHeap(netheader, headeroffset, rawHeader, name);
                case "#Blob":
                    return new BlobHeap(netheader, headeroffset, rawHeader, name);
                default:
                    return new MetaDataStream(netheader, headeroffset, rawHeader, name);
            }

        }  
    
    }
}
