using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;

#pragma warning disable 1591

namespace TUP.AsmResolver.PE
{
    internal class Structures
    {
        static Structures()
        {
            Structures.InitializeDataOffsetsFor(typeof(Structures.IMAGE_FILE_HEADER));
            Structures.InitializeDataOffsetsFor(typeof(Structures.IMAGE_OPTIONAL_HEADER32));
            Structures.InitializeDataOffsetsFor(typeof(Structures.IMAGE_OPTIONAL_HEADER64));
            Structures.InitializeDataOffsetsFor(typeof(Structures.IMAGE_SECTION_HEADER));
            Structures.InitializeDataOffsetsFor(typeof(Structures.IMAGE_COR20_HEADER));
            Structures.InitializeDataOffsetsFor(typeof(Structures.METADATA_HEADER_1));
            Structures.InitializeDataOffsetsFor(typeof(Structures.METADATA_HEADER_2));
            Structures.InitializeDataOffsetsFor(typeof(Structures.METADATA_STREAM_HEADER));
            Structures.InitializeDataOffsetsFor(typeof(Structures.METADATA_TABLE_HEADER));
            Structures.InitializeDataOffsetsFor(typeof(Structures.IMAGE_RESOURCE_DATA_ENTRY));
            Structures.InitializeDataOffsetsFor(typeof(Structures.IMAGE_RESOURCE_DIRECTORY));
            Structures.InitializeDataOffsetsFor(typeof(Structures.IMAGE_RESOURCE_DIRECTORY_ENTRY));
        }

        public static Dictionary<Type, int[]> DataOffsets = new Dictionary<Type, int[]>();

        public static void InitializeDataOffsetsFor(Type T)
        {
            int offset = 0;
            string name = T.Name;
            

            FieldInfo[] fields = T.GetFields();
            int[] offsets = new int[fields.Length];
            for (int i = 0; i < fields.Length;i++ )
            {
                offsets[i] = offset;
                FieldInfo nfo = fields[i];
                if (T.FullName == typeof(Structures.IMAGE_SECTION_HEADER).FullName & nfo.FieldType.FullName == typeof(string).FullName)
                    offset += 8;
                else
                    offset += Marshal.SizeOf(nfo.FieldType);
            }

            DataOffsets.Add(T, offsets);

        }


        [StructLayout(LayoutKind.Sequential)]
        public struct LOADED_IMAGE
        {
            public IntPtr moduleName;
            public IntPtr hFile;
            public IntPtr MappedAddress;
            public IntPtr FileHeader;
            public IntPtr lastRvaSection;
            public UInt32 numbOfSections;
            public IntPtr firstRvaSection;
            public UInt32 charachteristics;
            public ushort systemImage;
            public ushort dosImage;
            public ushort readOnly;
            public ushort version;
            public IntPtr links_1;  // these two comprise the LIST_ENTRY
            public IntPtr links_2;
            public UInt32 sizeOfImage;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_COR20_HEADER
        {
            public uint cb;
            public ushort MajorRuntimeVersion;
            public ushort MinorRuntimeVersion;
      
            public IMAGE_DATA_DIRECTORY MetaData;
            public uint Flags;
            public uint EntryPointToken;
         
            public IMAGE_DATA_DIRECTORY Resources;
            public IMAGE_DATA_DIRECTORY StrongNameSignature;
            public IMAGE_DATA_DIRECTORY CodeManagerTable;
            public IMAGE_DATA_DIRECTORY VTableFixups;
            public IMAGE_DATA_DIRECTORY ExportAddressTableJumps;
            public IMAGE_DATA_DIRECTORY ManagedNativeHeader;

        }

