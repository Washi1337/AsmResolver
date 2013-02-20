using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;
using System.IO;
using System.Drawing;

namespace TUP.AsmResolver.PE
{
    internal class ResourcesReader
    {
        NTHeader header;
        PeImage image;
        internal ResourceDirectory rootDirectory;
        DataDirectory resourceDirectory;
        internal ResourcesReader(NTHeader header)
        {
            this.header = header;
            image = header.assembly.peImage;
            resourceDirectory = header.OptionalHeader.DataDirectories[(int)DataDirectoryName.Resource];
            ReadRootDirectory();
        }

        internal void ReadRootDirectory()
        {
            image.SetOffset(resourceDirectory.TargetOffset.FileOffset);
            rootDirectory = ReadDirectory(null);
        }

        internal ResourceDirectoryEntry ReadDirectoryEntry()
        {
            uint offset = (uint)image.Position;
            var rawEntry = image.ReadStructure<Structures.IMAGE_RESOURCE_DIRECTORY_ENTRY>();
            string customName = string.Empty;
            ResourceDirectoryEntry resourceEntry = new ResourceDirectoryEntry(image, offset, rawEntry, customName); 

            return resourceEntry;
        }

        internal ResourceDirectory ReadDirectory(ResourceDirectoryEntry entry)
        {
            uint offset = (uint)image.Position;
            var rawDirectory = image.ReadStructure<Structures.IMAGE_RESOURCE_DIRECTORY>();
            
            return new ResourceDirectory(image, offset, entry, ReadChildEntries(rawDirectory.NumberOfIdEntries +  rawDirectory.NumberOfNamedEntries), rawDirectory);
        }
        internal ResourceDirectoryEntry[] ReadChildEntries(int count)
        {
            ResourceDirectoryEntry[] entries = ConstructChildEntries(count);
            FillChildEntries(ref entries);
            return entries;
        }
        internal ResourceDirectoryEntry[] ConstructChildEntries(int count)
        {
            ResourceDirectoryEntry[] entries = new ResourceDirectoryEntry[count];
            for (int i = 0; i < count; i++)
                entries[i] = ReadDirectoryEntry();
            return entries;
        }
        internal void FillChildEntries(ref ResourceDirectoryEntry[] entries)
        {
            for (int i = 0; i < entries.Length; i++)
            {

                if (!entries[i].IsEntryToData)
                {
                    image.SetOffset(resourceDirectory.TargetOffset.FileOffset + entries[i].OffsetToData - 0x80000000);
                    entries[i].Directory = ReadDirectory(entries[i]);
                }
                else
                {
                    image.SetOffset(resourceDirectory.TargetOffset.FileOffset + entries[i].OffsetToData);
                    entries[i].DataEntry = ReadDataEntry(entries[i]);
                }
            }
        }
        internal ResourceDataEntry ReadDataEntry(ResourceDirectoryEntry entry)
        {
            uint offset = (uint)image.Position;
            var rawDataEntry = image.ReadStructure<Structures.IMAGE_RESOURCE_DATA_ENTRY>();
            return new ResourceDataEntry(image, offset, entry, rawDataEntry);
        }

