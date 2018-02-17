﻿using System;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    public class FieldDefinition : MetadataMember<MetadataRow<FieldAttributes, uint, uint>>, ICallableMemberReference, IHasConstant, IHasFieldMarshal, IMemberForwarded
    {
        private readonly LazyValue<string> _name;
        private readonly LazyValue<FieldSignature> _signature;
        private readonly LazyValue<Constant> _constant;
        private readonly LazyValue<TypeDefinition> _declaringType;
        private readonly LazyValue<FieldRva> _fieldRva;
        private readonly LazyValue<FieldMarshal> _fieldMarshal;
        private string _fullName;

        public FieldDefinition(string name, FieldAttributes attributes, FieldSignature signature)
            : base(null, new MetadataToken(MetadataTokenType.Field))
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (signature == null)
                throw new ArgumentNullException("signature");

            _name = new LazyValue<string>(name);
            Attributes = attributes;
            _signature = new LazyValue<FieldSignature>(signature);
            
            _constant = new LazyValue<Constant>(default(Constant));
            _declaringType = new LazyValue<TypeDefinition>(default(TypeDefinition));
            _fieldRva = new LazyValue<FieldRva>(default(FieldRva));
            _fieldMarshal = new LazyValue<FieldMarshal>(default(FieldMarshal));
            
            CustomAttributes = new CustomAttributeCollection(this);
        }

        internal FieldDefinition(MetadataImage image, MetadataRow<FieldAttributes, uint, uint> row)
            : base(image, row.MetadataToken)
        {
            Attributes = row.Column1;
            
            _name = new LazyValue<string>(() => 
                image.Header.GetStream<StringStream>().GetStringByOffset(row.Column2));
            
            _signature = new LazyValue<FieldSignature>(() => 
                FieldSignature.FromReader(image, image.Header.GetStream<BlobStream>().CreateBlobReader(row.Column3)));

            _declaringType = new LazyValue<TypeDefinition>(() =>
            {
                var table = image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.TypeDef);
                var typeRow = table.GetRowClosestToKey(4, row.MetadataToken.Rid);
                return (TypeDefinition) table.GetMemberFromRow(image, typeRow);
            });
            
            _constant = new LazyValue<Constant>(() =>
            {
                var table = (ConstantTable) image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.Constant);
                var constantRow = table.FindConstantOfOwner(row.MetadataToken);
                return constantRow != null ? (Constant) table.GetMemberFromRow(image, constantRow) : null;
            });
            
            _fieldRva = new LazyValue<FieldRva>(() =>
            {
                var tableStream = image.Header.GetStream<TableStream>();
                var table = tableStream.GetTable(MetadataTokenType.FieldRva);
                var rvaRow = table.GetRowByKey(1, row.MetadataToken.Rid);
                return rvaRow != null ? (FieldRva) table.GetMemberFromRow(image, rvaRow) : null;
            });
            
            _fieldMarshal = new LazyValue<FieldMarshal>(() =>
            {
                var table = (FieldMarshalTable) image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.FieldMarshal);
                var marshalRow = table.FindFieldMarshalOfOwner(row.MetadataToken);
                return marshalRow != null ? (FieldMarshal) table.GetMemberFromRow(image, marshalRow) : null;
            });
            
            CustomAttributes = new CustomAttributeCollection(this);
        }

        public FieldAttributes Attributes
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

        public virtual string FullName
        {
            get { return _fullName ?? (_fullName = this.GetFullName(Signature)); }
        }

        public TypeDefinition DeclaringType
        {
            get { return _declaringType.Value;}
            internal set { _declaringType.Value = value; }
        }

        ITypeDefOrRef IMemberReference.DeclaringType
        {
            get { return DeclaringType; }
        }

        public FieldSignature Signature
        {
            get { return _signature.Value; }
            set
            {
                _signature.Value = value;
                _fullName = null;
            }
        }

        CallingConventionSignature ICallableMemberReference.Signature
        {
            get { return Signature; }
        }

        public Constant Constant
        {
            get { return _constant.Value; }
            set { _constant.Value = value; }
        }

        public FieldMarshal FieldMarshal
        {
            get { return _fieldMarshal.Value; }
            set { _fieldMarshal.Value = value; }
        }

        public bool HasFieldRva
        {
            get { return Attributes.HasFlag(FieldAttributes.HasFieldRva); }
            set { Attributes.SetFlag(FieldAttributes.HasFieldRva, value); }
        }

        public FieldRva FieldRva
        {
            get { return _fieldRva.Value; }
            set { _fieldRva.Value = value; }
        }
        
        public CustomAttributeCollection CustomAttributes
        {
            get;
            private set;
        }
        
        public bool IsPrivate
        {
            get { return GetFieldAccessAttribute(FieldAttributes.Private); }
            set { SetFieldAccessAttribute(FieldAttributes.Private, value); }
        }

        public bool IsFamilyAndAssembly
        {
            get { return GetFieldAccessAttribute(FieldAttributes.FamilyAndAssembly); }
            set { SetFieldAccessAttribute(FieldAttributes.FamilyAndAssembly, value); }
        }

        public bool IsFamilyOrAssembly
        {
            get { return GetFieldAccessAttribute(FieldAttributes.FamilyOrAssembly); }
            set { SetFieldAccessAttribute(FieldAttributes.FamilyOrAssembly, value); }
        }

        public bool IsAssembly
        {
            get { return GetFieldAccessAttribute(FieldAttributes.Assembly); }
            set { SetFieldAccessAttribute(FieldAttributes.Assembly, value); }
        }

        public bool IsFamily
        {
            get { return GetFieldAccessAttribute(FieldAttributes.Family); }
            set { SetFieldAccessAttribute(FieldAttributes.Family, value); }
        }

        public bool IsPublic
        {
            get { return GetFieldAccessAttribute(FieldAttributes.Public); }
            set { SetFieldAccessAttribute(FieldAttributes.Public, value); }
        }

        public bool IsStatic
        {
            get { return Attributes.HasFlag(FieldAttributes.Static); }
            set { Attributes = Attributes.SetFlag(FieldAttributes.Static, value); }
        }

        public bool IsLiteral
        {
            get { return Attributes.HasFlag(FieldAttributes.Literal); }
            set { Attributes = Attributes.SetFlag(FieldAttributes.Literal, value); }
        }

        public bool IsInitOnly
        {
            get { return Attributes.HasFlag(FieldAttributes.InitOnly); }
            set { Attributes = Attributes.SetFlag(FieldAttributes.InitOnly, value); }
        }

        private bool GetFieldAccessAttribute(FieldAttributes attribute)
        {
            return ((uint)Attributes).GetMaskedAttribute((uint)FieldAttributes.FieldAccessMask,
                (uint)attribute);
        }

        private void SetFieldAccessAttribute(FieldAttributes attribute, bool value)
        {
            Attributes = (FieldAttributes)((uint)Attributes).SetMaskedAttribute((uint)FieldAttributes.FieldAccessMask,
                (uint)attribute, value);
        }

        public override string ToString()
        {
            return FullName;
        }

        IMetadataMember IResolvable.Resolve()
        {
            return this;
        }
    }
}
