using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.DotNet.StrongName;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.DotNet.Cryptography
{
    public class StrongNameSigner
    {
        private readonly StrongNamePrivateKey _privateKey;

        public StrongNameSigner(StrongNamePrivateKey privateKey)
        {
            _privateKey = privateKey ?? throw new ArgumentNullException(nameof(privateKey));
        }

        public void PrepareImageForSigning(IPEImage image)
        {
            image.DotNetDirectory.StrongName = new DataSegment(new byte[_privateKey.BitLength / 8]);
        }

        public void SignImage(byte[] rawContents, AssemblyHashAlgorithm hashAlgorithm)
        {
            var file = PEFile.FromBytes(rawContents);
            var image = PEImage.FromFile(file);
            
            // Check existence of .NET metadata.
            if (image.DotNetDirectory is null)
                throw new BadImageFormatException("Input image is not a .NET assembly.");
            
            // Check existence of a valid sn directory.
            var strongNameDirectory = image.DotNetDirectory.StrongName;
            if (strongNameDirectory is null)
                throw new InvalidOperationException("Cannot sign an image without a strong name directory.");
            
            if (_privateKey.Modulus.Length != strongNameDirectory.GetPhysicalSize())
            {
                throw new ArgumentException(
                    "The strong name signature directory size does not match the size of the strong signature.");
            }

            // Compute hash of pseudo PE file.
            var hash = GetHashToSign(rawContents, file, image, hashAlgorithm);

            // Compute strong name signature.
            using var rsa = new RSACryptoServiceProvider();
            var rsaParameters = _privateKey.ToRsaParameters();
            rsa.ImportParameters(rsaParameters);
            
            var formatter = new RSAPKCS1SignatureFormatter(rsa);
            formatter.SetHashAlgorithm(hashAlgorithm.ToString().ToUpperInvariant());
            var signature = formatter.CreateSignature(hash);
            Array.Reverse(signature);
            
            // Copy strong name signature into target PE.
            Buffer.BlockCopy(signature, 0, rawContents, (int) strongNameDirectory.FileOffset, signature.Length);
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
            if (certificateDirectory.IsPresentInPE)
            {
                uint rva = file.RvaToFileOffset(certificateDirectory.VirtualAddress);
                hashBuilder.ExcludeRange(new OffsetRange(rva, rva + certificateDirectory.Size));
            }

            // Zero strong name directory entry.
            // var strongNameEntryOffset = image.DotNetDirectory.FileOffset + 0x20;
            // hashBuilder.ZeroRange(new OffsetRange(
            //     strongNameEntryOffset,
            //     strongNameEntryOffset + DataDirectory.DataDirectorySize));
            //
            // Exclude strong name directory.
            var strongNameDirectory = image.DotNetDirectory.StrongName;
            hashBuilder.ExcludeRange(new OffsetRange(
                strongNameDirectory.FileOffset,
                strongNameDirectory.FileOffset + strongNameDirectory.GetPhysicalSize()));
            
            return hashBuilder.ComputeHash();
        }
        
    }
}