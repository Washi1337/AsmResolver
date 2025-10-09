using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs.Serialized
{
    public class SerializedDocument : Document
    {
        private readonly PdbReaderContext _context;
        private readonly DocumentRow _row;

        public SerializedDocument(PdbReaderContext context, MetadataToken token, in DocumentRow row) : base(token)
        {
            _context = context;
            _row = row;
        }

        protected override Utf8String GetName()
        {
            var blobStream = _context.BlobStream!;
            if (!blobStream.TryGetBlobReaderByIndex(_row.Name, out var namePartsReader))
            {
                return Utf8String.Empty;
            }

            var name = new List<byte>();

            var sep = namePartsReader.ReadByte();
            var hasDoneFirstPart = false;
            while (namePartsReader.RelativeOffset != namePartsReader.Length)
            {
                var partIndex = namePartsReader.ReadCompressedUInt32();
                if (partIndex == 0 || !blobStream.TryGetBlobReaderByIndex(partIndex, out var partReader))
                {
                    if (sep != 0 && hasDoneFirstPart)
                    {
                        name.Add(sep);
                    }
                    hasDoneFirstPart = true;
                    continue;
                }

                if (sep != 0 && hasDoneFirstPart)
                {
                    name.Add(sep);
                }
                hasDoneFirstPart = true;

                var part = partReader.ReadToEnd();
                name.AddRange(part);
            }

            return new Utf8String(name.ToArray());
        }

        protected override Guid GetHashAlgorithm() => _context.GuidStream!.GetGuidByIndex(_row.HashAlgorithm);

        protected override byte[]? GetHash() => _context.BlobStream!.GetBlobByIndex(_row.Hash);

        protected override Guid GetLanguage() => _context.GuidStream!.GetGuidByIndex(_row.Language);
    }
}
