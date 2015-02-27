using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using AsmResolver.Net.Builder;
using AsmResolver.Net.Msil;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Metadata
{
    public class MethodDefinitionTable : MetadataTable<MethodDefinition>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.Method; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof (uint) +                                              // Rva
                   sizeof (ushort) +                                            // ImplAttrbibutes
                   sizeof (ushort) +                                            // Attributes
                   (uint)TableStream.StringIndexSize +                          // Name
                   (uint)TableStream.BlobIndexSize +                            // Signature
                   (uint)TableStream.GetTable<ParameterDefinition>().IndexSize; // ParamList
        }

        protected override MethodDefinition ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new MethodDefinition(Header, token, new MetadataRow<uint, ushort, ushort, uint, uint, uint>
            {
                Column1 = reader.ReadUInt32(),                                                      // Rva
                Column2 = reader.ReadUInt16(),                                                      // ImplAttrbibutes
                Column3 = reader.ReadUInt16(),                                                      // Attributes
                Column4 = reader.ReadIndex(TableStream.StringIndexSize),                            // Name
                Column5 = reader.ReadIndex(TableStream.BlobIndexSize),                              // Signature
                Column6 = reader.ReadIndex(TableStream.GetTable<ParameterDefinition>().IndexSize)   // ParamList
            });
        }

        protected override void UpdateMember(NetBuildingContext context, MethodDefinition member)
        {
            var row = member.MetadataRow;
            row.Column1 = member.Rva;
            row.Column2 = (ushort)member.ImplAttributes;
            row.Column3 = (ushort)member.Attributes;
            row.Column4 = context.GetStreamBuffer<StringStreamBuffer>().GetStringOffset(member.Name);
            row.Column5 = context.GetStreamBuffer<BlobStreamBuffer>().GetBlobOffset(member.Signature);
        }

        protected override void WriteMember(WritingContext context, MethodDefinition member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;

            writer.WriteUInt32(row.Column1);
            writer.WriteUInt16(row.Column2);
            writer.WriteUInt16(row.Column3);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column4);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column5);
            writer.WriteIndex(TableStream.GetTable<ParameterDefinition>().IndexSize, row.Column6);
        }
    }

    public class MethodDefinition : MetadataMember<MetadataRow<uint, ushort, ushort, uint, uint, uint>>, IHasSecurityAttribute, IMemberForwarded, IGenericParameterProvider, IMemberRefParent, ICustomAttributeType, ICollectionItem, IGenericContext
    {
        private string _name;
        private string _fullName;
        private MethodSignature _signature;
        private CustomAttributeCollection _customAttributes;
        private SecurityDeclarationCollection _securityDeclarations;
        private RangedDefinitionCollection<ParameterDefinition> _parameters;
        private MethodBody _body;
        private TypeDefinition _declaringType;
        private GenericParameterCollection _genericParameters;

        public MethodDefinition(string name, MethodAttributes attributes, MethodSignature signature)
            : base(null, new MetadataToken(MetadataTokenType.Method), new MetadataRow<uint, ushort, ushort, uint, uint, uint>())
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (signature == null)
                throw new ArgumentNullException("signature");

            Name = name;
            Attributes = attributes;
            Signature = signature;
        }

        internal MethodDefinition(MetadataHeader header, MetadataToken token, MetadataRow<uint, ushort, ushort, uint, uint, uint> row)
            : base(header, token, row)
        {
            var stringStream = header.GetStream<StringStream>();
            var blobStream = header.GetStream<BlobStream>();

            Rva = row.Column1;
            ImplAttributes = (MethodImplAttributes)row.Column2;
            Attributes = (MethodAttributes)row.Column3;
            Name = stringStream.GetStringByOffset(row.Column4);
            Signature = MethodSignature.FromReader(header, blobStream.CreateBlobReader(row.Column5));
        }

        public uint Rva
        {
            get;
            set;
        }

        public MethodImplAttributes ImplAttributes
        {
            get;
            set;
        }

        public MethodAttributes Attributes
        {
            get;
            set;
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                _fullName = null;
            }
        }

        public string FullName
        {
            get { return _fullName ?? (_fullName = this.GetFullName(Signature)); }
        }

        public TypeDefinition DeclaringType
        {
            get
            {
                if (_declaringType != null || Header == null)
                    return _declaringType;
                return _declaringType =
                    Header.GetStream<TableStream>()
                        .GetTable<TypeDefinition>()
                        .FirstOrDefault(x => x.Methods.Contains(this));
            }
        }

        ITypeDefOrRef IMemberReference.DeclaringType
        {
            get { return DeclaringType; }
        }

        public MethodSignature Signature
        {
            get { return _signature; }
            set
            {
                _signature = value;
                _fullName = null;
            }
        }

        MemberSignature IMethodDefOrRef.Signature
        {
            get { return Signature; }
        }

        public RangedDefinitionCollection<ParameterDefinition> Parameters
        {
            get
            {
                if (_parameters != null)
                    return _parameters;
                return
                    _parameters =
                        RangedDefinitionCollection<ParameterDefinition>.Create(Header, this,
                            x => (int)x.MetadataRow.Column6);
            }
        }

        public MethodBody MethodBody
        {
            get
            {
                if (_body != null)
                    return _body;

                if (Rva == 0)
                    return null;

                var application = Header.NetDirectory.Assembly;
                var offset = application.RvaToFileOffset(Rva);
                var context = application.ReadingContext.CreateSubContext(offset);
                return _body = MethodBody.FromReader(this, context);
            }
            set { _body = value; }
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

        object ICollectionItem.Owner
        {
            get { return DeclaringType; }
            set { _declaringType = value as TypeDefinition; }
        }

        public bool IsPrivate
        {
            get { return GetMethodAccessAttribute(MethodAttributes.Private); }
            set { SetMethodAccessAttribute(MethodAttributes.Private, value); }
        }

        public bool IsFamilyAndAssembly
        {
            get { return GetMethodAccessAttribute(MethodAttributes.FamilyAndAssembly); }
            set { SetMethodAccessAttribute(MethodAttributes.FamilyAndAssembly, value); }
        }

        public bool IsFamilyOrAssembly
        {
            get { return GetMethodAccessAttribute(MethodAttributes.FamilyOrAssembly); }
            set { SetMethodAccessAttribute(MethodAttributes.FamilyOrAssembly, value); }
        }

        public bool IsAssembly
        {
            get { return GetMethodAccessAttribute(MethodAttributes.Assembly); }
            set { SetMethodAccessAttribute(MethodAttributes.Assembly, value); }
        }

        public bool IsFamily
        {
            get { return GetMethodAccessAttribute(MethodAttributes.Family); }
            set { SetMethodAccessAttribute(MethodAttributes.Family, value); }
        }

        public bool IsPublic
        {
            get { return GetMethodAccessAttribute(MethodAttributes.Public); }
            set { SetMethodAccessAttribute(MethodAttributes.Public, value); }
        }

        public bool IsStatic
        {
            get { return ((uint)Attributes).GetAttribute((uint)MethodAttributes.Static); }
            set { Attributes = (MethodAttributes)((uint)Attributes).SetAttribute((uint)MethodAttributes.Static, value); }
        }

        public bool IsFinal
        {
            get { return ((uint)Attributes).GetAttribute((uint)MethodAttributes.Final); }
            set { Attributes = (MethodAttributes)((uint)Attributes).SetAttribute((uint)MethodAttributes.Final, value); }
        }

        public bool IsVirtual
        {
            get { return ((uint)Attributes).GetAttribute((uint)MethodAttributes.Virtual); }
            set { Attributes = (MethodAttributes)((uint)Attributes).SetAttribute((uint)MethodAttributes.Virtual, value); }
        }

        public bool IsHideBySig
        {
            get { return ((uint)Attributes).GetAttribute((uint)MethodAttributes.HideBySig); }
            set { Attributes = (MethodAttributes)((uint)Attributes).SetAttribute((uint)MethodAttributes.HideBySig, value); }
        }

        public bool IsAbstract
        {
            get { return ((uint)Attributes).GetAttribute((uint)MethodAttributes.Abstract); }
            set { Attributes = (MethodAttributes)((uint)Attributes).SetAttribute((uint)MethodAttributes.Abstract, value); }
        }

        public bool IsSpecialName
        {
            get { return ((uint)Attributes).GetAttribute((uint)MethodAttributes.SpecialName); }
            set { Attributes = (MethodAttributes)((uint)Attributes).SetAttribute((uint)MethodAttributes.SpecialName, value); }
        }

        public bool IsRuntimeSpecialName
        {
            get { return ((uint)Attributes).GetAttribute((uint)MethodAttributes.RuntimeSpecialName); }
            set { Attributes = (MethodAttributes)((uint)Attributes).SetAttribute((uint)MethodAttributes.RuntimeSpecialName, value); }
        }

        public bool HasSecurity
        {
            get { return ((uint)Attributes).GetAttribute((uint)MethodAttributes.HasSecurity); }
            set { Attributes = (MethodAttributes)((uint)Attributes).SetAttribute((uint)MethodAttributes.HasSecurity, value); }
        }

        public bool IsConstructor
        {
            get { return IsRuntimeSpecialName && IsSpecialName && (Name == ".ctor" || Name == ".cctor"); }
        }

        private bool GetMethodAccessAttribute(MethodAttributes attribute)
        {
            return ((uint)Attributes).GetMaskedAttribute((uint)MethodAttributes.MemberAccessMask,
                (uint)attribute);
        }

        private void SetMethodAccessAttribute(MethodAttributes attribute, bool value)
        {
            Attributes = (MethodAttributes)((uint)Attributes).SetMaskedAttribute((uint)MethodAttributes.MemberAccessMask,
                (uint)attribute, value);
        }

        public override string ToString()
        {
            return FullName;
        }

        IGenericParameterProvider IGenericContext.Type
        {
            get { return DeclaringType; }
        }

        IGenericParameterProvider IGenericContext.Method
        {
            get { return this; }
        }
    }
}