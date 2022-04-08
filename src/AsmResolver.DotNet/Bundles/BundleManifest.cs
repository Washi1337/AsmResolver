using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.IO;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;
using AsmResolver.PE.Win32Resources.Builder;

namespace AsmResolver.DotNet.Bundles
{
    /// <summary>
    /// Represents a set of bundled files embedded in a .NET application host or single-file host.
    /// </summary>
    public class BundleManifest
    {
        private const int DefaultBundleIDLength = 12;

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

        /// <summary>
        /// Initializes an empty bundle manifest.
        /// </summary>
        protected BundleManifest()
        {
        }

        /// <summary>
        /// Creates a new bundle manifest.
        /// </summary>
        /// <param name="majorVersionNumber">The file format version.</param>
        public BundleManifest(uint majorVersionNumber)
        {
            MajorVersion = majorVersionNumber;
            MinorVersion = 0;
        }

        /// <summary>
        /// Creates a new bundle manifest with a specific bundle identifier.
        /// </summary>
        /// <param name="majorVersionNumber">The file format version.</param>
        /// <param name="bundleId">The unique bundle manifest identifier.</param>
        public BundleManifest(uint majorVersionNumber, string bundleId)
        {
            MajorVersion = majorVersionNumber;
            MinorVersion = 0;
            BundleID = bundleId;
        }

        /// <summary>
        /// Gets or sets the major file format version of the bundle.
        /// </summary>
        /// <remarks>
        /// Version numbers recognized by the CLR are:
        /// <list type="bullet">
        ///     <item>1 for .NET Core 3.1</item>
        ///     <item>2 for .NET 5.0</item>
        ///     <item>6 for .NET 6.0</item>
        /// </list>
        /// </remarks>
        public uint MajorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minor file format version of the bundle.
        /// </summary>
        /// <remarks>
        /// This value is ignored by the CLR and should be set to 0.
        /// </remarks>
        public uint MinorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the unique identifier for the bundle manifest.
        /// </summary>
        /// <remarks>
        /// When this property is set to <c>null</c>, the bundle identifier will be generated upon writing the manifest
        /// based on the contents of the manifest.
        /// </remarks>
        public string? BundleID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets flags associated to the bundle.
        /// </summary>
        public BundleManifestFlags Flags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of files stored in the bundle.
        /// </summary>
        public IList<BundleFile> Files
        {
            get
            {
                if (_files is null)
                    Interlocked.CompareExchange(ref _files, GetFiles(), null);
                return _files;
            }
        }

        /// <summary>
        /// Attempts to automatically locate and parse the bundle header in the provided file.
        /// </summary>
        /// <param name="filePath">The path to the file to read.</param>
        /// <returns>The read manifest.</returns>
        public static BundleManifest FromFile(string filePath)
        {
            return FromBytes(File.ReadAllBytes(filePath));
        }

        /// <summary>
        /// Attempts to automatically locate and parse the bundle header in the provided file.
        /// </summary>
        /// <param name="data">The raw contents of the file to read.</param>
        /// <returns>The read manifest.</returns>
        public static BundleManifest FromBytes(byte[] data)
        {
            return FromDataSource(new ByteArrayDataSource(data));
        }

        /// <summary>
        /// Parses the bundle header in the provided file at the provided address.
        /// </summary>
        /// <param name="data">The raw contents of the file to read.</param>
        /// <param name="offset">The address within the file to start reading the bundle at.</param>
        /// <returns>The read manifest.</returns>
        public static BundleManifest FromBytes(byte[] data, ulong offset)
        {
            return FromDataSource(new ByteArrayDataSource(data), offset);
        }

        /// <summary>
        /// Attempts to automatically locate and parse the bundle header in the provided file.
        /// </summary>
        /// <param name="source">The raw contents of the file to read.</param>
        /// <returns>The read manifest.</returns>
        public static BundleManifest FromDataSource(IDataSource source)
        {
            long address = FindBundleManifestAddress(source);
            if (address == -1)
                throw new BadImageFormatException("File does not contain an AppHost bundle signature.");

            return FromDataSource(source, (ulong) address);
        }

        /// <summary>
        /// Parses the bundle header in the provided file at the provided address.
        /// </summary>
        /// <param name="source">The raw contents of the file to read.</param>
        /// <param name="offset">The address within the file to start reading the bundle at.</param>
        /// <returns>The read manifest.</returns>
        public static BundleManifest FromDataSource(IDataSource source, ulong offset)
        {
            var reader = new BinaryStreamReader(source, 0, 0, (uint) source.Length)
            {
                Offset = offset
            };

            return FromReader(reader);
        }

