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

        internal Structures.IMAGE_COR20_HEADER _rawHeader;
        internal Win32Assembly _assembly;
        //internal NETHeaderReader reader;
        StringsHeap _stringsheap;
        UserStringsHeap _usheap;
        TablesHeap _tableheap;
        BlobHeap _blobheap;
        GuidHeap _guidheap;
        internal TypeSystem _typeSystem;
        MetaDataHeader _metadata;
        internal MetaDataStream[] _streams;
        internal uint _rawOffset;

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
                if (_typeSystem == null)
                    _typeSystem = new TypeSystem(this);
                return _typeSystem;
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
            MetaDataResolver = new MetaDataResolver(new AssemblyResolver());
        }

        /// <summary>
        /// Gets the Portable Executeable's NT header by specifing the assembly.
        /// </summary>
        /// <param name="assembly">The assembly to read the nt header</param>
        /// <returns></returns>
        public static NETHeader FromAssembly(Win32Assembly assembly)
        {
            NETHeader header = new NETHeader();
            
            header._assembly = assembly;
            NETHeaderReader reader = new NETHeaderReader(assembly._ntHeader, header);
            header._metadata = new MetaDataHeader(reader);
            reader.LoadData();
            header.TokenResolver = new MetaDataTokenResolver(header);
            return header;
            

        }

        /// <summary>
        /// Gets or sets the EntryPoint Token of the loaded .NET application.
        /// </summary>
        public uint EntryPointToken
        {
            get { return _rawHeader.EntryPointToken; }
            set
            {
                int targetoffset = (int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_COR20_HEADER)][5];
                _assembly._peImage.SetOffset(targetoffset);
                _assembly._peImage.Writer.Write(value);
                _rawHeader.EntryPointToken = value;
            }
        }

        /// <summary>
        /// Gets or sets the Flags of this .NET header.
        /// </summary>
        public NETHeaderFlags Flags
        {
            get { return (NETHeaderFlags)_rawHeader.Flags; }
            set
            {
                int targetoffset = (int)RawOffset + Structures.DataOffsets[typeof(Structures.IMAGE_COR20_HEADER)][4];
                _assembly._peImage.SetOffset(targetoffset);
                _assembly._peImage.Writer.Write((uint)value);
                _rawHeader.Flags = (uint)value;
            }
        }

        /// <summary>
        /// Gets the header of the MetaData.
        /// </summary>
        public MetaDataHeader MetaDataHeader
        {
            get { return _metadata; }
        }

        /// <summary>
        /// Gets the metadata streams in an array.
        /// </summary>
        public MetaDataStream[] MetaDataStreams
        {
            get { return _streams; }
        }

        /// <summary>
        /// Gets the tables heap of the .net application.
        /// </summary>
        public TablesHeap TablesHeap
        {
            get 
            {
                if (_tableheap == null)
                {
                    _tableheap = MetaDataStreams.FirstOrDefault(t => t.Name == "#~" || t.Name == "#-") as TablesHeap;
                }

                return _tableheap;
            }
        }

        /// <summary>
        /// Gets the strings heap of the .net application.
        /// </summary>
        public StringsHeap StringsHeap
        {
            get
            {
                if (_stringsheap == null)
                    _stringsheap = MetaDataStreams.FirstOrDefault(t => t.Name == "#Strings") as StringsHeap;
                return _stringsheap;
            }
        }

        /// <summary>
        /// Gets the user specified strings heap of the .net application.
        /// </summary>
        public UserStringsHeap UserStringsHeap
        {
            get
            {
                if (_usheap == null)
                    _usheap = MetaDataStreams.FirstOrDefault(t => t.Name == "#US") as UserStringsHeap;
                return _usheap;
            }
        }

        /// <summary>
        /// Gets the blob heap of the .net application.
        /// </summary>
        public BlobHeap BlobHeap
        {
            get
            {
                if (_blobheap == null)
                    _blobheap = MetaDataStreams.FirstOrDefault(t => t.Name == "#Blob") as BlobHeap;
                return _blobheap;
            }
        }

        /// <summary>
        /// Gets the GUID heap of the .net application.
        /// </summary>
        public GuidHeap GuidHeap
        {
            get
            {
                if (_guidheap == null)
                    _guidheap = MetaDataStreams.FirstOrDefault(t => t.Name == "#GUID") as GuidHeap;
                return _guidheap;
            }
        }

        public MetaDataResolver MetaDataResolver
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the parent assembly container of the header.
        /// </summary>
        public Win32Assembly ParentAssembly 
        {
            get
            {
                return _assembly;
            }
        }

        /// <summary>
        /// Gets the raw file offset of the header.
        /// </summary>
        public long RawOffset
        {
            get
            {
                return _rawOffset;
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
                if (!HasStream("#Blob"))
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
            return (MetaDataStreams.FirstOrDefault(s => s._name == name) != null);
        }

        /// <summary>
        /// Frees all heaps and streams that are being used.
        /// </summary>
        public void Dispose()
        {
            if (_blobheap != null)
                _blobheap.Dispose();
            if (_guidheap != null)
                _guidheap.Dispose();
            if (_stringsheap != null)
                _stringsheap.Dispose();
            if (_usheap != null)
                _usheap.Dispose();
            if (_tableheap != null)
                _tableheap.Dispose();

        }

        public void ClearCache()
        {
            _tableheap = null;
            _stringsheap = null;
            _usheap = null;
            _blobheap = null;
            _guidheap = null;
            
        }

        public void LoadCache()
        {
            _tableheap = TablesHeap;
            _stringsheap = StringsHeap;
            _usheap = UserStringsHeap;
            _blobheap = BlobHeap;
            _guidheap = GuidHeap;
        }
    }
}
