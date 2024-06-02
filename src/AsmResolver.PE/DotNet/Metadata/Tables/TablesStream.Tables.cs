using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    public partial class TablesStream
    {
        /// <summary>
        /// Obtains the collection of tables in the tables stream.
        /// </summary>
        /// <returns>The tables, including empty tables if there are any.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Tables"/> property.
        /// </remarks>
        protected virtual IList<IMetadataTable?> GetTables()
        {
            var layouts = TableLayouts;
            return new IMetadataTable?[]
            {
                new MetadataTable<ModuleDefinitionRow>(TableIndex.Module, layouts[0]),
                new MetadataTable<TypeReferenceRow>(TableIndex.TypeRef, layouts[1]),
                new MetadataTable<TypeDefinitionRow>(TableIndex.TypeDef, layouts[2]),
                new MetadataTable<FieldPointerRow>(TableIndex.FieldPtr, layouts[3]),
                new MetadataTable<FieldDefinitionRow>(TableIndex.Field, layouts[4]),
                new MetadataTable<MethodPointerRow>(TableIndex.Method, layouts[5]),
                new MetadataTable<MethodDefinitionRow>(TableIndex.Method, layouts[6]),
                new MetadataTable<ParameterPointerRow>(TableIndex.ParamPtr, layouts[7]),
                new MetadataTable<ParameterDefinitionRow>(TableIndex.Param, layouts[8]),
                new MetadataTable<InterfaceImplementationRow>(TableIndex.InterfaceImpl, layouts[9], true),
                new MetadataTable<MemberReferenceRow>(TableIndex.MemberRef, layouts[10]),
                new MetadataTable<ConstantRow>(TableIndex.Constant, layouts[11], true),
                new MetadataTable<CustomAttributeRow>(TableIndex.CustomAttribute, layouts[12], true),
                new MetadataTable<FieldMarshalRow>(TableIndex.FieldMarshal, layouts[13], true),
                new MetadataTable<SecurityDeclarationRow>(TableIndex.DeclSecurity, layouts[14], true),
                new MetadataTable<ClassLayoutRow>(TableIndex.ClassLayout, layouts[15], true),
                new MetadataTable<FieldLayoutRow>(TableIndex.FieldLayout, layouts[16], true),
                new MetadataTable<StandAloneSignatureRow>(TableIndex.StandAloneSig, layouts[17]),
                new MetadataTable<EventMapRow>(TableIndex.EventMap, layouts[18]),
                new MetadataTable<EventPointerRow>(TableIndex.EventPtr, layouts[19]),
                new MetadataTable<EventDefinitionRow>(TableIndex.Event, layouts[20]),
                new MetadataTable<PropertyMapRow>(TableIndex.PropertyMap, layouts[21]),
                new MetadataTable<PropertyPointerRow>(TableIndex.PropertyPtr, layouts[22]),
                new MetadataTable<PropertyDefinitionRow>(TableIndex.Property, layouts[23]),
                new MetadataTable<MethodSemanticsRow>(TableIndex.MethodSemantics, layouts[24], true),
                new MetadataTable<MethodImplementationRow>(TableIndex.MethodImpl, layouts[25], true),
                new MetadataTable<ModuleReferenceRow>(TableIndex.ModuleRef, layouts[26]),
                new MetadataTable<TypeSpecificationRow>(TableIndex.TypeSpec, layouts[27]),
                new MetadataTable<ImplementationMapRow>(TableIndex.ImplMap, layouts[28], true),
                new MetadataTable<FieldRvaRow>(TableIndex.FieldRva, layouts[29], true),
                new MetadataTable<EncLogRow>(TableIndex.EncLog, layouts[30]),
                new MetadataTable<EncMapRow>(TableIndex.EncMap, layouts[31]),
                new MetadataTable<AssemblyDefinitionRow>(TableIndex.Assembly, layouts[32]),
                new MetadataTable<AssemblyProcessorRow>(TableIndex.AssemblyProcessor, layouts[33]),
                new MetadataTable<AssemblyOSRow>(TableIndex.AssemblyOS, layouts[34]),
                new MetadataTable<AssemblyReferenceRow>(TableIndex.AssemblyRef, layouts[35]),
                new MetadataTable<AssemblyRefProcessorRow>(TableIndex.AssemblyRefProcessor, layouts[36]),
                new MetadataTable<AssemblyRefOSRow>(TableIndex.AssemblyRefProcessor, layouts[37]),
                new MetadataTable<FileReferenceRow>(TableIndex.File, layouts[38]),
                new MetadataTable<ExportedTypeRow>(TableIndex.ExportedType, layouts[39]),
                new MetadataTable<ManifestResourceRow>(TableIndex.ManifestResource, layouts[40]),
                new MetadataTable<NestedClassRow>(TableIndex.NestedClass, layouts[41], true),
                new MetadataTable<GenericParameterRow>(TableIndex.GenericParam, layouts[42], true),
                new MetadataTable<MethodSpecificationRow>(TableIndex.MethodSpec, layouts[43]),
                new MetadataTable<GenericParameterConstraintRow>(TableIndex.GenericParamConstraint, layouts[44], true),
                null,
                null,
                null,
                new MetadataTable<DocumentRow>(TableIndex.Document, layouts[48]),
                new MetadataTable<MethodDebugInformationRow>(TableIndex.MethodDebugInformation, layouts[49]),
                new MetadataTable<LocalScopeRow>(TableIndex.LocalScope, layouts[50], true),
                new MetadataTable<LocalVariableRow>(TableIndex.LocalVariable, layouts[51]),
                new MetadataTable<LocalConstantRow>(TableIndex.LocalConstant, layouts[52]),
                new MetadataTable<ImportScopeRow>(TableIndex.ImportScope, layouts[53]),
                new MetadataTable<StateMachineMethodRow>(TableIndex.StateMachineMethod, layouts[54]),
                new MetadataTable<CustomDebugInformationRow>(TableIndex.CustomDebugInformation, layouts[55], true),
            };
        }

        private Dictionary<CodedIndex, IndexEncoder> CreateIndexEncoders()
        {
            return new()
            {
                [CodedIndex.TypeDefOrRef] = new IndexEncoder(this,
                    TableIndex.TypeDef, TableIndex.TypeRef, TableIndex.TypeSpec),

                [CodedIndex.HasConstant] = new(this,
                    TableIndex.Field, TableIndex.Param, TableIndex.Property),

                [CodedIndex.HasCustomAttribute] = new(this,
                    TableIndex.Method, TableIndex.Field, TableIndex.TypeRef, TableIndex.TypeDef,
                    TableIndex.Param, TableIndex.InterfaceImpl, TableIndex.MemberRef, TableIndex.Module,
                    TableIndex.DeclSecurity, TableIndex.Property, TableIndex.Event, TableIndex.StandAloneSig,
                    TableIndex.ModuleRef, TableIndex.TypeSpec, TableIndex.Assembly, TableIndex.AssemblyRef,
                    TableIndex.File, TableIndex.ExportedType, TableIndex.ManifestResource, TableIndex.GenericParam,
                    TableIndex.GenericParamConstraint, TableIndex.MethodSpec),

                [CodedIndex.HasFieldMarshal] = new(this,
                    TableIndex.Field, TableIndex.Param),

                [CodedIndex.HasDeclSecurity] = new(this,
                    TableIndex.TypeDef, TableIndex.Method, TableIndex.Assembly),

                [CodedIndex.MemberRefParent] = new(this,
                    TableIndex.TypeDef, TableIndex.TypeRef, TableIndex.ModuleRef, TableIndex.Method,
                    TableIndex.TypeSpec),

                [CodedIndex.HasSemantics] = new(this,
                    TableIndex.Event, TableIndex.Property),

                [CodedIndex.MethodDefOrRef] = new(this,
                    TableIndex.Method, TableIndex.MemberRef),

                [CodedIndex.MemberForwarded] = new(this,
                    TableIndex.Field, TableIndex.Method),

                [CodedIndex.Implementation] = new(this,
                    TableIndex.File, TableIndex.AssemblyRef, TableIndex.ExportedType),

                [CodedIndex.CustomAttributeType] = new(this,
                    0, 0, TableIndex.Method, TableIndex.MemberRef, 0),

                [CodedIndex.ResolutionScope] = new(this,
                    TableIndex.Module, TableIndex.ModuleRef, TableIndex.AssemblyRef, TableIndex.TypeRef),

                [CodedIndex.TypeOrMethodDef] = new(this,
                    TableIndex.TypeDef, TableIndex.Method),

                [CodedIndex.HasCustomDebugInformation] = new(this,
                    TableIndex.Method, TableIndex.Field, TableIndex.TypeRef, TableIndex.TypeDef, TableIndex.Param,
                    TableIndex.InterfaceImpl, TableIndex.MemberRef, TableIndex.Module, TableIndex.DeclSecurity,
                    TableIndex.Property, TableIndex.Event, TableIndex.StandAloneSig, TableIndex.ModuleRef,
                    TableIndex.TypeSpec, TableIndex.Assembly, TableIndex.AssemblyRef, TableIndex.File,
                    TableIndex.ExportedType, TableIndex.ManifestResource, TableIndex.GenericParam,
                    TableIndex.GenericParamConstraint, TableIndex.MethodSpec, TableIndex.Document,
                    TableIndex.LocalScope, TableIndex.LocalVariable, TableIndex.LocalConstant, TableIndex.ImportScope)
            };
        }

        /// <summary>
        /// Gets an ordered collection of the current table layouts.
        /// </summary>
        /// <returns>The table layouts.</returns>
        protected TableLayout[] GetTableLayouts() => new[]
        {
            // Module
            new TableLayout(
                new ColumnLayout("Generation", ColumnType.UInt16),
                new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                new ColumnLayout("Mvid", ColumnType.Guid, GuidIndexSize),
                new ColumnLayout("EncId", ColumnType.Guid, GuidIndexSize),
                new ColumnLayout("EncBaseId", ColumnType.Guid, GuidIndexSize)),

            // TypeRef
            new TableLayout(
                new ColumnLayout("ResolutionScope", ColumnType.ResolutionScope,
                    GetColumnSize(ColumnType.ResolutionScope)),
                new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                new ColumnLayout("Namespace", ColumnType.Guid, StringIndexSize)),

            // TypeDef
            new TableLayout(
                new ColumnLayout("Flags", ColumnType.UInt32),
                new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                new ColumnLayout("Namespace", ColumnType.String, StringIndexSize),
                new ColumnLayout("Extends", ColumnType.TypeDefOrRef,
                    GetColumnSize(ColumnType.TypeDefOrRef)),
                new ColumnLayout("FieldList", ColumnType.Field, GetColumnSize(ColumnType.Field)),
                new ColumnLayout("MethodList", ColumnType.Method, GetColumnSize(ColumnType.Method))),

            // FieldPtr
            new TableLayout(
                new ColumnLayout("Field", ColumnType.Field, GetColumnSize(ColumnType.Field))),

            // Field
            new TableLayout(
                new ColumnLayout("Flags", ColumnType.UInt16),
                new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                new ColumnLayout("Signature", ColumnType.Blob, BlobIndexSize)),

            // MethodPtr
            new TableLayout(
                new ColumnLayout("Method", ColumnType.Method, GetColumnSize(ColumnType.Method))),

            // Method
            new TableLayout(
                new ColumnLayout("RVA", ColumnType.UInt32),
                new ColumnLayout("ImplFlags", ColumnType.UInt16),
                new ColumnLayout("Flags", ColumnType.UInt16),
                new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                new ColumnLayout("Signature", ColumnType.Blob, BlobIndexSize),
                new ColumnLayout("ParamList", ColumnType.Param, GetColumnSize(ColumnType.Param))),

            // ParamPtr
            new TableLayout(
                new ColumnLayout("Parameter", ColumnType.Param, GetColumnSize(ColumnType.Param))),

            // Parameter
            new TableLayout(
                new ColumnLayout("Flags", ColumnType.UInt16),
                new ColumnLayout("Sequence", ColumnType.UInt16),
                new ColumnLayout("Name", ColumnType.String, StringIndexSize)),

            // InterfaceImpl
            new TableLayout(
                new ColumnLayout("Class", ColumnType.TypeDef, GetColumnSize(ColumnType.TypeDef)),
                new ColumnLayout("Interface", ColumnType.TypeDefOrRef, GetColumnSize(ColumnType.TypeDefOrRef))),

            // MemberRef
            new TableLayout(
                new ColumnLayout("Parent", ColumnType.MemberRefParent, GetColumnSize(ColumnType.MemberRefParent)),
                new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                new ColumnLayout("Signature", ColumnType.Blob, BlobIndexSize)),

            // Constant
            new TableLayout(
                new ColumnLayout("Type", ColumnType.Byte),
                new ColumnLayout("Padding", ColumnType.Byte),
                new ColumnLayout("Parent", ColumnType.HasConstant, GetColumnSize(ColumnType.HasConstant)),
                new ColumnLayout("Value", ColumnType.Blob, BlobIndexSize)),

            // CustomAttribute
            new TableLayout(
                new ColumnLayout("Parent", ColumnType.HasCustomAttribute,
                    GetColumnSize(ColumnType.HasCustomAttribute)),
                new ColumnLayout("Type", ColumnType.CustomAttributeType,
                    GetColumnSize(ColumnType.CustomAttributeType)),
                new ColumnLayout("Value", ColumnType.Blob, BlobIndexSize)),

            // FieldMarshal
            new TableLayout(
                new ColumnLayout("Parent", ColumnType.HasFieldMarshal, GetColumnSize(ColumnType.HasFieldMarshal)),
                new ColumnLayout("NativeType", ColumnType.Blob, BlobIndexSize)),

            // DeclSecurity
            new TableLayout(
                new ColumnLayout("Action", ColumnType.UInt16),
                new ColumnLayout("Parent", ColumnType.HasDeclSecurity, GetColumnSize(ColumnType.HasDeclSecurity)),
                new ColumnLayout("PermissionSet", ColumnType.Blob, BlobIndexSize)),

            // ClassLayout
            new TableLayout(
                new ColumnLayout("PackingSize", ColumnType.UInt16),
                new ColumnLayout("ClassSize", ColumnType.UInt32),
                new ColumnLayout("Parent", ColumnType.TypeDef, GetColumnSize(ColumnType.TypeDef))),

            // FieldLayout
            new TableLayout(
                new ColumnLayout("Offset", ColumnType.UInt32),
                new ColumnLayout("Field", ColumnType.TypeDef, GetColumnSize(ColumnType.Field))),

            // StandAloneSig
            new TableLayout(
                new ColumnLayout("Signature", ColumnType.Blob, BlobIndexSize)),

            // EventMap
            new TableLayout(
                new ColumnLayout("Parent", ColumnType.TypeDef, GetColumnSize(ColumnType.TypeDef)),
                new ColumnLayout("EventList", ColumnType.Event, GetColumnSize(ColumnType.Event))),

            // EventPtr
            new TableLayout(
                new ColumnLayout("Event", ColumnType.Event, GetColumnSize(ColumnType.Event))),

            // Event
            new TableLayout(
                new ColumnLayout("Flags", ColumnType.UInt16),
                new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                new ColumnLayout("EventType", ColumnType.TypeDefOrRef, GetColumnSize(ColumnType.TypeDefOrRef))),

            // PropertyMap
            new TableLayout(
                new ColumnLayout("Parent", ColumnType.TypeDef, GetColumnSize(ColumnType.TypeDef)),
                new ColumnLayout("PropertyList", ColumnType.Event, GetColumnSize(ColumnType.Property))),

            // PropertyPtr
            new TableLayout(
                new ColumnLayout("Property", ColumnType.Property, GetColumnSize(ColumnType.Property))),

            // Property
            new TableLayout(
                new ColumnLayout("Flags", ColumnType.UInt16),
                new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                new ColumnLayout("PropertyType", ColumnType.Blob, BlobIndexSize)),

            // MethodSemantics
            new TableLayout(
                new ColumnLayout("Semantic", ColumnType.UInt16),
                new ColumnLayout("Method", ColumnType.Method, GetColumnSize(ColumnType.Method)),
                new ColumnLayout("Association", ColumnType.HasSemantics, GetColumnSize(ColumnType.HasSemantics))),

            // MethodImpl
            new TableLayout(
                new ColumnLayout("Class", ColumnType.TypeDef, GetColumnSize(ColumnType.TypeDef)),
                new ColumnLayout("MethodBody", ColumnType.MethodDefOrRef, GetColumnSize(ColumnType.MethodDefOrRef)),
                new ColumnLayout("MethodDeclaration", ColumnType.MethodDefOrRef,
                    GetColumnSize(ColumnType.MethodDefOrRef))),

            // ModuleRef
            new TableLayout(
                new ColumnLayout("Name", ColumnType.String, StringIndexSize)),

            // TypeSpec
            new TableLayout(
                new ColumnLayout("Signature", ColumnType.Blob, BlobIndexSize)),

            // ImplMap
            new TableLayout(
                new ColumnLayout("MappingFlags", ColumnType.UInt16),
                new ColumnLayout("MemberForwarded", ColumnType.MemberForwarded,
                    GetColumnSize(ColumnType.MemberForwarded)),
                new ColumnLayout("ImportName", ColumnType.String, StringIndexSize),
                new ColumnLayout("ImportScope", ColumnType.ModuleRef, GetColumnSize(ColumnType.ModuleRef))),

            // FieldRva
            new TableLayout(
                new ColumnLayout("RVA", ColumnType.UInt32),
                new ColumnLayout("Field", ColumnType.Field, GetColumnSize(ColumnType.Field))),

            // EncLog
            new TableLayout(
                new ColumnLayout("Token", ColumnType.UInt32),
                new ColumnLayout("FuncCode", ColumnType.UInt32)),

            // EncMap
            new TableLayout(
                new ColumnLayout("Token", ColumnType.UInt32)),

            // Assembly
            new TableLayout(
                new ColumnLayout("HashAlgId", ColumnType.UInt32),
                new ColumnLayout("MajorVersion", ColumnType.UInt16),
                new ColumnLayout("MinorVersion", ColumnType.UInt16),
                new ColumnLayout("BuildNumber", ColumnType.UInt16),
                new ColumnLayout("RevisionNumber", ColumnType.UInt16),
                new ColumnLayout("Flags", ColumnType.UInt32),
                new ColumnLayout("PublicKey", ColumnType.Blob, BlobIndexSize),
                new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                new ColumnLayout("Culture", ColumnType.String, StringIndexSize)),

            // AssemblyProcessor
            new TableLayout(
                new ColumnLayout("Processor", ColumnType.UInt32)),

            // AssemblyOS
            new TableLayout(
                new ColumnLayout("PlatformId", ColumnType.UInt32),
                new ColumnLayout("MajorVersion", ColumnType.UInt32),
                new ColumnLayout("MinorVersion", ColumnType.UInt32)),

            // AssemblyRef
            new TableLayout(
                new ColumnLayout("MajorVersion", ColumnType.UInt16),
                new ColumnLayout("MinorVersion", ColumnType.UInt16),
                new ColumnLayout("BuildNumber", ColumnType.UInt16),
                new ColumnLayout("RevisionNumber", ColumnType.UInt16),
                new ColumnLayout("Flags", ColumnType.UInt32),
                new ColumnLayout("PublicKeyOrToken", ColumnType.Blob, BlobIndexSize),
                new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                new ColumnLayout("Culture", ColumnType.String, StringIndexSize),
                new ColumnLayout("HashValue", ColumnType.Blob, BlobIndexSize)),

            // AssemblyRefProcessor
            new TableLayout(
                new ColumnLayout("Processor", ColumnType.UInt32),
                new ColumnLayout("AssemblyRef", ColumnType.AssemblyRef, GetColumnSize(ColumnType.AssemblyRef))),

            // AssemblyRefOS
            new TableLayout(
                new ColumnLayout("PlatformId", ColumnType.UInt32),
                new ColumnLayout("MajorVersion", ColumnType.UInt32),
                new ColumnLayout("MinorVersion", ColumnType.UInt32),
                new ColumnLayout("AssemblyRef", ColumnType.AssemblyRef, GetColumnSize(ColumnType.AssemblyRef))),

            // File
            new TableLayout(
                new ColumnLayout("Flags", ColumnType.UInt32),
                new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                new ColumnLayout("HashValue", ColumnType.Blob, BlobIndexSize)),

            // ExportedType
            new TableLayout(
                new ColumnLayout("Flags", ColumnType.UInt32),
                new ColumnLayout("TypeDefId", ColumnType.UInt32),
                new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                new ColumnLayout("Namespace", ColumnType.String, StringIndexSize),
                new ColumnLayout("Implementation", ColumnType.Implementation,
                    GetColumnSize(ColumnType.Implementation))),

            // ManifestResource
            new TableLayout(
                new ColumnLayout("Offset", ColumnType.UInt32),
                new ColumnLayout("Flags", ColumnType.UInt32),
                new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                new ColumnLayout("Implementation", ColumnType.Implementation,
                    GetColumnSize(ColumnType.Implementation))),

            // NestedClass
            new TableLayout(
                new ColumnLayout("NestedClass", ColumnType.TypeDef, GetColumnSize(ColumnType.TypeDef)),
                new ColumnLayout("EnclosingClass", ColumnType.TypeDef, GetColumnSize(ColumnType.TypeDef))),

            // GenericParam
            new TableLayout(
                new ColumnLayout("Number", ColumnType.UInt16),
                new ColumnLayout("Flags", ColumnType.UInt16),
                new ColumnLayout("Owner", ColumnType.TypeOrMethodDef, GetColumnSize(ColumnType.TypeOrMethodDef)),
                new ColumnLayout("EnclosingClass", ColumnType.String, StringIndexSize)),

            // MethodSpec
            new TableLayout(
                new ColumnLayout("Method", ColumnType.Method, GetColumnSize(ColumnType.MethodDefOrRef)),
                new ColumnLayout("Instantiation", ColumnType.Blob, BlobIndexSize)),

            // GenericParamConstraint
            new TableLayout(
                new ColumnLayout("Owner", ColumnType.GenericParam, GetColumnSize(ColumnType.GenericParam)),
                new ColumnLayout("Constraint", ColumnType.TypeDefOrRef, GetColumnSize(ColumnType.TypeDefOrRef))),

            // Unused
            default,
            default,
            default,

            // Document
            new TableLayout(
                new ColumnLayout("Name", ColumnType.Blob, BlobIndexSize),
                new ColumnLayout("HashAlgorithm", ColumnType.Guid, GuidIndexSize),
                new ColumnLayout("Hash", ColumnType.Blob, BlobIndexSize),
                new ColumnLayout("Language", ColumnType.Guid, GuidIndexSize)),

            // MethodDebugInformation
            new TableLayout(
                new ColumnLayout("Document", ColumnType.Document, GetColumnSize(ColumnType.Document)),
                new ColumnLayout("SequencePoints", ColumnType.Blob, BlobIndexSize)),

            // LocalScope
            new TableLayout(
                new ColumnLayout("Method", ColumnType.Method, GetColumnSize(ColumnType.Method)),
                new ColumnLayout("ImportScope", ColumnType.ImportScope, GetColumnSize(ColumnType.ImportScope)),
                new ColumnLayout("VariableList", ColumnType.LocalVariable, GetColumnSize(ColumnType.LocalVariable)),
                new ColumnLayout("ConstantList", ColumnType.LocalConstant, GetColumnSize(ColumnType.LocalConstant)),
                new ColumnLayout("StartOffset", ColumnType.UInt32),
                new ColumnLayout("Length", ColumnType.UInt32)),

            // LocalVariable
            new TableLayout(
                new ColumnLayout("Attributes", ColumnType.UInt16),
                new ColumnLayout("Index", ColumnType.UInt16),
                new ColumnLayout("VariableList", ColumnType.String, StringIndexSize)),

            // LocalConstant
            new TableLayout(
                new ColumnLayout("Name", ColumnType.String, StringIndexSize),
                new ColumnLayout("Signature", ColumnType.Blob, BlobIndexSize)),

            // ImportScope
            new TableLayout(
                new ColumnLayout("Parent", ColumnType.ImportScope, GetColumnSize(ColumnType.ImportScope)),
                new ColumnLayout("Imports", ColumnType.Blob, BlobIndexSize)),

            // StateMachineMethod
            new TableLayout(
                new ColumnLayout("MoveNextMethod", ColumnType.Method, GetColumnSize(ColumnType.Method)),
                new ColumnLayout("KickoffMethod", ColumnType.Method, GetColumnSize(ColumnType.Method))),

            // CustomDebugInformation
            new TableLayout(
                new ColumnLayout("Parent", ColumnType.HasCustomDebugInformation,
                    GetColumnSize(ColumnType.HasCustomDebugInformation)),
                new ColumnLayout("Kind", ColumnType.Guid, GuidIndexSize),
                new ColumnLayout("Value", ColumnType.Blob, BlobIndexSize)),
        };
    }
}