        /// <summary>
        /// Parses the bundle header from the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream pointing to the start of the bundle to read.</param>
        /// <returns>The read manifest.</returns>
        public static BundleManifest FromReader(BinaryStreamReader reader) => new SerializedBundleManifest(reader);

        private static long FindInFile(IDataSource source, byte[] data)
        {
            // Note: For performance reasons, we read data from the data source in blocks, such that we avoid
            // virtual-dispatch calls and do the searching directly on a byte array instead.

            byte[] buffer = new byte[0x1000];

            ulong start = 0;
            while (start < source.Length)
            {
                int read = source.ReadBytes(start, buffer, 0, buffer.Length);

                for (int i = sizeof(ulong); i < read - data.Length; i++)
                {
                    bool fullMatch = true;
                    for (int j = 0; fullMatch && j < data.Length; j++)
                    {
                        if (buffer[i + j] != data[j])
                            fullMatch = false;
                    }

                    if (fullMatch)
                        return (long) start + i;
                }

                start += (ulong) read;
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

        /// <summary>
        /// Attempts to find the start of the bundle header in the provided file.
        /// </summary>
        /// <param name="source">The file to locate the bundle header in.</param>
        /// <returns>The offset, or -1 if none was found.</returns>
        public static long FindBundleManifestAddress(IDataSource source)
        {
            long signatureAddress = FindInFile(source, BundleSignature);
            if (signatureAddress == -1)
                return -1;

            return ReadBundleManifestAddress(source, signatureAddress);
        }

        /// <summary>
        /// Gets a value indicating whether the provided data source contains a conventional bundled assembly signature.
        /// </summary>
        /// <param name="source">The file to locate the bundle header in.</param>
        /// <returns><c>true</c> if a bundle signature was found, <c>false</c> otherwise.</returns>
        public static bool IsBundledAssembly(IDataSource source) => FindBundleManifestAddress(source) != -1;

        /// <summary>
        /// Obtains the list of files stored in the bundle.
        /// </summary>
        /// <returns>The files</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Files"/> property.
        /// </remarks>
        protected virtual IList<BundleFile> GetFiles() => new OwnedCollection<BundleManifest, BundleFile>(this);

        /// <summary>
        /// Generates a bundle identifier based on the SHA-256 hashes of all files in the manifest.
        /// </summary>
        /// <returns>The generated bundle identifier.</returns>
        public string GenerateDeterministicBundleID()
        {
            using var manifestHasher = SHA256.Create();

            for (int i = 0; i < Files.Count; i++)
            {
                var file = Files[i];
                using var fileHasher = SHA256.Create();
                byte[] fileHash = fileHasher.ComputeHash(file.GetData());
                manifestHasher.TransformBlock(fileHash, 0, fileHash.Length, fileHash, 0);
            }

            manifestHasher.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            byte[] manifestHash = manifestHasher.Hash!;

            return Convert.ToBase64String(manifestHash)
                .Substring(DefaultBundleIDLength)
                .Replace('/', '_');
        }

        /// <summary>
        /// Constructs a new application host file based on the bundle manifest.
        /// </summary>
        /// <param name="outputPath">The path of the file to write to.</param>
        /// <param name="parameters">The parameters to use for bundling all files into a single executable.</param>
        public void WriteUsingTemplate(string outputPath, in BundlerParameters parameters)
        {
            using var fs = File.Create(outputPath);
            WriteUsingTemplate(fs, parameters);
        }

        /// <summary>
        /// Constructs a new application host file based on the bundle manifest.
        /// </summary>
        /// <param name="outputStream">The output stream to write to.</param>
        /// <param name="parameters">The parameters to use for bundling all files into a single executable.</param>
        public void WriteUsingTemplate(Stream outputStream, in BundlerParameters parameters)
        {
            WriteUsingTemplate(new BinaryStreamWriter(outputStream), parameters);
        }

        /// <summary>
        /// Constructs a new application host file based on the bundle manifest.
        /// </summary>
        /// <param name="writer">The output stream to write to.</param>
        /// <param name="parameters">The parameters to use for bundling all files into a single executable.</param>
        public void WriteUsingTemplate(IBinaryStreamWriter writer, BundlerParameters parameters)
        {
            var appBinaryEntry = Files.FirstOrDefault(f => f.RelativePath == parameters.ApplicationBinaryPath);
            if (appBinaryEntry is null)
                throw new ArgumentException($"Application {parameters.ApplicationBinaryPath} does not exist within the bundle.");

            byte[] appBinaryPathBytes = Encoding.UTF8.GetBytes(parameters.ApplicationBinaryPath);
            if (appBinaryPathBytes.Length > 1024)
                throw new ArgumentException("Application binary path cannot exceed 1024 bytes.");

            if (!parameters.IsArm64Linux)
                EnsureAppHostPEHeadersAreUpToDate(ref parameters);

            var appHostTemplateSource = new ByteArrayDataSource(parameters.ApplicationHostTemplate);
            long signatureAddress = FindInFile(appHostTemplateSource, BundleSignature);
            if (signatureAddress == -1)
                throw new ArgumentException("AppHost template does not contain the bundle signature.");

            long appBinaryPathAddress = FindInFile(appHostTemplateSource, AppBinaryPathPlaceholder);
            if (appBinaryPathAddress == -1)
                throw new ArgumentException("AppHost template does not contain the application binary path placeholder.");

            writer.WriteBytes(parameters.ApplicationHostTemplate);
            writer.Offset = writer.Length;
            ulong headerAddress = WriteManifest(writer, parameters.IsArm64Linux);

            writer.Offset = (ulong) signatureAddress - sizeof(ulong);
            writer.WriteUInt64(headerAddress);

            writer.Offset = (ulong) appBinaryPathAddress;
            writer.WriteBytes(appBinaryPathBytes);
            if (AppBinaryPathPlaceholder.Length > appBinaryPathBytes.Length)
                writer.WriteZeroes(AppBinaryPathPlaceholder.Length - appBinaryPathBytes.Length);
        }

        private static void EnsureAppHostPEHeadersAreUpToDate(ref BundlerParameters parameters)
        {
            PEFile file;
            try
            {
                file = PEFile.FromBytes(parameters.ApplicationHostTemplate);
            }
            catch (BadImageFormatException)
            {
                // Template is not a PE file.
                return;
            }

            bool changed = false;

            // Ensure same Windows subsystem is used (typically required for GUI applications).
            if (file.OptionalHeader.SubSystem != parameters.SubSystem)
            {
                file.OptionalHeader.SubSystem = parameters.SubSystem;
                changed = true;
            }

            // If the app binary has resources (such as an icon or version info), we need to copy it into the
            // AppHost template so that they are also visible from the final packed executable.
            if (parameters.Resources is { } directory)
            {
                // Put original resource directory in a new .rsrc section.
                var buffer = new ResourceDirectoryBuffer();
                buffer.AddDirectory(directory);
                var rsrc = new PESection(".rsrc", SectionFlags.MemoryRead | SectionFlags.ContentInitializedData);
                rsrc.Contents = buffer;

                // Find .reloc section, and insert .rsrc before it if it is present. Otherwise just append to the end.
                int sectionIndex = file.Sections.Count - 1;
                for (int i = file.Sections.Count - 1; i >= 0; i--)
                {
                    if (file.Sections[i].Name == ".reloc")
                    {
                        sectionIndex = i;
                        break;
                    }
                }

                file.Sections.Insert(sectionIndex, rsrc);

                // Update resource data directory va + size.
                file.AlignSections();
                file.OptionalHeader.DataDirectories[(int) DataDirectoryIndex.ResourceDirectory] = new DataDirectory(
                    buffer.Rva,
                    buffer.GetPhysicalSize());

                changed = true;
            }

            // Rebuild AppHost PE file if necessary.
            if (changed)
            {
                using var stream = new MemoryStream();
                file.Write(stream);
                parameters.ApplicationHostTemplate = stream.ToArray();
            }
        }

        /// <summary>
        /// Writes the manifest to an output stream.
        /// </summary>
        /// <param name="writer">The output stream to write to.</param>
        /// <param name="isArm64Linux"><c>true</c> if the application host is a Linux ELF binary targeting ARM64.</param>
        /// <returns>The address of the bundle header.</returns>
        /// <remarks>
        /// This does not necessarily produce a working executable file, it only writes the contents of the entire manifest,
        /// without a host application that invokes the manifest. If you want to produce a runnable executable, use one
        /// of the <c>WriteUsingTemplate</c> methods instead.
        /// </remarks>
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

            BundleID ??= GenerateDeterministicBundleID();
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
