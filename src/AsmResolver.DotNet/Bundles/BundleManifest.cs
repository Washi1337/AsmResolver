using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.IO;

namespace AsmResolver.DotNet.Bundles
{
    public class BundleManifest
    {
        private static readonly byte[] BundleSignature =
        {
            0x8b, 0x12, 0x02, 0xb9, 0x6a, 0x61, 0x20, 0x38,
            0x72, 0x7b, 0x93, 0x02, 0x14, 0xd7, 0xa0, 0x32,
            0x13, 0xf5, 0xb9, 0xe6, 0xef, 0xae, 0x33, 0x18,
            0xee, 0x3b, 0x2d, 0xce, 0x24, 0xb3, 0x6a, 0xae
        };

        private static readonly byte[] AppBinaryPathPlaceholder =
            Encoding.UTF8.GetBytes("c3ab8ff13720e8ad9047dd39466b3c8974e592c2fa383d4a3960714caef0c4f2");

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

        private static long FindInFile(IDataSource source, byte[] data)
        {
            for (ulong i = sizeof(ulong); i < source.Length - (ulong) data.Length; i++)
            {
                bool fullMatch = true;
                for (int j = 0; fullMatch && j < data.Length; j++)
                {
                    if (source[i + (ulong) j] != data[j])
                        fullMatch = false;
                }

                if (fullMatch)
                    return (long) i;
            }

            return -1;
        }

        private static long ReadBundleManifestAddress(IDataSource source, long signatureAddress)
        {
            var reader = new BinaryStreamReader(source, (ulong) signatureAddress - sizeof(ulong), 0, 8);
            ulong manifestAddress = reader.ReadUInt64();

            return source.IsValidAddress(manifestAddress)
                ? (long) manifestAddress
                : -1;
        }

        public static long FindBundleManifestAddress(IDataSource source)
        {
            long signatureAddress = FindInFile(source, BundleSignature);
            if (signatureAddress == -1)
                return -1;

            return ReadBundleManifestAddress(source, signatureAddress);
        }

        protected virtual IList<BundleFile> GetFiles() => new OwnedCollection<BundleManifest, BundleFile>(this);

        public void WriteUsingTemplate(string appHostTemplatePath, Stream outputStream, string appBinaryPath)
        {
            WriteUsingTemplate(System.IO.File.ReadAllBytes(appHostTemplatePath), outputStream, appBinaryPath,
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                && RuntimeInformation.ProcessArchitecture
                == Architecture.Arm64);
        }

        public void WriteUsingTemplate(byte[] appHostTemplate, Stream outputStream, string appBinaryPath)
        {
            WriteUsingTemplate(appHostTemplate, outputStream, appBinaryPath,
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                && RuntimeInformation.ProcessArchitecture
                == Architecture.Arm64);
        }

        public void WriteUsingTemplate(byte[] appHostTemplate, Stream outputStream, string appBinaryPath, bool isArm64Linux)
        {
            WriteUsingTemplate(appHostTemplate, new BinaryStreamWriter(outputStream), appBinaryPath, isArm64Linux);
        }

        public void WriteUsingTemplate(byte[] appHostTemplate, IBinaryStreamWriter writer, string appBinaryPath, bool isArm64Linux)
        {
            byte[] appBinaryPathBytes = Encoding.UTF8.GetBytes(appBinaryPath);
            if (appBinaryPathBytes.Length > 1024)
                throw new ArgumentException("Application binary path cannot exceed 1024 bytes.");

            long signatureAddress = FindInFile(new ByteArrayDataSource(appHostTemplate), BundleSignature);
            if (signatureAddress == -1)
                throw new ArgumentException("AppHost template does not contain the bundle signature.");

            long appBinaryPathAddress = FindInFile(new ByteArrayDataSource(appHostTemplate), AppBinaryPathPlaceholder);
            if (appBinaryPathAddress == -1)
                throw new ArgumentException("AppHost template does not contain the application binary path placeholder.");

            writer.WriteBytes(appHostTemplate);
            writer.Offset = writer.Length;
            ulong headerAddress = WriteManifest(writer, isArm64Linux);

            writer.Offset = (ulong) signatureAddress - sizeof(ulong);
            writer.WriteUInt64(headerAddress);

            writer.Offset = (ulong) appBinaryPathAddress;
            writer.WriteBytes(appBinaryPathBytes);
            if (AppBinaryPathPlaceholder.Length > appBinaryPathBytes.Length)
                writer.WriteZeroes(AppBinaryPathPlaceholder.Length - appBinaryPathBytes.Length);
        }

        public ulong WriteManifest(IBinaryStreamWriter writer, bool isArm64Linux)
        {
            WriteFileContents(writer, isArm64Linux
                ? 4096u
                : 16u);

            ulong headerAddress = writer.Offset;
            WriteManifestHeader(writer);

            return headerAddress;
        }

        private void WriteFileContents(IBinaryStreamWriter writer, uint alignment)
        {
            for (int i = 0; i < Files.Count; i++)
            {
                var file = Files[i];

                if (file.Type == BundleFileType.Assembly)
                    writer.Align(alignment);

                file.Contents.UpdateOffsets(writer.Offset, (uint) writer.Offset);
                file.Contents.Write(writer);
            }
        }

        private void WriteManifestHeader(IBinaryStreamWriter writer)
        {
            writer.WriteUInt32(MajorVersion);
            writer.WriteUInt32(MinorVersion);
            writer.WriteInt32(Files.Count);
            writer.WriteBinaryFormatterString(BundleID);

            if (MajorVersion >= 2)
            {
                WriteFileOffsetSizePair(writer, Files.FirstOrDefault(f => f.Type == BundleFileType.DepsJson));
                WriteFileOffsetSizePair(writer, Files.FirstOrDefault(f => f.Type == BundleFileType.RuntimeConfigJson));
                writer.WriteUInt64((ulong) Flags);
            }

            WriteFileHeaders(writer);
        }

        private void WriteFileHeaders(IBinaryStreamWriter writer)
        {
            for (int i = 0; i < Files.Count; i++)
            {
                var file = Files[i];

                WriteFileOffsetSizePair(writer, file);

                if (MajorVersion >= 6)
                    writer.WriteUInt64(file.IsCompressed ? file.Contents.GetPhysicalSize() : 0);

                writer.WriteByte((byte) file.Type);
                writer.WriteBinaryFormatterString(file.RelativePath);
            }
        }

        private static void WriteFileOffsetSizePair(IBinaryStreamWriter writer, BundleFile? file)
        {
            if (file is not null)
            {
                writer.WriteUInt64(file.Contents.Offset);
                writer.WriteUInt64((ulong) file.GetData().Length);
            }
            else
            {
                writer.WriteUInt64(0);
                writer.WriteUInt64(0);
            }
        }

    }
}
