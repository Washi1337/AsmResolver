using System;
using System.IO;
using System.Security.Cryptography;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.DotNet.StrongName
{
    /// <summary>
    /// Provides a mechanism for adding a strong name signature to a PE image.
    /// </summary>
    public class StrongNameSigner
    {
        /// <summary>
        /// Creates a new strong name signer instance.
        /// </summary>
        /// <param name="privateKey">The private key to use.</param>
        public StrongNameSigner(StrongNamePrivateKey privateKey)
        {
            PrivateKey = privateKey ?? throw new ArgumentNullException(nameof(privateKey));
        }

        /// <summary>
        /// Gets the private key to use for signing the image.
        /// </summary>
        public StrongNamePrivateKey PrivateKey
        {
            get;
        }
        
        /// <summary>
        /// Finalizes a delay-signed PE image. 
        /// </summary>
        /// <param name="imageStream">The stream containing the image to sign.</param>
        /// <param name="hashAlgorithm">The hashing algorithm to use.</param>
        /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET image.</exception>
        /// <exception cref="ArgumentException">Occurs when the image does not contain a strong name directory of the right size.</exception>
        public void SignImage(Stream imageStream, AssemblyHashAlgorithm hashAlgorithm)
        {
            imageStream.Position = 0;
            using var streamCopy = new MemoryStream();
            imageStream.CopyTo(streamCopy);
            streamCopy.Position = 0;
            
            var file = PEFile.FromBytes(streamCopy.ToArray());
            var image = PEImage.FromFile(file);
            
            // Check existence of .NET metadata.
            if (image.DotNetDirectory is null)
                throw new BadImageFormatException("Input image is not a .NET assembly.");
            
            // Check existence of a valid sn directory.
            var strongNameDirectory = image.DotNetDirectory.StrongName;
            if (strongNameDirectory is null)
                throw new ArgumentException("Cannot sign an image without a strong name directory.");
            
            if (PrivateKey.Modulus.Length != strongNameDirectory.GetPhysicalSize())
            {
                throw new ArgumentException(
                    "The strong name signature directory size does not match the size of the strong signature.");
            }

            // Compute hash of pseudo PE file.
            var hash = GetHashToSign(streamCopy, file, image, hashAlgorithm);

            // Compute strong name signature.
            var signature = ComputeSignature(hashAlgorithm, hash);

            // Copy strong name signature into target PE.
            imageStream.Position = strongNameDirectory.FileOffset;
            imageStream.Write(signature, 0, signature.Length);
        }

        private byte[] GetHashToSign(
            Stream imageStream,
            PEFile file, 
            IPEImage image,
            AssemblyHashAlgorithm hashAlgorithm)
        {
            var hashBuilder = new StrongNameDataHashBuilder(imageStream, hashAlgorithm);
            
            // Include DOS, NT and section headers in the hash.
            hashBuilder.IncludeRange(new OffsetRange(0,
                (uint) (file.DosHeader.GetPhysicalSize()
                        + sizeof(uint)
                        + file.FileHeader.GetPhysicalSize()
                        + file.OptionalHeader.GetPhysicalSize()
                        + file.Sections.Count * SectionHeader.SectionHeaderSize)));
            
            // Include section data.
            foreach (var section in file.Sections)
            {
                hashBuilder.IncludeRange(new OffsetRange(
                    section.FileOffset,
                    section.FileOffset + section.GetPhysicalSize()));
            }

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
            
            // Exclude strong name directory.
            var strongNameDirectory = image.DotNetDirectory.StrongName;
            hashBuilder.ExcludeRange(new OffsetRange(
                strongNameDirectory.FileOffset,
                strongNameDirectory.FileOffset + strongNameDirectory.GetPhysicalSize()));
            
            return hashBuilder.ComputeHash();
        }

        private byte[] ComputeSignature(AssemblyHashAlgorithm hashAlgorithm, byte[] hash)
        {
            // Translate strong name private key to RSA parameters.
            using var rsa = RSA.Create();
            var rsaParameters = PrivateKey.ToRsaParameters();
            rsa.ImportParameters(rsaParameters);

            // Compute the signature.
            var formatter = new RSAPKCS1SignatureFormatter(rsa);
            formatter.SetHashAlgorithm(hashAlgorithm.ToString().ToUpperInvariant());
            var signature = formatter.CreateSignature(hash);
            Array.Reverse(signature);

            return signature;
        }

    }
}