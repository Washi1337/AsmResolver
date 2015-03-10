using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class TypeDefinitionTable : MetadataTable<TypeDefinition>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.TypeDef; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof (uint) +                                                        // Attributes
                   (uint)TableStream.StringIndexSize +                                    // Name
                   (uint)TableStream.StringIndexSize +                                    // Namespace
                   (uint)TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).IndexSize + // BaseType
                   (uint)TableStream.GetTable<FieldDefinition>().IndexSize +              // FieldList
                   (uint)TableStream.GetTable<MethodDefinition>().IndexSize;              // MethodList 
        }

        protected override TypeDefinition ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new TypeDefinition(Header, token, new MetadataRow<uint, uint, uint, uint, uint, uint>
            {
                Column1 = reader.ReadUInt32(),                                                               // Attributes
                Column2 = reader.ReadIndex(TableStream.StringIndexSize),                                     // Name
                Column3 = reader.ReadIndex(TableStream.StringIndexSize),                                     // Namespace
                Column4 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).IndexSize),  // BaseType
                Column5 = reader.ReadIndex(TableStream.GetTable<FieldDefinition>().IndexSize),               // FieldList
                Column6 = reader.ReadIndex(TableStream.GetTable<MethodDefinition>().IndexSize),              // MethodList 
            });
        }

        protected override void UpdateMember(NetBuildingContext context, TypeDefinition member)
        {
            var stringStream = context.GetStreamBuffer<StringStreamBuffer>();

            var row = member.MetadataRow;
            row.Column1 = (uint)member.Attributes;
            row.Column2 = stringStream.GetStringOffset(member.Name);
            row.Column3 = stringStream.GetStringOffset(member.Namespace);
            row.Column4 = member.BaseType == null ? 0 : TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef)
                .EncodeToken(member.BaseType.MetadataToken);
            // TODO: update field and method list.
        }

        protected override void WriteMember(WritingContext context, TypeDefinition member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;

            writer.WriteUInt32(row.Column1);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column2);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column3);
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).IndexSize, row.Column4);
            writer.WriteIndex(TableStream.GetTable<FieldDefinition>().IndexSize, row.Column5);
            writer.WriteIndex(TableStream.GetTable<MethodDefinition>().IndexSize, row.Column6);
        }
    }

    public class TypeDefinition : MetadataMember<MetadataRow<uint, uint, uint, uint, uint, uint>>, ITypeDefOrRef, IHasSecurityAttribute, IGenericParameterProvider, IGenericContext
    {
        private CustomAttributeCollection _customAttributes;
        private SecurityDeclarationCollection _securityDeclarations;
        private RangedDefinitionCollection<FieldDefinition> _fields;
        private RangedDefinitionCollection<MethodDefinition> _methods;
        private PropertyMap _propertyMap;
        private EventMap _eventMap;
        private NestedClassCollection _nestedClasses;
        private GenericParameterCollection _genericParameters;
        private ClassLayout _classLayout;

        public TypeDefinition(string @namespace, string name)
            : this(@namespace, name, null)
        {
        }

        public TypeDefinition(string @namespace, string name, ITypeDefOrRef baseType)
            : base(null, new MetadataToken(MetadataTokenType.TypeDef), new MetadataRow<uint, uint, uint, uint, uint, uint>())
        {
            Namespace = @namespace;
            Name = name;
            BaseType = baseType;
        }

        internal TypeDefinition(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint, uint, uint, uint, uint> row)
            : base(header, token, row)
        {
            var stringStream = header.GetStream<StringStream>();
            var tableStream = header.GetStream<TableStream>();

            Attributes = (TypeAttributes)row.Column1;
            Name = stringStream.GetStringByOffset(row.Column2);
            Namespace = stringStream.GetStringByOffset(row.Column3);

            var baseTypeToken = tableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).DecodeIndex(row.Column4);
            if (baseTypeToken.Rid != 0)
            {
                MetadataMember baseType;
                if (tableStream.TryResolveMember(baseTypeToken, out baseType))
                    BaseType = baseType as ITypeDefOrRef;
            }
        }

        public TypeAttributes Attributes
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Namespace
        {
            get;
            set;
        }

        public IResolutionScope ResolutionScope
        {
            get
            {
                if (Header == null)
                    return null;
                var tableStream = Header.GetStream<TableStream>();
                if (tableStream == null)
                    return null;
                var moduleTable = tableStream.GetTable<ModuleDefinition>();
                if (moduleTable == null || moduleTable.Count == 0)
                    return null;
                return moduleTable[0];
            }
        }

        public ITypeDefOrRef BaseType
        {
            get;
            set;
        }

        public RangedDefinitionCollection<FieldDefinition> Fields
        {
            get
            {
                return _fields ?? (_fields =
                    RangedDefinitionCollection<FieldDefinition>.Create(Header, this,
                        x => (int)x.MetadataRow.Column5));
            }
        }

        public RangedDefinitionCollection<MethodDefinition> Methods
        {
            get
            {
                return _methods ?? (_methods =
                    RangedDefinitionCollection<MethodDefinition>.Create(Header, this,
                        x => (int)x.MetadataRow.Column6));
            }
        }

        public PropertyMap PropertyMap
        {
            get
            {
                if (_propertyMap != null)
                    return _propertyMap;
                return Header == null
                    ? (_propertyMap = new PropertyMap(this))
                    : (_propertyMap =
                        Header.GetStream<TableStream>().GetTable<PropertyMap>().FirstOrDefault(x => x.Parent == this));
            }
        }

        public EventMap EventMap
        {
            get
            {
                if (_eventMap != null)
                    return _eventMap;
                return Header == null
                    ? (_eventMap = new EventMap(this))
                    : (_eventMap =
                        Header.GetStream<TableStream>().GetTable<EventMap>().FirstOrDefault(x => x.Parent == this));
            }
        }

        public virtual string FullName
        {
            get
            {
                if (DeclaringType != null)
                    return DeclaringType.FullName + '+' + Name;
                return string.IsNullOrEmpty(Namespace) ? Name : Namespace + "." + Name;
            }
        }

        public TypeDefinition DeclaringType
        {
            get
            {
                if (Header == null)
                    return null;
                var tableStream = Header.GetStream<TableStream>();
                if (tableStream == null)
                    return null;
                var nestedClassTable = tableStream.GetTable<NestedClass>();
                if (nestedClassTable == null)
                    return null;
                var nestedClass = nestedClassTable.FirstOrDefault(x => x.Class == this);
                if (nestedClass == null)
                    return null;
                return nestedClass.EnclosingClass;
            }
        }

        ITypeDescriptor ITypeDescriptor.DeclaringTypeDescriptor
        {
            get { return DeclaringType; }
        }

        ITypeDefOrRef IMemberReference.DeclaringType
        {
            get { return DeclaringType; }
        }

        public NestedClassCollection NestedClasses
        {
            get { return _nestedClasses ?? (_nestedClasses = new NestedClassCollection(this)); }
        }
        
        public CustomAttributeCollection CustomAttributes
        {
            get { return _customAttributes ?? (_customAttributes = new CustomAttributeCollection(this)); }
        }

        public SecurityDeclarationCollection SecurityDeclarations
        {
            get { return _securityDeclarations ?? (_securityDeclarations = new SecurityDeclarationCollection(this)); }
        }

        public GenericParameterCollection GenericParameters
        {
            get { return _genericParameters ?? (_genericParameters = new GenericParameterCollection(this)); }
        }

        public ClassLayout ClassLayout
        {
            get
            {
                if (_classLayout != null || Header == null)
                    return _classLayout;
                var table = Header.GetStream<TableStream>().GetTable<ClassLayout>();
                return _classLayout = table.FirstOrDefault(x => x.Parent == this);
            }
            set { _classLayout = value; }
        }

        public bool IsNotPublic
        {
            get { return GetTypeAccessAttribute(TypeAttributes.NotPublic); }
            set { SetTypeAccessAttribute(TypeAttributes.NotPublic, value); }
        }

        public bool IsPublic
        {
            get { return GetTypeAccessAttribute(TypeAttributes.Public); }
            set { SetTypeAccessAttribute(TypeAttributes.Public, value); }
        }

        public bool IsNestedPublic
        {
            get { return GetTypeAccessAttribute(TypeAttributes.NestedPublic); }
            set { SetTypeAccessAttribute(TypeAttributes.NestedPublic, value); }
        }

        public bool IsNestedPrivate
        {
            get { return GetTypeAccessAttribute(TypeAttributes.NestedPrivate); }
            set { SetTypeAccessAttribute(TypeAttributes.NestedPrivate, value); }
        }

        public bool IsNestedFamily
        {
            get { return GetTypeAccessAttribute(TypeAttributes.NestedFamily); }
            set { SetTypeAccessAttribute(TypeAttributes.NestedFamily, value); }
        }

        public bool IsNestedAssembly
        {
            get { return GetTypeAccessAttribute(TypeAttributes.NestedAssembly); }
            set { SetTypeAccessAttribute(TypeAttributes.NestedAssembly, value); }
        }

        public bool IsNestedFamilyAndAssembly
        {
            get { return GetTypeAccessAttribute(TypeAttributes.NestedFamANDAssem); }
            set { SetTypeAccessAttribute(TypeAttributes.NestedFamANDAssem, value); }
        }

        public bool IsNestedFamilyOrAssembly
        {
            get { return GetTypeAccessAttribute(TypeAttributes.NestedFamORAssem); }
            set { SetTypeAccessAttribute(TypeAttributes.NestedFamORAssem, value); }
        }

        public bool IsClass
        {
            get { return ((uint)Attributes).GetMaskedAttribute((uint)TypeAttributes.ClassSemanticsMask, (uint)TypeAttributes.Class); }
            set
            {
                Attributes = (TypeAttributes)((uint)Attributes).SetMaskedAttribute((uint)TypeAttributes.ClassSemanticsMask,
                    (uint)TypeAttributes.Class, value);
            }
        }

        public bool IsInterface
        {
            get { return ((uint)Attributes).GetMaskedAttribute((uint)TypeAttributes.ClassSemanticsMask, (uint)TypeAttributes.Interface); }
            set
            {
                Attributes = (TypeAttributes)((uint)Attributes).SetMaskedAttribute((uint)TypeAttributes.ClassSemanticsMask,
                    (uint)TypeAttributes.Interface, value);
            }
        }

        public bool IsAbstract
        {
            get { return ((uint)Attributes).GetAttribute((uint)TypeAttributes.Abstract); }
            set { Attributes = (TypeAttributes)((uint)Attributes).SetAttribute((uint)TypeAttributes.Abstract, value); }
        }

        public bool IsSealed
        {
            get { return ((uint)Attributes).GetAttribute((uint)TypeAttributes.Sealed); }
            set { Attributes = (TypeAttributes)((uint)Attributes).SetAttribute((uint)TypeAttributes.Sealed, value); }
        }

        public bool IsSpecialName
        {
            get { return ((uint)Attributes).GetAttribute((uint)TypeAttributes.SpecialName); }
            set { Attributes = (TypeAttributes)((uint)Attributes).SetAttribute((uint)TypeAttributes.SpecialName, value); }
        }

        public bool IsImport
        {
            get { return ((uint)Attributes).GetAttribute((uint)TypeAttributes.Import); }
            set { Attributes = (TypeAttributes)((uint)Attributes).SetAttribute((uint)TypeAttributes.Import, value); }
        }

        public bool IsSerializable
        {
            get { return ((uint)Attributes).GetAttribute((uint)TypeAttributes.Serializable); }
            set { Attributes = (TypeAttributes)((uint)Attributes).SetAttribute((uint)TypeAttributes.Serializable, value); }
        }

        public bool IsAnsiClass
        {
            get { return ((uint)Attributes).GetMaskedAttribute((uint)TypeAttributes.StringFormatMask, (uint)TypeAttributes.AnsiClass); }
            set
            {
                Attributes = (TypeAttributes)((uint)Attributes).SetMaskedAttribute((uint)TypeAttributes.StringFormatMask,
                    (uint)TypeAttributes.AnsiClass, value);
            }
        }

        public bool IsUnicodeClass
        {
            get { return ((uint)Attributes).GetMaskedAttribute((uint)TypeAttributes.StringFormatMask, (uint)TypeAttributes.UnicodeClass); }
            set
            {
                Attributes = (TypeAttributes)((uint)Attributes).SetMaskedAttribute((uint)TypeAttributes.StringFormatMask,
                    (uint)TypeAttributes.UnicodeClass, value);
            }
        }

        public bool IsAutoClass
        {
            get { return ((uint)Attributes).GetMaskedAttribute((uint)TypeAttributes.StringFormatMask, (uint)TypeAttributes.AutoClass); }
            set
            {
                Attributes = (TypeAttributes)((uint)Attributes).SetMaskedAttribute((uint)TypeAttributes.StringFormatMask,
                    (uint)TypeAttributes.AutoClass, value);
            }
        }

        public bool IsBeforeFieldInit
        {
            get { return ((uint)Attributes).GetAttribute((uint)TypeAttributes.BeforeFieldInit); }
            set { Attributes = (TypeAttributes)((uint)Attributes).SetAttribute((uint)TypeAttributes.BeforeFieldInit, value); }
        }

        public bool IsForwarder
        {
            get { return ((uint)Attributes).GetAttribute((uint)TypeAttributes.Forwarder); }
            set { Attributes = (TypeAttributes)((uint)Attributes).SetAttribute((uint)TypeAttributes.Forwarder, value); }
        }

        public bool IsRuntimeSpecialName
        {
            get { return ((uint)Attributes).GetAttribute((uint)TypeAttributes.RTSpecialName); }
            set { Attributes = (TypeAttributes)((uint)Attributes).SetAttribute((uint)TypeAttributes.RTSpecialName, value); }
        }

        public bool HasSecurity
        {
            get { return ((uint)Attributes).GetAttribute((uint)TypeAttributes.HasSecurity); }
            set { Attributes = (TypeAttributes)((uint)Attributes).SetAttribute((uint)TypeAttributes.HasSecurity, value); }
        }
        
        public bool IsEnum
        {
            get { return BaseType != null && BaseType.IsTypeOf("System", "Enum"); }
        }
        
        public bool IsValueType
        {
            get
            {
                return BaseType != null &&
                       (BaseType.IsTypeOf("System", "ValueType") || IsEnum);
            }
        }

        private bool GetTypeAccessAttribute(TypeAttributes attribute)
        {
            return ((uint)Attributes).GetMaskedAttribute((uint)TypeAttributes.VisibilityMask,
                (uint)attribute);
        }

        private void SetTypeAccessAttribute(TypeAttributes attribute, bool value)
        {
            Attributes = (TypeAttributes)((uint)Attributes).SetMaskedAttribute((uint)TypeAttributes.VisibilityMask,
                (uint)attribute, value);
        }

        public ITypeDescriptor GetElementType()
        {
            return this;
        }

        public override string ToString()
        {
            return FullName;
        }

        IGenericParameterProvider IGenericContext.Type
        {
            get { return this; }
        }

        IGenericParameterProvider IGenericContext.Method
        {
            get { return null; }
        }
    }
}
