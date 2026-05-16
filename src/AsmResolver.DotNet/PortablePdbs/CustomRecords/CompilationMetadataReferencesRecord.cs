using System;
using System.Collections.Generic;
using AsmResolver.DotNet.PortablePdbs.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.IO;
using AsmResolver.Shims;

namespace AsmResolver.DotNet.PortablePdbs.CustomRecords;

public class CompilationMetadataReferencesRecord : CustomDebugRecord
{
    public static Guid KnownKind { get; } = new("7E4D4708-096E-4C5C-AEDA-CB10BA6A740D");

    public override Guid Kind => KnownKind;

    public override bool HasBlob => true;

    public MetadataReferenceInfo[]? References { get; set; }

    public static CompilationMetadataReferencesRecord FromReader(PdbReaderContext context, ref BinaryStreamReader reader)
    {
        var referenceList = new List<MetadataReferenceInfo>();
        while (reader.CanRead(1))
        {
            var fileName = reader.ReadUtf8String();
            var aliasesString = reader.ReadUtf8String();
            Utf8String[] aliases;
            if (aliasesString.GetBytesUnsafe().Contains((byte)','))
            {
                aliases = Array.ConvertAll(aliasesString.Value.Split(','), alias => (Utf8String)alias);
            }
            else if (aliasesString.Length != 0)
            {
                aliases = [aliasesString];
            }
            else
            {
                aliases = ArrayShim.Empty<Utf8String>();
            }
            var flags = (MetadataReferenceFlags)reader.ReadByte();
            var timeStamp = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(reader.ReadUInt32());
            var fileSize = reader.ReadUInt32();
            var mvid = new Guid(reader.ReadBytes(16));

            referenceList.Add(new MetadataReferenceInfo
            {
                FileName = fileName,
                Aliases = aliases,
                Flags = flags,
                TimeStamp = timeStamp,
                FileSize = fileSize,
                Mvid = mvid,
            });
        }

        return new CompilationMetadataReferencesRecord
        {
            References = referenceList.ToArray(),
        };
    }

    protected override void WriteContents(in BlobSerializationContext context)
    {
        throw new NotImplementedException();
    }

    public struct MetadataReferenceInfo
    {
        public Utf8String? FileName { get; set; }
        public Utf8String[]? Aliases { get; set; }
        public MetadataReferenceFlags Flags { get; set; }
        public DateTime TimeStamp { get; set; }
        public uint FileSize { get; set; }
        public Guid Mvid { get; set; }
    }

    public enum MetadataReferenceFlags
    {
        None = 0,
        AssemblyReference = 1,
        EmbeddedInteropTypes = 2,
    }
}