       // internal string GetEntryName() { }
        //internal void LoadResources()
        //{
        //
        //    image.stream.Seek(header.OptionalHeader.DataDirectories[(int)DataDirectoryName.Resource].targetOffset.FileOffset, SeekOrigin.Begin);
        //
        //
        //    startfileoffset = (uint)image.stream.Position;
        //    rootDirectory =image.ReadStructure<Structures.IMAGE_RESOURCE_DIRECTORY>();
        //
        //
        //
        //
        //    targetsection = Section.GetSectionByRawOffset(header.Sections, (int)header.OptionalHeader.DataDirectories[(int)DataDirectoryName.Clr].targetOffset.FileOffset);
        //
        //    if (targetsection != null)
        //    {
        //
        //
        //        // stream.Seek(startfileoffset, SeekOrigin.Begin);
        //
        //        for (int i = 0; i < rootDirectory.NumberOfNamedEntries; i++)
        //        {
        //            Structures.IMAGE_RESOURCE_DIRECTORY_ENTRY root = image.ReadStructure<Structures.IMAGE_RESOURCE_DIRECTORY_ENTRY>();
        //
        //            ReadEntry(image.reader, root);
        //        }
        //        for (int i = 0; i < rootDirectory.NumberOfIdEntries; i++)
        //        {
        //            Structures.IMAGE_RESOURCE_DIRECTORY_ENTRY root = image.ReadStructure<Structures.IMAGE_RESOURCE_DIRECTORY_ENTRY>();
        //
        //            ReadEntry(image.reader, root);
        //        }
        //    }
        //
        //}
        //
        //
        //internal void ReadEntry(BinaryReader reader, Structures.IMAGE_RESOURCE_DIRECTORY_ENTRY entry)
        //{
        //    if (entry.OffsetToData == 0)
        //    {
        //        Resources.Add(new ResourceDirectory("INVALID - " + entry.Name.ToString(), entry.Name, null));
        //        return;
        //    }
        //    Stream stream = reader.BaseStream;
        //    long toffset1 = stream.Position;
        //    uint offset1 = startfileoffset + entry.OffsetToData;
        //    if (offset1 > 0x80000000)
        //        offset1 -= 0x80000000;
        //
        //    //inner directory
        //    stream.Seek(offset1, SeekOrigin.Begin);
        //    Structures.IMAGE_RESOURCE_DIRECTORY directory = CommonAPIs.FromBinaryReader<Structures.IMAGE_RESOURCE_DIRECTORY>(reader);
        //    //Console.WriteLine("    - DIRECTORY (ENTRIES: " + directory.NumberOfIdEntries.ToString() + ")");
        //    
        //  
        //
        //
        //
        //    for (int ii = 0; ii < directory.NumberOfNamedEntries; ii++)
        //    {
        //        Structures.IMAGE_RESOURCE_DIRECTORY_ENTRY innerentry = CommonAPIs.FromBinaryReader<Structures.IMAGE_RESOURCE_DIRECTORY_ENTRY>(reader);
        //        #region Name
        //        string name = innerentry.Name.ToString();
        //        if (innerentry.Name > 0x80000000)
        //            {
        //                long tempoffs = stream.Position;
        //                stream.Seek(innerentry.Name - 0x80000000 + startfileoffset, SeekOrigin.Begin);
        //                ////Console.WriteLine("READ POINT: " + stream.Position.ToString("X8"));
        //                ushort namelength = reader.ReadUInt16();
        //                StringBuilder builder = new StringBuilder();
        //                for (int iii = 0; iii < namelength; iii++)
        //                {
        //                    builder.Append(((char)reader.ReadInt16()).ToString());
        //                }
        //                //Console.WriteLine(" - " + builder.ToString());
        //                name = builder.ToString();
        //                stream.Seek(tempoffs, SeekOrigin.Begin);
        //        
        //            }
        //            else
        //            {
        //                //Console.WriteLine(" - " + innerentry.Name.ToString());
        //               // stream.Seek(startfileoffset + (innerentry.OffsetToData), SeekOrigin.Begin);
        //            }
        //        #endregion
        //        //Console.WriteLine(" - " + innerentry.Name.ToString("X8"));
        //        long toffset = stream.Position;
        //        uint offset = startfileoffset + innerentry.OffsetToData;
        //        if (offset > 0x80000000)
        //            offset -= 0x80000000;
        //
        //        stream.Seek(offset, SeekOrigin.Begin);
        //        Structures.IMAGE_RESOURCE_DIRECTORY innerdirectory = CommonAPIs.FromBinaryReader<Structures.IMAGE_RESOURCE_DIRECTORY>(reader);
        //        //Console.WriteLine("    - DIRECTORY (ENTRIES: " + innerdirectory.NumberOfIdEntries.ToString() + ")");
        //
        //        Resources.Add(new ResourceDirectory(name, innerentry.Name, ReadInnerDirectory(reader, innerdirectory)));
        //
        //       
        //        stream.Seek(toffset, SeekOrigin.Begin);
        //
        //
        //    }
        //
        //
        //    for (int ii = 0; ii < directory.NumberOfIdEntries; ii++)
        //    {
        //        Structures.IMAGE_RESOURCE_DIRECTORY_ENTRY innerentry = CommonAPIs.FromBinaryReader<Structures.IMAGE_RESOURCE_DIRECTORY_ENTRY>(reader);
        //        //Console.WriteLine(" - " + innerentry.Name.ToString());
        //
        //        long toffset = stream.Position;
        //        uint offset = startfileoffset + innerentry.OffsetToData;
        //        if (offset > 0x80000000)
        //            offset -= 0x80000000;
        //
        //        stream.Seek(offset, SeekOrigin.Begin);
        //        Structures.IMAGE_RESOURCE_DIRECTORY innerdirectory = CommonAPIs.FromBinaryReader<Structures.IMAGE_RESOURCE_DIRECTORY>(reader);
        //        //Console.WriteLine("    - DIRECTORY (ENTRIES: " + innerdirectory.NumberOfIdEntries.ToString() + ")");
        //
        //        Resources.Add(new ResourceDirectory(((ResourceDirectoryType)entry.Name).ToString().Replace("_", " "), innerentry.Name, ReadInnerDirectory(reader, innerdirectory)));
        //
        //        stream.Seek(toffset, SeekOrigin.Begin);
        //    }
        //    // //Console.WriteLine(directory.NumberOfIdEntries.ToString() + " " + directory.NumberOfNamedEntries.ToString());
        //
        //    stream.Seek(toffset1, SeekOrigin.Begin);
        //     
        //
        //}
        //
        //internal ResourceData[] ReadInnerDirectory(BinaryReader reader, Structures.IMAGE_RESOURCE_DIRECTORY directory)
        //{
        //    List<ResourceData> data = new List<ResourceData>();
        //    for (int ii = 0; ii < directory.NumberOfNamedEntries; ii++)
        //    {
        //        Structures.IMAGE_RESOURCE_DIRECTORY_ENTRY entry = CommonAPIs.FromBinaryReader<Structures.IMAGE_RESOURCE_DIRECTORY_ENTRY>(reader);
        //        ResourceData res = ReadDataFromEntry(reader, entry);
        //
        //        if (entry.Name > 0x80000000)
        //        {
        //            long tempoffs = reader.BaseStream.Position;
        //            reader.BaseStream.Seek(entry.Name - 0x80000000 + startfileoffset, SeekOrigin.Begin);
        //            ////Console.WriteLine("READ POINT: " + stream.Position.ToString("X8"));
        //            ushort namelength = reader.ReadUInt16();
        //            StringBuilder builder = new StringBuilder();
        //            for (int iii = 0; iii < namelength; iii++)
        //            {
        //                builder.Append(((char)reader.ReadInt16()).ToString());
        //            }
        //            //Console.WriteLine(" - " + builder.ToString());
        //            res.name = builder.ToString();
        //            reader.BaseStream.Seek(tempoffs, SeekOrigin.Begin);
        //
        //        }
        //    
        //
        //        data.Add(res);
        //        
        //    }
        //    for (int ii = 0; ii < directory.NumberOfIdEntries; ii++)
        //    {
        //        data.Add(ReadDataFromEntry(reader, CommonAPIs.FromBinaryReader<Structures.IMAGE_RESOURCE_DIRECTORY_ENTRY>(reader)));
        //    }
        //    return data.ToArray();
        //}
        //
        //
        //internal ResourceData ReadDataFromEntry(BinaryReader reader, Structures.IMAGE_RESOURCE_DIRECTORY_ENTRY entry)
        //{
        //    long tempoffset = reader.BaseStream.Position;
        //
        //    long offset = entry.OffsetToData + startfileoffset;
        //    if (offset > 0x80000000)
        //        offset -= 0x80000000;
        //    reader.BaseStream.Seek(offset, SeekOrigin.Begin);
        //
        //    Structures.IMAGE_RESOURCE_DATA_ENTRY dir = CommonAPIs.FromBinaryReader<Structures.IMAGE_RESOURCE_DATA_ENTRY>(reader);
        //
        //    uint fileoffset = dir.OffsetToData - targetsection.RVA + targetsection.RawOffset;
        //
        //    reader.BaseStream.Seek(fileoffset, SeekOrigin.Begin);
        //    return new ResourceData(entry.Name.ToString(), reader.ReadBytes((int)dir.Size));
        //}




    }
}
