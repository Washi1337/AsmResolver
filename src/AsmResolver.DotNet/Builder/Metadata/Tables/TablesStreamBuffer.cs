using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Builder.Metadata;
using AsmResolver.DotNet.Signatures.Marshal;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Builder.Metadata.Tables
{
    /// <summary>
    /// Provides a mutable buffer for building up a tables stream in a .NET portable executable. 
    /// </summary>
    public class TablesStreamBuffer : IMetadataStreamBuffer
    {
        private static readonly TableIndex[] EncTables =
        {
            TableIndex.FieldPtr, TableIndex.MethodPtr, TableIndex.ParamPtr, TableIndex.EventPtr, TableIndex.PropertyPtr,
            TableIndex.EncLog, TableIndex.EncMap,
        };
            
        private readonly TablesStream _tablesStream = new TablesStream();
        private readonly IMetadataTableBuffer[] _tableBuffers;
        
        /// <summary>
        /// Creates a new mutable tables stream buffer.
        /// </summary>
        public TablesStreamBuffer()
        {
            _tableBuffers = new IMetadataTableBuffer[]
            {
                Unsorted<ModuleDefinitionRow>(TableIndex.Module),
                Distinct<TypeReferenceRow>(TableIndex.TypeRef),
                Unsorted<TypeDefinitionRow>(TableIndex.TypeDef),
                Unsorted<FieldPointerRow>(TableIndex.FieldPtr),
                Unsorted<FieldDefinitionRow>(TableIndex.Field),
                Unsorted<MethodPointerRow>(TableIndex.MethodPtr),
                Unsorted<MethodDefinitionRow>(TableIndex.Method),
                Unsorted<ParameterPointerRow>(TableIndex.ParamPtr),
                Unsorted<ParameterDefinitionRow>(TableIndex.Param),
                Sorted<InterfaceImplementation, InterfaceImplementationRow>(TableIndex.InterfaceImpl, 0),
                Distinct<MemberReferenceRow>(TableIndex.MemberRef),
                Sorted<Constant, ConstantRow>(TableIndex.Constant, 2),
                Sorted<CustomAttribute, CustomAttributeRow>(TableIndex.CustomAttribute, 0),
                Sorted<IHasFieldMarshal, FieldMarshalRow>(TableIndex.FieldMarshal, 0),
                Sorted<SecurityDeclaration, SecurityDeclarationRow>(TableIndex.DeclSecurity, 1),
                Sorted<ClassLayout, ClassLayoutRow>(TableIndex.ClassLayout, 2),
                Sorted<FieldDefinition, FieldLayoutRow>(TableIndex.FieldLayout, 1),
                Distinct<StandAloneSignatureRow>(TableIndex.StandAloneSig),
                Sorted<TypeDefinition, EventMapRow>(TableIndex.EventMap, 0, 1),
                Unsorted<EventPointerRow>(TableIndex.EventPtr),
                Unsorted<EventDefinitionRow>(TableIndex.Event),
                Sorted<TypeDefinition, PropertyMapRow>(TableIndex.PropertyMap, 0, 1),
                Unsorted<PropertyPointerRow>(TableIndex.PropertyPtr),
                Unsorted<PropertyDefinitionRow>(TableIndex.Property),
                Sorted<MethodSemantics, MethodSemanticsRow>(TableIndex.MethodSemantics, 2),
                Sorted<MethodImplementation, MethodImplementationRow>(TableIndex.MethodImpl, 0),
                Distinct<ModuleReferenceRow>(TableIndex.ModuleRef),
                Distinct<TypeSpecificationRow>(TableIndex.TypeSpec),
                Sorted<ImplementationMap, ImplementationMapRow>(TableIndex.ImplMap, 1),
                Sorted<FieldDefinition, FieldRvaRow>(TableIndex.FieldRva, 1),
                Unsorted<EncLogRow>(TableIndex.EncLog),
                Unsorted<EncMapRow>(TableIndex.EncMap),
                Unsorted<AssemblyDefinitionRow>(TableIndex.Assembly),
                Unsorted<AssemblyProcessorRow>(TableIndex.AssemblyProcessor),
                Unsorted<AssemblyOSRow>(TableIndex.AssemblyOS),
                Distinct<AssemblyReferenceRow>(TableIndex.AssemblyRef),
                Unsorted<AssemblyRefProcessorRow>(TableIndex.AssemblyRefProcessor),
                Unsorted<AssemblyRefOSRow>(TableIndex.AssemblyRefOS),
                Distinct<FileReferenceRow>(TableIndex.File),
                Distinct<ExportedTypeRow>(TableIndex.ExportedType),
                Distinct<ManifestResourceRow>(TableIndex.ManifestResource),
                Sorted<TypeDefinition, NestedClassRow>(TableIndex.NestedClass, 0),
                Sorted<GenericParameter, GenericParameterRow>(TableIndex.GenericParam, 2, 0),
                Distinct<MethodSpecificationRow>(TableIndex.MethodSpec),
                Distinct<GenericParameterConstraintRow>(TableIndex.GenericParamConstraint),
            };
        }

        /// <inheritdoc />
        public string Name => HasEnCData ? TablesStream.EncStreamName : TablesStream.CompressedStreamName;

        /// <summary>
        /// Gets a value indicating whether the buffer contains edit-and-continue data.
        /// </summary>
        public bool HasEnCData
        {
            get
            {
                for (int i = 0; i < EncTables.Length; i++)
                {
                    if (_tableBuffers[(int) EncTables[i]].Count > 0)
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets a table buffer by its table index.
        /// </summary>
        /// <param name="table">The index of the table to get.</param>
        /// <typeparam name="TRow">The type of rows the table stores.</typeparam>
        /// <returns>The metadata table.</returns>
        public IMetadataTableBuffer<TRow> GetTable<TRow>(TableIndex table)
            where TRow : struct, IMetadataRow
        {
            return (IMetadataTableBuffer<TRow>) _tableBuffers[(int) table];
        }

        /// <summary>
        /// Gets a table buffer by its table index.
        /// </summary>
        /// <param name="table">The index of the table to get.</param>
        /// <typeparam name="TKey">The type of members that are assigned new metadata rows.</typeparam>
        /// <typeparam name="TRow">The type of rows the table stores.</typeparam>
        /// <returns>The metadata table.</returns>
        public ISortedMetadataTableBuffer<TKey, TRow> GetSortedTable<TKey, TRow>(TableIndex table)
            where TRow : struct, IMetadataRow
        {
            return (ISortedMetadataTableBuffer<TKey, TRow>) _tableBuffers[(int) table];
        }

        /// <summary>
        /// Gets an encoder/decoder for a particular coded index.
        /// </summary>
        /// <param name="codedIndex">The coded index.</param>
        /// <returns>The encoder/decoder object.</returns>
        public IndexEncoder GetIndexEncoder(CodedIndex codedIndex)
        {
            return _tablesStream.GetIndexEncoder(codedIndex);
        }

        /// <summary>
        /// Serializes the tables stream buffer to a metadata stream.
        /// </summary>
        /// <returns>The stream.</returns>
        public TablesStream CreateStream()
        {
            foreach (var tableBuffer in _tableBuffers)
                tableBuffer.FlushToTable();
            _tablesStream.Name = Name;
            return _tablesStream;
        }

        /// <inheritdoc />
        IMetadataStream IMetadataStreamBuffer.CreateStream() => CreateStream();

        private IMetadataTableBuffer<TRow> Unsorted<TRow>(TableIndex table) 
            where TRow : struct, IMetadataRow
        {
            return new UnsortedMetadataTableBuffer<TRow>((MetadataTable<TRow>) _tablesStream.GetTable(table));
        }

        private IMetadataTableBuffer<TRow> Distinct<TRow>(TableIndex table) 
            where TRow : struct, IMetadataRow
        {
            return new DistinctMetadataTableBuffer<TRow>(Unsorted<TRow>(table));
        }

        private ISortedMetadataTableBuffer<TKey, TRow> Sorted<TKey, TRow>(TableIndex table, int primaryColumn)
            where TRow : struct, IMetadataRow
        {
            return new SortedMetadataTableBuffer<TKey, TRow>(
                (MetadataTable<TRow>) _tablesStream.GetTable(table), 
                primaryColumn);
        }

        private ISortedMetadataTableBuffer<TKey, TRow> Sorted<TKey, TRow>(TableIndex table, int primaryColumn, int secondaryColumn)
            where TRow : struct, IMetadataRow 
        {
            return new SortedMetadataTableBuffer<TKey, TRow>(
                (MetadataTable<TRow>) _tablesStream.GetTable(table), 
                primaryColumn,
                secondaryColumn);
        }
    }
}