        [StructLayout(LayoutKind.Sequential)]
        public struct METADATA_HEADER_1
        {
            public uint Signature;
            public ushort MajorVersion;
            public ushort MinorVersion;
            public uint Reserved;
            public uint VersionLength;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct METADATA_HEADER_2
        {
            public ushort Flags;
            public ushort NumberOfStreams;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct METADATA_STREAM_HEADER
        {
            public uint Offset;
            public uint Size;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct METADATA_TABLE_HEADER
        {
            public uint Reserved1;
            public byte MajorVersion;
            public byte MinorVersion;
            public byte HeapOffsetSizes;
            public byte Reserved2;
            public ulong MaskValid;
            public ulong MaskSorted;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct METADATA_TABLE_ITEMS_HEADER
        {
            
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_DATA_DIRECTORY
        {
            public uint RVA;
            public uint Size;
        }


        public struct IMAGE_RESOURCE_DIRECTORY
        {
            public uint Characteristics;
            public uint TimeDateStamp;
            public ushort MajorVersion;
            public ushort MinorVersion;
            public ushort NumberOfNamedEntries;
            public ushort NumberOfIdEntries;
            /*  IMAGE_RESOURCE_DIRECTORY_ENTRY DirectoryEntries[]; */
        }

        public struct IMAGE_RESOURCE_DIRECTORY_ENTRY
        {
            public uint Name;
            public uint OffsetToData;

        }
        public struct IMAGE_RESOURCE_DIR_STRING_U
        {

            public ushort Length;        // Number of Unicode characters in string

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1)]
            public string NameString; // Length Unicode characters

        }

        public struct IMAGE_RESOURCE_DATA_ENTRY
        {
            public uint OffsetToData;
            public uint Size;
            public uint CodePage;
            public uint Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_VXD_HEADER
        {
            ushort e32_magic;
            byte e32_border;
            byte e32_ushorter;
            uint e32_level;
            ushort e32_cpu;
            ushort e32_os;
            uint e32_ver;
            uint e32_mflags;
            uint e32_mpages;
            uint e32_startobj;
            uint e32_eip;
            uint e32_stackobj;
            uint e32_esp;
            uint e32_pagesize;
            uint e32_lastpagesize;
            uint e32_fixupsize;
            uint e32_fixupsum;
            uint e32_ldrsize;
            uint e32_ldrsum;
            uint e32_objtab;
            uint e32_objcnt;
            uint e32_objmap;
            uint e32_itermap;
            uint e32_rsrctab;
            uint e32_rsrccnt;
            uint e32_restab;
            uint e32_enttab;
            uint e32_dirtab;
            uint e32_dircnt;
            uint e32_fpagetab;
            uint e32_frectab;
            uint e32_impmod;
            uint e32_impmodcnt;
            uint e32_impproc;
            uint e32_pagesum;
            uint e32_datapage;
            uint e32_preload;
            uint e32_nrestab;
            uint e32_cbnrestab;
            uint e32_nressum;
            uint e32_autodata;
            uint e32_debuginfo;
            uint e32_debuglen;
            uint e32_instpreload;
            uint e32_instdemand;
            uint e32_heapsize;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            byte e32_res3;
            uint e32_winresoff;
            uint e32_winreslen;
            ushort e32_devid;
            ushort e32_ddkver;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_OS2_HEADER
        {
            ushort ne_magic;             /* 00 NE signature 'NE' */
            byte ne_ver;               /* 02 Linker version number */
            byte ne_rev;               /* 03 Linker revision number */
            ushort ne_enttab;            /* 04 Offset to entry table relative to NE */
            ushort ne_cbenttab;          /* 06 Length of entry table in bytes */
            long ne_crc;               /* 08 Checksum */
            ushort ne_flags;             /* 0c Flags about segments in this file */
            ushort ne_autodata;          /* 0e Automatic data segment number */
            ushort ne_heap;              /* 10 Initial size of local heap */
            ushort ne_stack;             /* 12 Initial size of stack */
            uint ne_csip;              /* 14 Initial CS:IP */
            uint ne_sssp;              /* 18 Initial SS:SP */
            ushort ne_cseg;              /* 1c # of entries in segment table */
            ushort ne_cmod;              /* 1e # of entries in module reference tab. */
            ushort ne_cbnrestab;         /* 20 Length of nonresident-name table     */
            ushort ne_segtab;            /* 22 Offset to segment table */
            ushort ne_rsrctab;           /* 24 Offset to resource table */
            ushort ne_restab;            /* 26 Offset to resident-name table */
            ushort ne_modtab;            /* 28 Offset to module reference table */
            ushort ne_imptab;            /* 2a Offset to imported name table */
            uint ne_nrestab;           /* 2c Offset to nonresident-name table */
            ushort ne_cmovent;           /* 30 # of movable entry points */
            ushort ne_align;             /* 32 Logical sector alignment shift count */
            ushort ne_cres;              /* 34 # of resource segments */
            byte ne_exetyp;            /* 36 Flags indicating target OS */
            byte ne_flagsothers;       /* 37 Additional information flags */
            ushort ne_pretthunks;        /* 38 Offset to return thunks */
            ushort ne_psegrefbytes;      /* 3a Offset to segment ref. bytes */
            ushort ne_swaparea;          /* 3c Reserved by Microsoft */
            ushort ne_expver;            /* 3e Expected Windows version number */
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_DOS_HEADER
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] e_magic;       // Magic number                         offset: 0
            public UInt16 e_cblp;        // Bytes on last page of file           offset: 2
            public UInt16 e_cp;          // Pages in file                        offset: 4
            public UInt16 e_crlc;        // Relocations                          offset: 6
            public UInt16 e_cparhdr;     // Size of header in paragraphs         offset: 8
            public UInt16 e_minalloc;    // Minimum extra paragraphs needed      offset: A
            public UInt16 e_maxalloc;    // Maximum extra paragraphs needed      offset: C
            public UInt16 e_ss;          // Initial (relative) SS value          offset: E
            public UInt16 e_sp;          // Initial SP value                     offset: 10
            public UInt16 e_csum;        // Checksum                             offset: 12
            public UInt16 e_ip;          // Initial IP value                     offset: 14
            public UInt16 e_cs;          // Initial (relative) CS value          offset: 16
            public UInt16 e_lfarlc;      // File address of relocation table     offset: 18
            public UInt16 e_ovno;        // Overlay number                       offset: 1A
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public UInt16[] e_res1;      // Reserved words                       offset: 22
            public UInt16 e_oemid;       // OEM identifier (for e_oeminfo)       offset: 24
            public UInt16 e_oeminfo;     // OEM information; e_oemid specific    offset: 26
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public UInt16[] e_res2;      // Reserved words                       offset: 3A
            public Int32 e_lfanew;       // File address of new exe header       offset: 3C

            private string _e_magic
            {
                get { return new string(e_magic); }
            }

            public bool isValid
            {
                get { return _e_magic == "MZ"; }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_OPTIONAL_HEADER32
        {
            public UInt16 Magic;                                   //0
            public Byte MajorLinkerVersion;                        //2
            public Byte MinorLinkerVersion;                        //3
            public UInt32 SizeOfCode;                              //4
            public UInt32 SizeOfInitializedData;                   //8
            public UInt32 SizeOfUninitializedData;                 //12
            public UInt32 AddressOfEntryPoint;                     //16
            public UInt32 BaseOfCode;                              //20
            public UInt32 BaseOfData;                              //24
            public UInt32 ImageBase;                               //28
            public UInt32 SectionAlignment;                        //32
            public UInt32 FileAlignment;                           //36
            public UInt16 MajorOperatingSystemVersion;             //40
            public UInt16 MinorOperatingSystemVersion;             //42
            public UInt16 MajorImageVersion;                       //44
            public UInt16 MinorImageVersion;                       //46
            public UInt16 MajorSubsystemVersion;                   //48
            public UInt16 MinorSubsystemVersion;                   //50
            public UInt32 Win32VersionValue;                       //52
            public UInt32 SizeOfImage;                             //56
            public UInt32 SizeOfHeaders;                           //60
            public UInt32 CheckSum;                                //64
            public UInt16 Subsystem;                               //68
            public UInt16 DllCharacteristics;                      //70
            public UInt32 SizeOfStackReserve;                      //72
            public UInt32 SizeOfStackCommit;                       //76
            public UInt32 SizeOfHeapReserve;                       //80
            public UInt32 SizeOfHeapCommit;                        //84
            public UInt32 LoaderFlags;                             //88
            public UInt32 NumberOfRvaAndSizes;                     //92
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_OPTIONAL_HEADER64
        {
            public UInt16 Magic;                                   //0
            public Byte MajorLinkerVersion;                        //2
            public Byte MinorLinkerVersion;                        //3
            public UInt32 SizeOfCode;                              //4
            public UInt32 SizeOfInitializedData;                   //8
            public UInt32 SizeOfUninitializedData;                 //12
            public UInt32 AddressOfEntryPoint;                     //16
            public UInt32 BaseOfCode;                              //20
            public UInt64 ImageBase;                               //24
            public UInt32 SectionAlignment;                        //32
            public UInt32 FileAlignment;                           //36
            public UInt16 MajorOperatingSystemVersion;             //40
            public UInt16 MinorOperatingSystemVersion;             //42
            public UInt16 MajorImageVersion;                       //44
            public UInt16 MinorImageVersion;                       //46
            public UInt16 MajorSubsystemVersion;                   //48
            public UInt16 MinorSubsystemVersion;                   //50
            public UInt32 Win32VersionValue;                       //52
            public UInt32 SizeOfImage;                             //56
            public UInt32 SizeOfHeaders;                           //60
            public UInt32 CheckSum;                                //64
            public UInt16 Subsystem;                               //68
            public UInt16 DllCharacteristics;                      //70
            public UInt64 SizeOfStackReserve;                      //78
            public UInt64 SizeOfStackCommit;                       //86
            public UInt64 SizeOfHeapReserve;                       //94
            public UInt64 SizeOfHeapCommit;                        //102
            public UInt32 LoaderFlags;                             //110
            public UInt32 NumberOfRvaAndSizes;                     //118      
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_FILE_HEADER
        {
            public UInt16 Machine;
            public UInt16 NumberOfSections;
            public UInt32 TimeDateStamp;
            public UInt32 PointerToSymbolTable;
            public UInt32 NumberOfSymbols;
            public UInt16 SizeOfOptionalHeader;
            public UInt16 Characteristics;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct Misc
        {
            [FieldOffset(0)]
            public UInt32 PhysicalAddress;
            [FieldOffset(0)]
            public UInt32 VirtualSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_SECTION_HEADER
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
            public string Name;
            public Misc Misc;
            public UInt32 VirtualAddress;
            public UInt32 SizeOfRawData;
            public UInt32 PointerToRawData;
            public UInt32 PointerToRelocations;
            public UInt32 PointerToLinenumbers;
            public UInt16 NumberOfRelocations;
            public UInt16 NumberOfLinenumbers;
            public UInt32 Characteristics;
        }


        [StructLayout(LayoutKind.Explicit)]
        public unsafe struct IMAGE_IMPORT_BY_NAME
        {
            [FieldOffset(0)]
            public ushort Hint;
            [FieldOffset(2)]
            public fixed char Name[1];
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct IMAGE_IMPORT_DESCRIPTOR
        {
            #region union
            /// <summary>
            /// CSharp doesnt really support unions, but they can be emulated by a field offset 0
            /// </summary>

            [FieldOffset(0)]
            public uint Characteristics;            // 0 for terminating null import descriptor
            [FieldOffset(0)]
            public uint OriginalFirstThunk;         // RVA to original unbound IAT (PIMAGE_THUNK_DATA)
            #endregion

            [FieldOffset(4)]
            public uint TimeDateStamp;
            [FieldOffset(8)]
            public uint ForwarderChain;
            [FieldOffset(12)]
            public uint NameRVA;
            [FieldOffset(16)]
            public uint FirstThunk;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_EXPORT_DIRECTORY
        {
            public uint Characteristics;
            public uint TimeDateStamp;
            public ushort MajorVersion;
            public ushort MinorVersion;
            public uint Name;
            public uint Base;
            public uint NumberOfFunctions;
            public uint NumberOfNames;
            public uint AddressOfFunctions;     // RVA from base of image
            public uint AddressOfNames;         // RVA from base of image
            public uint AddressOfNameOrdinals;  // RVA from base of image
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct THUNK_DATA
        {
            [FieldOffset(0)]
            public uint ForwarderString;      // PBYTE 
            [FieldOffset(0)]
            public uint Function;             // PDWORD
            [FieldOffset(0)]
            public uint Ordinal;
            [FieldOffset(0)]
            public uint AddressOfData;        // PIMAGE_IMPORT_BY_NAME
        }
    }
}
