using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.PE.DotNet.StrongName
{
    internal class StrongNameDataHashBuilder
    {
        private readonly Stream _imageStream;
        private readonly AssemblyHashAlgorithm _hashAlgorithm;

        private readonly List<OffsetRange> _includedRanges = new List<OffsetRange>();
        private readonly List<OffsetRange> _zeroRanges = new List<OffsetRange>();

        public StrongNameDataHashBuilder(Stream imageStream, AssemblyHashAlgorithm hashAlgorithm)
        {
            _imageStream = imageStream ?? throw new ArgumentNullException(nameof(imageStream));
            _hashAlgorithm = hashAlgorithm;
        }

        public void IncludeRange(OffsetRange range)
        {
            _includedRanges.Add(range);
        }

        public void ExcludeRange(OffsetRange range)
        {
            for (int i = 0; i < _includedRanges.Count; i++)
            {
                var includedRange =  _includedRanges[i];
                var (left, right) = includedRange.Exclude(range);
                if (left.IsEmpty)
                {
                    _includedRanges[i] = right;
                }
                else if (right.IsEmpty)
                {
                    _includedRanges[i] = left;
                }
                else
                {
                    _includedRanges[i] = left;
                    _includedRanges.Insert(i + 1, right);
                    i++;
                }
            }
        }

        public void ZeroRange(OffsetRange range)
        {
            _zeroRanges.Add(range);
        }

        public byte[] ComputeHash()
        {
            using HashAlgorithm algorithm = _hashAlgorithm switch
            {
                AssemblyHashAlgorithm.Md5 => MD5.Create(),
                AssemblyHashAlgorithm.Sha1 => SHA1.Create(),
                AssemblyHashAlgorithm.Sha256 => SHA256.Create(),
                AssemblyHashAlgorithm.Sha384 => SHA384.Create(),
                AssemblyHashAlgorithm.Sha512 => SHA512.Create(),
                _ => throw new NotSupportedException($"Invalid or unsupported hashing algorithm {_hashAlgorithm}.")
            };
            
            var buffer = new byte[0x1000];

            foreach (var range in _includedRanges)
            {
                _imageStream.Position = range.Start;
                while (_imageStream.Position < range.End)
                {
                    int chunkLength = Math.Min(buffer.Length, (int) (range.End - _imageStream.Position));
                    var currentRange = new OffsetRange(
                        (uint) _imageStream.Position,
                        (uint) (_imageStream.Position + chunkLength));
                    
                    _imageStream.Read(buffer, 0, chunkLength);
                    
                    ZeroRangesIfApplicable(buffer, currentRange);
                    algorithm.TransformBlock(buffer, 0, chunkLength, buffer, 0);
                }
            }

            algorithm.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            return algorithm.Hash;
        }

        private void ZeroRangesIfApplicable(byte[] buffer, OffsetRange currentRange)
        {
            foreach (var range in _zeroRanges)
            {
                if (currentRange.Intersects(range))
                {
                    var intersection = currentRange.Intersect(range);
                    for (uint i = intersection.Start; i < intersection.End; i++)
                        buffer[i] = 0;
                }
            }
        }
        
    }
}