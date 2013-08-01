using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;

namespace TUP.AsmResolver.PE.Readers
{

    internal class PeHeaderReader
    {

        //internal Stream stream;
        internal UInt32 mainheadersignature;
        internal UInt32 ntHeadersSignature;
        internal Win32Assembly assembly;
        internal long ntheaderoffset;
        internal long fileheaderoffset;
        internal long optionalheaderoffset;
        internal uint optionalheadersig;
        
        PeImage image;

        internal Structures.IMAGE_DOS_HEADER dosHeader;
        internal Structures.IMAGE_FILE_HEADER fileHeader;

        internal Structures.IMAGE_OPTIONAL_HEADER32 optionalHeader32;

        internal Structures.IMAGE_OPTIONAL_HEADER64 optionalHeader64;

        internal IOptionalHeader optionalHeader;



        internal static PeHeaderReader FromAssembly(Win32Assembly assembly)
        {
            PeHeaderReader headerreader = new PeHeaderReader();
            headerreader.assembly = assembly;
            headerreader.image = assembly._peImage;
            return headerreader;
            
        }


        
        internal List<Section> sections = new List<Section>();
        internal List<DataDirectory> datadirectories = new List<DataDirectory>();

        internal void LoadData(bool ignoreHeaderNumbers)
        {

            CheckDosHeader();

            dosHeader = image.ReadStructure<Structures.IMAGE_DOS_HEADER>();

            ntheaderoffset = image.Stream.Position;

            image.Stream.Seek(dosHeader.e_lfanew, SeekOrigin.Begin);

            ntHeadersSignature = image.Reader.ReadUInt32();

            ReadFileHeader();
            ReadOptionalHeader();


            Dictionary<uint, Structures.IMAGE_DATA_DIRECTORY> rawdatadirs = ConstructDataDirectories(ignoreHeaderNumbers);

            ReadSections();

            CompleteDataDirectories(rawdatadirs);

        }

        private void CheckDosHeader()
        {
            mainheadersignature = image.Reader.ReadUInt16();
            image.Reader.BaseStream.Seek(0, SeekOrigin.Begin);

            if (((ImageSignature)mainheadersignature) != ImageSignature.DOS)
                throw new InvalidDataException("Assembly's signature is not recognized as a valid signature for an executable or library.");
        }
        private void ReadFileHeader()
        {

            fileheaderoffset = image.Stream.Position;
            fileHeader = image.ReadStructure<Structures.IMAGE_FILE_HEADER>();

        }
        private void ReadOptionalHeader()
        {
            optionalheaderoffset = image.Stream.Position;
            optionalheadersig = image.Reader.ReadUInt16();
            image.Stream.Seek(-2, SeekOrigin.Current);

            if (optionalheadersig == 0x10b)
            {
                optionalHeader32 = image.ReadStructure<Structures.IMAGE_OPTIONAL_HEADER32>();
                optionalHeader = OptionalHeader32.FromAssembly(assembly);
            }
            else
            {
                optionalHeader64 = image.ReadStructure<Structures.IMAGE_OPTIONAL_HEADER64>();
                optionalHeader = OptionalHeader64.FromAssembly(assembly);
            }

        }
        private Dictionary<uint, Structures.IMAGE_DATA_DIRECTORY> ConstructDataDirectories(bool ignoreHeaderNumbers)
        {
            Dictionary<uint, Structures.IMAGE_DATA_DIRECTORY> rawdatadirs = new Dictionary<uint, Structures.IMAGE_DATA_DIRECTORY>();

            for (int i = 0; i < (ignoreHeaderNumbers ? 0x10 : (optionalheadersig == 0x10b ? optionalHeader32.NumberOfRvaAndSizes : optionalHeader64.NumberOfRvaAndSizes)); i++)
            {
                uint byteoffset = (uint)image.Stream.Position;
                Structures.IMAGE_DATA_DIRECTORY datadir = image.ReadStructure<Structures.IMAGE_DATA_DIRECTORY>();
                rawdatadirs.Add(byteoffset, datadir);
            }
            return rawdatadirs;
        }

        private void ReadSections()
        {
            image.Stream.Seek(optionalheaderoffset + fileHeader.SizeOfOptionalHeader, SeekOrigin.Begin);

            for (int i = 0; i < fileHeader.NumberOfSections; i++)
            {
                uint byteoffset = (uint)image.Stream.Position;
                Structures.IMAGE_SECTION_HEADER section = image.ReadStructure<Structures.IMAGE_SECTION_HEADER>();
                Section s = new Section(assembly, byteoffset, section);
                sections.Add(s);
            }
        }

        private void CompleteDataDirectories(Dictionary<uint, Structures.IMAGE_DATA_DIRECTORY> rawdatadirs)
        {
            var keys = rawdatadirs.Keys.ToArray();
            var values = rawdatadirs.Values.ToArray();
            for (int i = 0;i < rawdatadirs.Count-1;i++)
            {
                DataDirectory datadir = new DataDirectory((DataDirectoryName)i, sections.ToArray(), keys[i], values[i]);
                datadirectories.Add(datadir);
            }

        }




        #region Properties
        
        // Gets if the file header is 32 bit or not
        public bool Is32BitHeader
        {
            get
            {
                //return true;
                return optionalheadersig == 0x10b;
               //UInt16 IMAGE_FILE_32BIT_MACHINE = 0x0100;
               //return (IMAGE_FILE_32BIT_MACHINE & FileHeader.Characteristics) == IMAGE_FILE_32BIT_MACHINE;
            }
        }

        // Gets the timestamp from the file header
        public DateTime TimeStamp
        {
            get
            {
                // Timestamp is a date offset from 1970
                DateTime returnValue = new DateTime(1970, 1, 1, 0, 0, 0);

                // Add in the number of seconds since 1970/1/1
                returnValue = returnValue.AddSeconds(fileHeader.TimeDateStamp);
                // Adjust to local timezone
                returnValue += TimeZone.CurrentTimeZone.GetUtcOffset(returnValue);

                return returnValue;
            }

        }

       /* public bool IsManagedAssembly
        {
            get
            {
                if (Is32BitHeader)
                {
                    return (optionalHeader32.DataDirectory.Size > 0);
                }
                else
                {
                    return (optionalHeader64.DataDirectory.Size > 0);
                }
       
            }
        }
        */
        #endregion Properties
    }
}

