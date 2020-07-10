using System;
using System.Linq;
using System.Security.Cryptography;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.DotNet.Cryptography
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

        private byte[] GetHashToSign(
            byte[] rawContents,
            PEFile file, 
            IPEImage image,
            AssemblyHashAlgorithm hashAlgorithm)
        {
            
            var hashBuilder = new StrongNameDataHashBuilder(rawContents, hashAlgorithm);

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

            // Zero strong name directory entry.
            var strongNameEntryOffset = image.DotNetDirectory.FileOffset + 0x20;
            hashBuilder.ZeroRange(new OffsetRange(
                strongNameEntryOffset,
                strongNameEntryOffset + DataDirectory.DataDirectorySize));
            
            // Exclude strong name directory.
            var strongNameDirectory = image.DotNetDirectory.StrongName;
            hashBuilder.ExcludeRange(new OffsetRange(
                strongNameDirectory.FileOffset,
                strongNameDirectory.GetPhysicalSize()));
            
            return hashBuilder.ComputeHash();
        }
    }
}