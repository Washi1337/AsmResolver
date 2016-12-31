using System;
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
        private readonly LazyValue<string> _name;
        private readonly LazyValue<string> _namespace;
        private readonly LazyValue<ITypeDefOrRef> _baseType;
        private CustomAttributeCollection _customAttributes;
        private SecurityDeclarationCollection _securityDeclarations;
        private RangedDefinitionCollection<FieldDefinition> _fields;
        private RangedDefinitionCollection<MethodDefinition> _methods;
        private PropertyMap _propertyMap;
        private EventMap _eventMap;
        private NestedClassCollection _nestedClasses;
        private GenericParameterCollection _genericParameters;
        private InterfaceImplementationCollection _interfaces;
        private MethodImplementationCollection _methodImplementations;
        private ClassLayout _classLayout;
        private string _fullName;

        public TypeDefinition(string @namespace, string name)
            : this(@namespace, name, null)
        {
        }

        public TypeDefinition(string @namespace, string name, ITypeDefOrRef baseType)
            : base(null, new MetadataToken(MetadataTokenType.TypeDef), new MetadataRow<uint, uint, uint, uint, uint, uint>())
        {
            _namespace = new LazyValue<string>(@namespace);
            _name = new LazyValue<string>(name);
            _baseType = new LazyValue<ITypeDefOrRef>(baseType);
            MetadataRow.Column5 = MetadataRow.Column6 = 1;
        }

        internal TypeDefinition(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint, uint, uint, uint, uint> row)
            : base(header, token, row)
        {
            var stringStream = header.GetStream<StringStream>();
            var tableStream = header.GetStream<TableStream>();

            Attributes = (TypeAttributes)row.Column1;

            _name = _namespace = new LazyValue<string>(() => stringStream.GetStringByOffset(row.Column2));
            _namespace = new LazyValue<string>(() => stringStream.GetStringByOffset(row.Column3));
            _baseType = new LazyValue<ITypeDefOrRef>(() =>
            {
                var baseTypeToken = tableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).DecodeIndex(row.Column4);
                if (baseTypeToken.Rid != 0)
                {
                    MetadataMember baseType;
                    if (tableStream.TryResolveMember(baseTypeToken, out baseType))
                        return baseType as ITypeDefOrRef;
                }
                return null;
            });
        }

        public TypeAttributes Attributes
        {
            get;
            set;
        }

        public string Name
        {
            get { return _name.Value; }
            set
            {
                _name.Value = value;
                _fullName = null;
            }
        }

        public string Namespace
        {
            get { return _namespace.Value; }
            set
            {
                _namespace.Value = value;
                _fullName = null;
            }
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
            get { return _baseType.Value; }
            set { _baseType.Value = value; }
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
                if (_fullName != null)
                    return _fullName;
                if (DeclaringType != null)
                    return _fullName = DeclaringType.FullName + '+' + Name;
                return _fullName = string.IsNullOrEmpty(Namespace) ? Name : Namespace + "." + Name;
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

        public InterfaceImplementationCollection Interfaces
        {
            get { return _interfaces ?? (_interfaces = new InterfaceImplementationCollection(this)); }
        }

        public MethodImplementationCollection MethodImplementations
        {
            get { return _methodImplementations ?? (_methodImplementations = new MethodImplementationCollection(this)); }
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
            get { return GetTypeAccessAttribute(TypeAttributes.NestedFamilyAndAssembly); }
            set { SetTypeAccessAttribute(TypeAttributes.NestedFamilyAndAssembly, value); }
        }

        public bool IsNestedFamilyOrAssembly
        {
            get { return GetTypeAccessAttribute(TypeAttributes.NestedFamilyOrAssembly); }
            set { SetTypeAccessAttribute(TypeAttributes.NestedFamilyOrAssembly, value); }
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
            get { return Attributes.HasFlag(TypeAttributes.Abstract); }
            set { Attributes = Attributes.SetFlag(TypeAttributes.Abstract, value); }
        }

        public bool IsSealed
        {
            get { return Attributes.HasFlag(TypeAttributes.Sealed); }
            set { Attributes = Attributes.SetFlag(TypeAttributes.Sealed, value); }
        }

        public bool IsSpecialName
        {
            get { return Attributes.HasFlag(TypeAttributes.SpecialName); }
            set { Attributes = Attributes.SetFlag(TypeAttributes.SpecialName, value); }
        }

        public bool IsImport
        {
            get { return Attributes.HasFlag(TypeAttributes.Import); }
            set { Attributes = Attributes.SetFlag(TypeAttributes.Import, value); }
        }

        public bool IsSerializable
        {
            get { return Attributes.HasFlag(TypeAttributes.Serializable); }
            set { Attributes = Attributes.SetFlag(TypeAttributes.Serializable, value); }
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
            get { return Attributes.HasFlag(TypeAttributes.BeforeFieldInit); }
            set { Attributes = Attributes.SetFlag(TypeAttributes.BeforeFieldInit, value); }
        }

        public bool IsForwarder
        {
            get { return Attributes.HasFlag(TypeAttributes.Forwarder); }
            set { Attributes = Attributes.SetFlag(TypeAttributes.Forwarder, value); }
        }

        public bool IsRuntimeSpecialName
        {
            get { return Attributes.HasFlag(TypeAttributes.RuntimeSpecialName); }
            set { Attributes = Attributes.SetFlag(TypeAttributes.RuntimeSpecialName, value); }
        }

        public bool HasSecurity
        {
            get { return Attributes.HasFlag(TypeAttributes.HasSecurity); }
            set { Attributes = Attributes.SetFlag(TypeAttributes.HasSecurity, value); }
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

        IMetadataMember IResolvable.Resolve()
        {
            return this;
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
