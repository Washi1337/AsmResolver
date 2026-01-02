using System.Collections;
using System.Collections.Generic;
using AsmResolver.DotNet.PortablePdbs.Serialized;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs
{
    public class ImportsCollection : IList<Import>, IReadOnlyList<Import>
    {
        private List<Import> _imports = new();

        public IEnumerator<Import> GetEnumerator() => _imports.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(Import item) => _imports.Add(item);
        public void Clear() => _imports.Clear();
        public bool Contains(Import item) => _imports.Contains(item);

        public void CopyTo(Import[] array, int arrayIndex) => _imports.CopyTo(array, arrayIndex);
        public bool Remove(Import item) => _imports.Remove(item);

        public int Count => _imports.Count;

        bool ICollection<Import>.IsReadOnly => false;

        public int IndexOf(Import item) => _imports.IndexOf(item);

        public void Insert(int index, Import item) => _imports.Insert(index, item);
        public void RemoveAt(int index) => _imports.RemoveAt(index);

        public Import this[int index]
        {
            get => _imports[index];
            set => _imports[index] = value;
        }

        public static ImportsCollection FromReader(PdbReaderContext context, ImportScope scope, ref BinaryStreamReader reader)
        {
            var imports = new ImportsCollection();

            var encoder = context.TablesStream.GetIndexEncoder(CodedIndex.TypeDefOrRef);

            while (reader.CanRead(1))
            {
                Utf8String? alias = null;
                AssemblyReference? assembly = null;
                Utf8String? targetNamespace = null;
                ITypeDefOrRef? targetType = null;
                var kind = (ImportKind)reader.ReadCompressedUInt32();
                switch (kind)
                {
                    case ImportKind.ImportNamespace:
                        ReadTargetNamespace(ref reader);
                        break;
                    case ImportKind.ImportAssemblyNamespace:
                        ReadAssembly(ref reader);
                        ReadTargetNamespace(ref reader);
                        break;
                    case ImportKind.ImportType:
                        ReadTargetType(ref reader);
                        break;
                    case ImportKind.ImportXmlNamespace:
                        ReadAlias(ref reader);
                        ReadTargetNamespace(ref reader);
                        break;
                    case ImportKind.ImportAssemblyReferenceAlias:
                        ReadAlias(ref reader);
                        break;
                    case ImportKind.AliasAssemblyReference:
                        ReadAlias(ref reader);
                        ReadAssembly(ref reader);
                        break;
                    case ImportKind.AliasNamespace:
                        ReadAlias(ref reader);
                        ReadTargetNamespace(ref reader);
                        break;
                    case ImportKind.AliasAssemblyNamespace:
                        ReadAlias(ref reader);
                        ReadAssembly(ref reader);
                        ReadTargetNamespace(ref reader);
                        break;
                    case ImportKind.AliasType:
                        ReadAlias(ref reader);
                        ReadTargetType(ref reader);
                        break;
                    default:
                        context.BadImage($"Unknown ImportKind {kind}");
                        break;
                }
                imports.Add(new Import
                {
                    Kind = kind,
                    Alias = alias,
                    Assembly = assembly,
                    TargetNamespace = targetNamespace,
                    TargetType = targetType,
                });

                void ReadAlias(ref BinaryStreamReader reader)
                {
                    if (context.BlobStream!.TryGetBlobReaderByIndex(reader.ReadCompressedUInt32(), out var aliasReader))
                        alias = aliasReader.ReadUtf8String();
                }
                void ReadAssembly(ref BinaryStreamReader reader)
                {
                    assembly = context.OwningModule.LookupMember<AssemblyReference>(new MetadataToken(TableIndex.AssemblyRef, reader.ReadCompressedUInt32()));
                }
                void ReadTargetNamespace(ref BinaryStreamReader reader)
                {
                    if (context.BlobStream!.TryGetBlobReaderByIndex(reader.ReadCompressedUInt32(), out var namespaceReader))
                        targetNamespace = namespaceReader.ReadUtf8String();
                }
                void ReadTargetType(ref BinaryStreamReader reader)
                {
                    targetType = context.OwningModule.LookupMember<ITypeDefOrRef>(encoder.DecodeIndex(reader.ReadCompressedUInt32()));
                }
            }

            return imports;
        }
    }
}
