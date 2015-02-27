using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net
{
    public class TableStream : MetadataStream
    {
        internal const MetadataTokenType NotUsed = (MetadataTokenType)(-1);

        internal static TableStream FromReadingContext(ReadingContext context)
        {
            var reader = context.Reader;
            var stream = new TableStream
            {
                StartOffset = reader.Position,

                Reserved = reader.ReadUInt32(),
                MajorVersion = reader.ReadByte(),
                MinorVersion = reader.ReadByte(),
                HeapSizes = reader.ReadByte(),
                Reserved2 = reader.ReadByte(),
                ValidBitVector = reader.ReadUInt64(),
                SortedBitVector = reader.ReadUInt64(),
            };

            var presentTables = stream.GetPresentTables().ToArray();
            var currentOffset = reader.Position + (presentTables.Length * sizeof (uint));

            foreach (var table in presentTables)
                table.SetMemberCount(reader.ReadUInt32());

            foreach (var table in presentTables)
            {
                var size = (int)table.GetPhysicalLength();
                var tableContext = context.CreateSubContext(currentOffset, size);

                table.SetReadingContext(tableContext);

                currentOffset += size;
            }

            return stream;
        }

        private readonly MetadataTable[] _tables = new MetadataTable[45]
        {
            new ModuleDefinitionTable(),
            new TypeReferenceTable(),
            new TypeDefinitionTable(), 
            new FieldPtrTable(), 
            new FieldDefinitionTable(), 
            new MethodPtrTable(), 
            new MethodDefinitionTable(), 
            new ParamPtrTable(), 
            new ParameterDefinitionTable(), 
            new InterfaceImplementationTable(), 
            new MemberReferenceTable(), 
            new ConstantTable(), 
            new CustomAttributeTable(), 
            new FieldMarshalTable(), 
            new SecurityDeclarationTable(), 
            new ClassLayoutTable(), 
            new FieldLayoutTable(), 
            new StandAloneSignatureTable(),
            new EventMapTable(), 
            new EventPtrTable(), 
            new EventDefinitionTable(), 
            new PropertyMapTable(), 
            new PropertyPtrTable(), 
            new PropertyDefinitionTable(), 
            new MethodSemanticsTable(), 
            new MethodImplementationTable(), 
            new ModuleReferenceTable(), 
            new TypeSpecificationTable(), 
            new PInvokeImplementationTable(), 
            new FieldRvaTable(), 
            new EncLogTable(), 
            new EncMapTable(), 
            new AssemblyDefinitionTable(), 
            new AssemblyProcessorTable(), 
            new AssemblyOsTable(), 
            new AssemblyReferenceTable(), 
            new AssemblyRefProcessorTable(), 
            new AssemblyRefOsTable(), 
            new FileDefinitionTable(), 
            new ExportedTypeTable(), 
            new ManifestResourceTable(), 
            new NestedClassTable(), 
            new GenericParameterTable(), 
            new MethodSpecificationTable(), 
            new GenericParameterConstraintTable(),
        };

        private readonly IndexEncoder[] _encoders;

        public TableStream()
        {
            foreach (var table in _tables)
                if (table != null)
                    table.TableStream = this;
            _encoders = new IndexEncoder[]
            {
                new IndexEncoder(this, MetadataTokenType.TypeDef, MetadataTokenType.TypeRef,
                    MetadataTokenType.TypeSpec),

                new IndexEncoder(this, MetadataTokenType.Field, MetadataTokenType.Param, MetadataTokenType.Property),

                new IndexEncoder(this, MetadataTokenType.Method, MetadataTokenType.Field,
                    MetadataTokenType.TypeRef, MetadataTokenType.TypeDef, MetadataTokenType.Param,
                    MetadataTokenType.InterfaceImpl, MetadataTokenType.MemberRef, MetadataTokenType.Module,
                    MetadataTokenType.DeclSecurity, MetadataTokenType.Property, MetadataTokenType.Event,
                    MetadataTokenType.StandAloneSig, MetadataTokenType.ModuleRef, MetadataTokenType.TypeSpec, 
                    MetadataTokenType.Assembly, MetadataTokenType.AssemblyRef, MetadataTokenType.File,
                    MetadataTokenType.ExportedType, MetadataTokenType.ManifestResource),

                new IndexEncoder(this, MetadataTokenType.Field, MetadataTokenType.Param),

                new IndexEncoder(this, MetadataTokenType.TypeDef, MetadataTokenType.Method,
                    MetadataTokenType.Assembly),

                new IndexEncoder(this, MetadataTokenType.TypeDef, MetadataTokenType.TypeRef,
                    MetadataTokenType.ModuleRef, MetadataTokenType.Method, MetadataTokenType.TypeSpec),

                new IndexEncoder(this, MetadataTokenType.Event, MetadataTokenType.Property),

                new IndexEncoder(this, MetadataTokenType.Method, MetadataTokenType.MemberRef),

                new IndexEncoder(this, MetadataTokenType.Field, MetadataTokenType.Method),

                new IndexEncoder(this, MetadataTokenType.File, MetadataTokenType.AssemblyRef,
                    MetadataTokenType.ExportedType),

                new IndexEncoder(this, NotUsed, NotUsed, MetadataTokenType.Method,
                    MetadataTokenType.MemberRef, NotUsed),

                new IndexEncoder(this, MetadataTokenType.Module, MetadataTokenType.ModuleRef,
                    MetadataTokenType.AssemblyRef, MetadataTokenType.TypeRef),

                new IndexEncoder(this, MetadataTokenType.TypeDef, MetadataTokenType.Method),

            };
        }

        public uint Reserved
        {
            get;
            set;
        }

        public byte MajorVersion
        {
            get;
            set;
        }

        public byte MinorVersion
        {
            get;
            set;
        }

        public byte HeapSizes
        {
            get;
            set;
        }

        public byte Reserved2
        {
            get;
            set;
        }

        public ulong ValidBitVector
        {
            get;
            set;
        }

        public ulong SortedBitVector
        {
            get;
            set;
        }

        public IndexEncoder GetIndexEncoder(CodedIndex index)
        {
            return _encoders[(int)index];
        }

        private IndexSize GetIndexSize(int bit)
        {
            var bitmask = 1 << bit;
            return (HeapSizes & bitmask) == bitmask ? IndexSize.Long : IndexSize.Short;
        }

        private void SetIndexSize(int bit, IndexSize value)
        {
            var bitmask = 0;
            switch (value)
            {
                case IndexSize.Short:
                    break;
                case IndexSize.Long:
                    bitmask = 1 << bit;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("value");
            }
            HeapSizes = (byte)((HeapSizes & ~(1 << bit)) | bitmask);
        }

        public IndexSize StringIndexSize
        {
            get { return GetIndexSize(0); }
            set { SetIndexSize(0, value); }
        }

        public IndexSize GuidIndexSize
        {
            get { return GetIndexSize(1); }
            set { SetIndexSize(1, value); }
        }

        public IndexSize BlobIndexSize
        {
            get { return GetIndexSize(2); }
            set { SetIndexSize(2, value); }
        }

        public IEnumerable<MetadataTable> GetPresentTables()
        {
            for (int i = 0; i < _tables.Length; i++)
                if (((ValidBitVector & (1ul << i)) == (1ul << i)))
                    yield return _tables[i];
        }

        public MetadataTable GetTable(MetadataTokenType tokenType)
        {
            return _tables[(int)tokenType];
        }

        public MetadataTable<TElement> GetTable<TElement>()
            where TElement : MetadataMember
        {
            return (MetadataTable<TElement>)_tables.First(x => x is MetadataTable<TElement>);
        }

        public MetadataMember ResolveMember(MetadataToken token)
        {
            var table = GetTable(token.TokenType);
            return table.GetMember((int)(token.Rid - 1));
        }

        public override uint GetPhysicalLength()
        {
            var presentTables = GetPresentTables().ToArray();
            return Align((uint)(1 * sizeof (uint) +
                                4 * sizeof (byte) +
                                2 * sizeof (ulong) +
                                presentTables.Length * sizeof (uint) +
                                presentTables.Sum(x => x.GetPhysicalLength())), 4);
        }

        public virtual ulong ComputeValidBitVector()
        {
            ulong vector = 0;
            for (int i = 0; i < _tables.Length; i++)
                if (_tables[i].Count > 0)
                    vector |= (1ul << i);
            return vector;
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteUInt32(Reserved);
            writer.WriteByte(MajorVersion);
            writer.WriteByte(MinorVersion);
            writer.WriteByte(HeapSizes);
            writer.WriteByte(Reserved2);
            writer.WriteUInt64(ValidBitVector);
            writer.WriteUInt64(SortedBitVector);

            var presentTables = GetPresentTables().ToArray();

            foreach (var table in presentTables)
                writer.WriteUInt32((uint)table.Count);

            foreach (var table in presentTables)
                table.Write(context);

            writer.WriteUInt32(0);
        }
    }
}
