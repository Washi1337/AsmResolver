using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.DotNet.StrongName
{
    public class StrongNameSigner
    {
        private readonly RSA _cryptoService;

        public StrongNameSigner(string snkFile)
            : this(System.IO.File.ReadAllBytes(snkFile))
        {
        }

        public StrongNameSigner(byte[] snkData)
        {
            if (snkData == null)
                throw new ArgumentNullException(nameof(snkData));
            
            var rsa = new RSACryptoServiceProvider();
            rsa.ImportCspBlob(snkData);
            _cryptoService = rsa;
        }
        
        public StrongNameSigner(RSA cryptoService)
        {
            _cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
        }

        private byte[] GetDataToSign(byte[] peFileContents)
        {
            var file = PEFile.FromBytes(peFileContents);
            
            // Obtain used hashing algorithm.
            var image = PEImage.FromFile(file);
            var hashAlgorithm = image.DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetTable<AssemblyDefinitionRow>(TableIndex.Assembly)
                .FirstOrDefault().HashAlgorithm;
            
            var hashBuilder = new StrongNameDataHashBuilder(peFileContents, hashAlgorithm);

            // Zero checksum in optional header.
            uint peChecksumOffset = file.OptionalHeader.FileOffset + 0x40;
            hashBuilder.ZeroRange(new OffsetRange(peChecksumOffset, peChecksumOffset + sizeof(uint)));
            
            // Zero certificate directory entry.
            uint optionalHeaderSize = file.OptionalHeader.Magic == OptionalHeaderMagic.Pe32
                ? OptionalHeader.OptionalHeader32SizeExcludingDataDirectories
                : OptionalHeader.OptionalHeader64SizeExcludingDataDirectories;
            uint certificateEntryOffset = file.OptionalHeader.FileOffset
                                          + optionalHeaderSize
                                          + OptionalHeader.CertificateDirectoryIndex * DataDirectory.DataDirectorySize;
            hashBuilder.ZeroRange(new OffsetRange(certificateEntryOffset, certificateEntryOffset + DataDirectory.DataDirectorySize));
            
            // Exclude certificate directory contents.
            var certificateDirectory = file.OptionalHeader.DataDirectories[OptionalHeader.CertificateDirectoryIndex];
            hashBuilder.ExcludeRange(new OffsetRange(
                file.RvaToFileOffset(certificateDirectory.VirtualAddress),
                certificateDirectory.Size));

            // Exclude strong name directory.
            var strongNameDirectory = image.DotNetDirectory.StrongName;
            hashBuilder.ExcludeRange(new OffsetRange(
                strongNameDirectory.FileOffset,
                strongNameDirectory.GetPhysicalSize()));
            
            return hashBuilder.ComputeHash();
        }

    }
}