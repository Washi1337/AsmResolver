using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.PE;
using TUP.AsmResolver.PE.Readers;
using TUP.AsmResolver.NET.Specialized;
namespace TUP.AsmResolver.NET
{
    /// <summary>
    /// Represents a .NET header from an application. This header is only available if the assembly is written in a .NET language.
    /// </summary>
    public class NETHeader : IHeader , IDisposable , IDataDirectoryProvider, ICacheProvider
    {

        internal Structures.IMAGE_COR20_HEADER rawHeader;
        internal Win32Assembly assembly;
        //internal NETHeaderReader reader;
        StringsHeap stringsheap;
        UserStringsHeap usheap;
        TablesHeap tableheap;
        BlobHeap blobheap;
        GuidHeap guidheap;
        internal TypeSystem typeSystem;
        MetaDataHeader metadata;
        internal MetaDataStream[] streams;
        internal uint rawOffset;

        /// <summary>
        /// Gets a metadata token resolver to lookup Members by its metadata token.
        /// </summary>
        public MetaDataTokenResolver TokenResolver { get; internal set; }

        /// <summary>
        /// Gets the type system class that holds all element types.
        /// </summary>
        public TypeSystem TypeSystem
        {
            get
            {
                if (typeSystem == null)
                    typeSystem = new TypeSystem(this);
                return typeSystem;
            }
        }


        public DataDirectory[] DataDirectories { get; internal set; }
        public DataDirectory MetaDataDirectory { get { return DataDirectories[0]; } }
        public DataDirectory ResourcesDirectory { get { return DataDirectories[1]; } }
        public DataDirectory StrongNameDirectory { get { return DataDirectories[2]; } }
        public DataDirectory CodeManagerDirectory { get { return DataDirectories[3]; } }
        public DataDirectory VTableFixupsDirectory { get { return DataDirectories[4]; } }
        public DataDirectory ExportAddressesDirectory { get { return DataDirectories[5]; } }
        public DataDirectory ManagedNativeHeaderDirectory { get { return DataDirectories[6]; } }



        internal NETHeader()
        {
        }

        /// <summary>
        /// Gets the Portable Executeable's NT header by specifing the assembly.
        /// </summary>
        /// <param name="assembly">The assembly to read the nt header</param>
        /// <returns></returns>
        public static NETHeader FromAssembly(Win32Assembly assembly)
        {
            NETHeader header = new NETHeader();
            
            header.assembly = assembly;
            NETHeaderReader reader = new NETHeaderReader(assembly._ntHeader, header);
            header.metadata = new MetaDataHeader(reader);
            reader.LoadData();
            header.TokenResolver = new MetaDataTokenResolver(header);
            return header;
            

        }

        /// <summary>
        /// Gets the EntryPoint Token of the loaded .NET application.
        /// </summary>
        public uint EntryPointToken
        {
            get { return rawHeader.EntryPointToken; }
        }

        /// <summary>
        /// Gets or sets the Flags of this .NET header.
        /// </summary>
        public NETHeaderFlags Flags
        {
            get { return (NETHeaderFlags)rawHeader.Flags; }
            set
            {
                int targetoffset = (int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_COR20_HEADER)][4];
                assembly._peImage.SetOffset(targetoffset);
                assembly._peImage.Writer.Write((uint)value);
                rawHeader.Flags = (uint)value;
            }
        }

        /// <summary>
        /// Gets the header of the MetaData.
        /// </summary>
        public MetaDataHeader MetaDataHeader
        {
            get { return metadata; }
        }

        /// <summary>
        /// Gets the metadata streams in an array.
        /// </summary>
        public MetaDataStream[] MetaDataStreams
        {
            get { return streams; }
        }

        /// <summary>
        /// Gets the tables heap of the .net application.
        /// </summary>
        public TablesHeap TablesHeap
        {
            get 
            {
                if (tableheap == null)
                {
                    tableheap = (TablesHeap)MetaDataStreams.FirstOrDefault(t => t.name == "#~" || t.name == "#-");
                }

                return tableheap;
            }
        }

        /// <summary>
        /// Gets the strings heap of the .net application.
        /// </summary>
        public StringsHeap StringsHeap
        {
            get
            {
                if (stringsheap == null)
                    stringsheap = (StringsHeap)MetaDataStreams.FirstOrDefault(t => t.name == "#Strings");
                return stringsheap;
            }
        }

        /// <summary>
        /// Gets the user specified strings heap of the .net application.
        /// </summary>
        public UserStringsHeap UserStringsHeap
        {
            get
            {
                if (usheap == null)
                    usheap = (UserStringsHeap)MetaDataStreams.FirstOrDefault(t => t.name == "#US");
                return usheap;
            }
        }

        /// <summary>
        /// Gets the blob heap of the .net application.
        /// </summary>
        public BlobHeap BlobHeap
        {
            get
            { 
                if (blobheap == null)
                    blobheap = (BlobHeap)MetaDataStreams.FirstOrDefault(t => t.name == "#Blob"); 
                return blobheap;
            }
        }

        /// <summary>
        /// Gets the GUID heap of the .net application.
        /// </summary>
        public GuidHeap GuidHeap
        {
            get
            {
                if (guidheap == null)
                    guidheap = (GuidHeap)MetaDataStreams.First(t => t.name == "#GUID");
                return guidheap;
            }
        }

        /// <summary>
        /// Gets the parent assembly container of the header.
        /// </summary>
        public Win32Assembly ParentAssembly 
        {
            get
            {
                return assembly;
            }
        }

        /// <summary>
        /// Gets the raw file offset of the header.
        /// </summary>
        public long RawOffset
        {
            get
            {
                return rawOffset;
            }
        }

        /// <summary>
        /// Gets a value indicating the .NET header is valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (MetaDataStreams == null || MetaDataStreams.Length == 0)
                    return false;
                if (!HasStream("#~") && !HasStream("#-"))
                    return false;
                if (!HasStream("#Strings"))
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Returns true when a stream specified by its name is present in the assembly.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <returns></returns>
        public bool HasStream(string name)
        {
            if (MetaDataStreams == null || MetaDataStreams.Length == 0)
                return false;
            return (MetaDataStreams.FirstOrDefault(s => s.name == name) != null);
        }

        /// <summary>
        /// Frees all heaps and streams that are being used.
        /// </summary>
        public void Dispose()
        {
            if (blobheap != null)
                blobheap.Dispose();
            if (guidheap != null)
                guidheap.Dispose();
            if (stringsheap != null)
                stringsheap.Dispose();
            if (usheap != null)
                usheap.Dispose();
            if (tableheap != null)
                tableheap.Dispose();

        }

        public void ClearCache()
        {
            tableheap = null;
            stringsheap = null;
            usheap = null;
            blobheap = null;
            guidheap = null;
            
        }

        public void LoadCache()
        {
            tableheap = TablesHeap;
            stringsheap = StringsHeap;
            usheap = UserStringsHeap;
            blobheap = BlobHeap;
            guidheap = GuidHeap;
        }
    }
}
