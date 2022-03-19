using System;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Bundles
{
    public class BundleManifest
    {
        private static readonly byte[] BundleSignature = {
            0x8b, 0x12, 0x02, 0xb9, 0x6a, 0x61, 0x20, 0x38,
            0x72, 0x7b, 0x93, 0x02, 0x14, 0xd7, 0xa0, 0x32,
            0x13, 0xf5, 0xb9, 0xe6, 0xef, 0xae, 0x33, 0x18,
            0xee, 0x3b, 0x2d, 0xce, 0x24, 0xb3, 0x6a, 0xae
        };

        private IList<BundleFile>? _files;

        protected BundleManifest()
        {
        }

        public BundleManifest(uint version, string bundleId)
        {
            MajorVersion = version;
            MinorVersion = 0;
            BundleID = bundleId;
        }

        public uint MajorVersion
        {
            get;
            set;
        }

        public uint MinorVersion
        {
            get;
            set;
        }

        public string BundleID
        {
            get;
            set;
        }

        public BundleManifestFlags Flags
        {
            get;
            set;
        }

        public IList<BundleFile> Files
        {
            get
            {
                if (_files is null)
                    Interlocked.CompareExchange(ref _files, GetFiles(), null);
                return _files;
            }
        }

        public static BundleManifest FromFile(string filePath)
        {
            return FromBytes(System.IO.File.ReadAllBytes(filePath));
        }

        public static BundleManifest FromBytes(byte[] data)
        {
            return FromDataSource(new ByteArrayDataSource(data));
        }

        public static BundleManifest FromBytes(byte[] data, ulong offset)
        {
            return FromDataSource(new ByteArrayDataSource(data), offset);
        }

        public static BundleManifest FromDataSource(IDataSource source)
        {
            long address = FindBundleManifestAddress(source);
            if (address == -1)
                throw new BadImageFormatException("File does not contain an AppHost bundle signature.");

            return FromDataSource(source, (ulong) address);
        }

        public static BundleManifest FromDataSource(IDataSource source, ulong offset)
        {
            var reader = new BinaryStreamReader(source, 0, 0, (uint) source.Length)
            {
                Offset = offset
            };

            return FromReader(reader);
        }

        public static BundleManifest FromReader(BinaryStreamReader reader) => new SerializedBundleManifest(reader);

        public static long FindBundleManifestAddress(IDataSource source)
        {
            for (ulong i = sizeof(ulong); i < source.Length - (ulong) BundleSignature.Length; i++)
            {
                bool fullMatch = true;
                for (int j = 0; fullMatch && j < BundleSignature.Length; j++)
                {
                    if (source[i + (ulong) j] != BundleSignature[j])
                        fullMatch = false;
                }

                if (fullMatch)
                {
                    var reader = new BinaryStreamReader(source, i - sizeof(ulong), 0, 8);
                    ulong address = reader.ReadUInt64();
                    if (source.IsValidAddress(address))
                        return (long) address;
                }
            }

            return -1;
        }

        protected virtual IList<BundleFile> GetFiles() => new List<BundleFile>();
    }
}
