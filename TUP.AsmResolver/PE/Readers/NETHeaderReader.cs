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
        internal NTHeader header;
        internal Structures.IMAGE_COR20_HEADER netheader;
        internal Structures.METADATA_HEADER_1 metadataheader1;
        internal Structures.METADATA_HEADER_2 metadataheader2;
        internal string metadataversionstring;

        internal uint sectionfileoffset   ;
        internal uint virtualstartoffset;
        internal uint netheaderoffset;

        internal uint metadatafileoffset;
        internal uint metadatavirtualoffset;

        internal uint metadatastreamsoffset;

        internal uint metadataheaderlastoffset;

        PeImage image;
        OffsetConverter offsetConverter;

        internal List<MetaDataStream> metadatastreams = new List<MetaDataStream>();
        internal NETHeaderReader(NTHeader header, NETHeader parent)
        {
                header.assembly.netheader = parent;
                image = header.assembly.peImage;
                this.header = header;
            
        }

        public void LoadData()
        {

            if (header.IsManagedAssembly)
            {

                image.stream.Seek(header.OptionalHeader.DataDirectories[(int)DataDirectoryName.Clr].targetOffset.FileOffset, SeekOrigin.Begin);

                netheader = image.ReadStructure<Structures.IMAGE_COR20_HEADER>();


                uint netheaderoffset = header.OptionalHeader.DataDirectories[(int)DataDirectoryName.Clr].targetOffset.FileOffset;


                Section targetsection = Section.GetSectionByFileOffset(header.Sections, netheaderoffset);
                offsetConverter = new OffsetConverter(targetsection);


                sectionfileoffset = targetsection.RawOffset;
                virtualstartoffset = targetsection.RVA;

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
            header.assembly.netheader.DataDirectories = new DataDirectory[] {
             new DataDirectory(DataDirectoryName.NETMetadata, offsetConverter.TargetSection, 0, netheader.MetaData.RVA, netheader.MetaData.Size),
             new DataDirectory(DataDirectoryName.NETResource, offsetConverter.TargetSection, 0, netheader.Resources.RVA, netheader.Resources.Size),
             new DataDirectory(DataDirectoryName.NETStrongName, offsetConverter.TargetSection, 0, netheader.StrongNameSignature.RVA, netheader.StrongNameSignature.Size),
             new DataDirectory(DataDirectoryName.NETCodeManager, offsetConverter.TargetSection, 0, netheader.CodeManagerTable.RVA, netheader.CodeManagerTable.Size),
             new DataDirectory(DataDirectoryName.NETVTableFixups, offsetConverter.TargetSection, 0, netheader.VTableFixups.RVA, netheader.VTableFixups.Size),
             new DataDirectory(DataDirectoryName.NETExport, offsetConverter.TargetSection, 0, netheader.ExportAddressTableJumps.RVA, netheader.ExportAddressTableJumps.Size),
             new DataDirectory(DataDirectoryName.NETNativeHeader, offsetConverter.TargetSection, 0, netheader.ManagedNativeHeader.RVA, netheader.ManagedNativeHeader.Size),
            };

       }
        
        void LoadMetaData()
        {

            metadatavirtualoffset = netheader.MetaData.RVA;

            Section section = Section.GetSectionByRva(header.assembly, metadatavirtualoffset);

            metadatafileoffset = offsetConverter.RvaToFileOffset(metadatavirtualoffset);//= (uint)new CodeOffsetConverter(header.oheader).RVirtualToFileOffset((int)metadatavirtualoffset);

          

            metadataheader1 = header.assembly.peImage.ReadStructure<Structures.METADATA_HEADER_1>(metadatafileoffset);



            byte[] versionBytes = image.ReadBytes((int)metadatafileoffset + sizeof(Structures.METADATA_HEADER_1), (int)metadataheader1.VersionLength);
           // for (int i = 0; i < metadataheader1.VersionLength; i++)
           //     versionbytes.Add(header.assembly.peImage.ReadByte((int)metadatafileoffset + sizeof(Structures.METADATA_HEADER_1) + i));

            metadataversionstring = Encoding.ASCII.GetString(versionBytes).Trim();


            metadataheader2 = header.assembly.peImage.ReadStructure<Structures.METADATA_HEADER_2>((int)metadatafileoffset + sizeof(Structures.METADATA_HEADER_1) + metadataheader1.VersionLength);


            metadatastreamsoffset = (uint)metadatafileoffset + (uint)sizeof(Structures.METADATA_HEADER_1) + (uint)metadataheader1.VersionLength + (uint)sizeof(Structures.METADATA_HEADER_2);
            LoadMetaDataStreams();
        }


        void LoadMetaDataStreams()
        {
            int add = 0;
            for (int i = 0; i < metadataheader2.NumberOfStreams; i++)
            {
                long offset = metadatastreamsoffset + add + (i * 4);

                Structures.METADATA_STREAM_HEADER streamheader = header.assembly.peImage.ReadStructure<Structures.METADATA_STREAM_HEADER>(offset);

                long stringoffset = offset + 8;
                string name = header.assembly.peImage.ReadASCIIString(stringoffset).Replace("\0","");

                if (name.Length >= 8)
                    add += 16;
                else if (name.Length >= 4)
                    add += 12;
                else
                    add += 8;
            

                metadatastreams.Add(new MetaDataStream(header.assembly.netheader, this, (int)offset, (int)streamheader.Offset, (int)streamheader.Size, name));

                if (i == metadataheader2.NumberOfStreams -1)
                    metadataheaderlastoffset = (uint)offset + 16;

            }
            for (int i = 0; i < metadataheader2.NumberOfStreams; i++)
                metadatastreams[i].streamoffset = (int)metadatafileoffset + metadatastreams[i].offset;

           //for (int i = 0; i < metadataheader2.NumberOfStreams; i++)
           //{
           //    if (i == 0)
           //        metadatastreams[i].streamoffset = (int)metadataheaderlastoffset ;
           //    else
           //        metadatastreams[i].streamoffset = metadatastreams[i - 1].StreamOffset + metadatastreams[i - 1].StreamSize;
           //
            //}
        }
    }
}
