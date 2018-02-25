using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net
{
    /// <summary>
    /// Represents a tables storage stream (#~ or #-) in a .NET assembly image.
    /// </summary>
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
                table.SetRowCount(reader.ReadUInt32());

            foreach (var table in presentTables)
            {
                var size = (int)table.GetPhysicalLength();
                var tableContext = context.CreateSubContext(currentOffset, size);

                table.SetReadingContext(tableContext);

                currentOffset += size;
            }

            return stream;
        }

        private uint _reserved;
        private byte _majorVersion;
        private byte _minorVersion;
        private byte _heapSizes;
        private byte _reserved2;
        private ulong _validBitVector;
        private ulong _sortedBitVector;

        private readonly IndexEncoder[] _encoders;

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
            new ImplementationMapTable(), 
            new FieldRvaTable(), 
            new EncLogTable(), 
            new EncMapTable(), 
            new AssemblyDefinitionTable(), 
            new AssemblyProcessorTable(), 
            new AssemblyOsTable(), 
            new AssemblyReferenceTable(), 
            new AssemblyRefProcessorTable(), 
            new AssemblyRefOsTable(), 
            new FileReferenceTable(), 
            new ExportedTypeTable(), 
            new ManifestResourceTable(), 
            new NestedClassTable(), 
            new GenericParameterTable(), 
            new MethodSpecificationTable(), 
            new GenericParameterConstraintTable(),
        };

        private bool _isReadOnly;

        public TableStream()
        {
            foreach (var table in _tables)
                table.TableStream = this;
            
            _encoders = new []
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

            MajorVersion = 2;
            Reserved2 = 1;
            SortedBitVector = 0x000016003301FA00;
        }

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            internal set
            {
                if (_isReadOnly != value)
                {
                    _isReadOnly = value;
                    foreach (var table in _tables)
                        table.IsReadOnly = value;
                }
            }
        }

        /// <summary>
        /// Reserved, should be zero.
        /// </summary>
        public uint Reserved
        {
            get { return _reserved; }
            set
            {
                AssertIsWriteable();
                _reserved = value;
            }
        }

        /// <summary>
        /// Gets or sets the major version of table schemata. Shall be 2.
        /// </summary>
        public byte MajorVersion
        {
            get { return _majorVersion; }
            set
            {
                AssertIsWriteable();
                _majorVersion = value;
            }
        }

        /// <summary>
        /// Gets or sets the minor version of table schemata. Shall be 0.
        /// </summary>
        public byte MinorVersion
        {
            get { return _minorVersion; }
            set
            {
                AssertIsWriteable();
                _minorVersion = value;
            }
        }

        /// <summary>
        /// Gets or sets the bit vector indicating the sizes of the indices to data in the strings, user-strings, blob and guid streams.
        /// </summary>
        public byte HeapSizes
        {
            get { return _heapSizes; }
            set
            {
                AssertIsWriteable();
                _heapSizes = value;
            }
        }

        /// <summary>
        /// Reserved, should be 1.
        /// </summary>
        public byte Reserved2
        {
            get { return _reserved2; }
            set
            {
                AssertIsWriteable();
                _reserved2 = value;
            }
        }

        /// <summary>
        /// Gets or sets the bit vector indicating the tables that are present in the tables storage stream.
        /// </summary>
        public ulong ValidBitVector
        {
            get { return _validBitVector; }
            set
            {
                AssertIsWriteable();
                _validBitVector = value;
            }
        }

        /// <summary>
        /// Gets or sets the bit vector indicating the tables that are sorted.
        /// </summary>
        public ulong SortedBitVector
        {
            get { return _sortedBitVector; }
            set
            {
                AssertIsWriteable();
                _sortedBitVector = value;
            }
        }

        /// <summary>
        /// Gets the index encoder for a specific type of coded index.
        /// </summary>
        /// <param name="index">The index type to get the encoder for.</param>
        /// <returns></returns>
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
            AssertIsWriteable();
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

        /// <summary>
        /// Gets or sets the size of indices to a string in the strings storage stream.
        /// </summary>
        public IndexSize StringIndexSize
        {
            get { return GetIndexSize(0); }
            set { SetIndexSize(0, value); }
        }

        /// <summary>
        /// Gets or sets the size of indices to a GUID in the GUIDs storage stream.
        /// </summary>
        public IndexSize GuidIndexSize
        {
            get { return GetIndexSize(1); }
            set { SetIndexSize(1, value); }
        }

        /// <summary>
        /// Gets or sets the size of indices to a blob in the blobs storage stream.
        /// </summary>
        public IndexSize BlobIndexSize
        {
            get { return GetIndexSize(2); }
            set { SetIndexSize(2, value); }
        }

        /// <summary>
        /// Enumerates all tables present in the table stream.
        /// </summary>
        /// <returns>The tables in the table stream.</returns>
        public IEnumerable<MetadataTable> GetPresentTables()
        {
            for (int i = 0; i < _tables.Length; i++)
                if (((ValidBitVector & (1ul << i)) == (1ul << i)))
                    yield return _tables[i];
        }

        /// <summary>
        /// Gets a table of a specific member type.
        /// </summary>
        /// <param name="tokenType">The member type of the table to get.</param>
        /// <returns>The table.</returns>
        public MetadataTable GetTable(MetadataTokenType tokenType)
        {
            return _tables[(int)tokenType];
        }

        /// <summary>
        /// Gets the table instance given a specific table type.
        /// </summary>
        /// <typeparam name="TTable">The table type to get.</typeparam>
        /// <returns>The table.</returns>
        public TTable GetTable<TTable>()
            where TTable : MetadataTable
        {
            return (TTable)_tables.First(x => x is TTable);
        }

        /// <summary>
        /// Tries to resolve a metadata row by a metadata token.
        /// </summary>
        /// <param name="token">The token to resolve.</param>
        /// <param name="member">The resolved metadata row, or null if resolution failed.</param>
        /// <returns><c>True</c> if resolution succeeded, <c>False</c> otherwise.</returns>
        public bool TryResolveRow(MetadataToken token, out MetadataRow member)
        {
            var table = GetTable(token.TokenType);
            return table.TryGetRow((int)(token.Rid - 1), out member);
        }

        /// <summary>
        /// Resolves a metadata row by a metadata token.
        /// </summary>
        /// <param name="token">The token to resolve.</param>
        /// <returns>The resolved metadata row.</returns>
        public MetadataRow ResolveRow(MetadataToken token)
        {
            var table = GetTable(token.TokenType);
            return table.GetRow((int)(token.Rid - 1));
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

        private void AssertIsWriteable()
        {
            if (IsReadOnly)
                throw new InvalidOperationException("Table stream cannot be modified in read-only mode.");
        }
    }
}
