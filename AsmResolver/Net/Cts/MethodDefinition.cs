using System;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    public class MethodDefinition : MetadataMember<MetadataRow<uint, MethodImplAttributes, MethodAttributes, uint, uint, uint>>, ICallableMemberReference, IHasSecurityAttribute, IMemberForwarded, IGenericParameterProvider, IMemberRefParent, ICustomAttributeType
    {
        private readonly LazyValue<string> _name;
        private readonly LazyValue<MethodSignature> _signature;
        private string _fullName;
        private CustomAttributeCollection _customAttributes;
        private SecurityDeclarationCollection _securityDeclarations;
        //private RangedDefinitionCollection<ParameterDefinition> _parameters;
        //private MethodBody _body;
        private TypeDefinition _declaringType;
        //private GenericParameterCollection _genericParameters;
        //private PInvokeImplementation _pinvokeImplementation;

        public MethodDefinition(string name, MethodAttributes attributes, MethodSignature signature)
            : base(null, new MetadataToken(MetadataTokenType.Method))
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (signature == null)
                throw new ArgumentNullException("signature");

            _name = new LazyValue<string>(name);
            Attributes = attributes;
            _signature = new LazyValue<MethodSignature>(signature);
        }

        internal MethodDefinition(MetadataImage image, MetadataRow<uint, MethodImplAttributes, MethodAttributes, uint, uint, uint> row)
            : base(image, row.MetadataToken)
        {
            var stringStream = image.Header.GetStream<StringStream>();
            var blobStream = image.Header.GetStream<BlobStream>();

            Rva = row.Column1;
            ImplAttributes = row.Column2;
            Attributes = row.Column3;
            _name = new LazyValue<string>(() => stringStream.GetStringByOffset(row.Column4));
            
            IBinaryStreamReader blobReader;
            if (blobStream.TryCreateBlobReader(row.Column5, out blobReader))
                _signature = new LazyValue<MethodSignature>(() => MethodSignature.FromReader(image, blobReader));
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
            get { return _name.Value; }
            set
            {
                _name.Value = value;
                _fullName = null;
            }
        }

        public string FullName
        {
            get { return _fullName ?? (_fullName = this.GetFullName(Signature)); }
        }

        public TypeDefinition DeclaringType
        {
            get;
            internal set;
        }

        ITypeDefOrRef IMemberReference.DeclaringType
        {
            get { return DeclaringType; }
        }

        public MethodSignature Signature
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

        MemberSignature IMethodDefOrRef.Signature
        {
            get { return Signature; }
        }

        // TODO
        //public RangedDefinitionCollection<ParameterDefinition> Parameters
        //{
        //    get
        //    {
        //        if (_parameters != null)
        //            return _parameters;
        //        return
        //            _parameters =
        //                RangedDefinitionCollection<ParameterDefinition>.Create(Header, this,
        //                    x => (int)x.MetadataRow.Column6);
        //    }
        //}

        //public MethodBody MethodBody
        //{
        //    get
        //    {
        //        if (_body != null)
        //            return _body;

        //        if (Rva == 0 || IsNative)
        //            return null;

        //        var application = Header.NetDirectory.Assembly;
        //        var offset = application.RvaToFileOffset(Rva);
        //        var context = application.ReadingContext.CreateSubContext(offset);
        //        return _body = MethodBody.FromReadingContext(this, context);
        //    }
        //    set { _body = value; }
        //}

        public CustomAttributeCollection CustomAttributes
        {
            get { return _customAttributes ?? (_customAttributes = new CustomAttributeCollection(this)); }
        }

        public SecurityDeclarationCollection SecurityDeclarations
        {
            get { return _securityDeclarations ?? (_securityDeclarations = new SecurityDeclarationCollection(this)); }
        }

        // TODO
        //public GenericParameterCollection GenericParameters
        //{
        //    get { return _genericParameters ?? (_genericParameters = new GenericParameterCollection(this)); }
        //}

        //public PInvokeImplementation PInvokeImplementation
        //{
        //    get
        //    {
        //        if (_pinvokeImplementation != null || Header == null)
        //            return _pinvokeImplementation;

        //        return _pinvokeImplementation = Header.GetStream<TableStream>()
        //            .GetTable<PInvokeImplementation>()
        //            .FirstOrDefault(x => x.MemberForwarded == this);
        //    }
        //}
        
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
            get { return Attributes.HasFlag(MethodAttributes.Static); }
            set { Attributes = Attributes.SetFlag(MethodAttributes.Static, value); }
        }

        public bool IsFinal
        {
            get { return Attributes.HasFlag(MethodAttributes.Final); }
            set { Attributes = Attributes.SetFlag(MethodAttributes.Final, value); }
        }

        public bool IsVirtual
        {
            get { return Attributes.HasFlag(MethodAttributes.Virtual); }
            set { Attributes = Attributes.SetFlag(MethodAttributes.Virtual, value); }
        }

        public bool IsHideBySig
        {
            get { return Attributes.HasFlag(MethodAttributes.HideBySig); }
            set { Attributes = Attributes.SetFlag(MethodAttributes.HideBySig, value); }
        }

        public bool IsAbstract
        {
            get { return Attributes.HasFlag(MethodAttributes.Abstract); }
            set { Attributes = Attributes.SetFlag(MethodAttributes.Abstract, value); }
        }

        public bool IsSpecialName
        {
            get { return Attributes.HasFlag(MethodAttributes.SpecialName); }
            set { Attributes = Attributes.SetFlag(MethodAttributes.SpecialName, value); }
        }

        public bool IsRuntimeSpecialName
        {
            get { return Attributes.HasFlag(MethodAttributes.RuntimeSpecialName); }
            set { Attributes = Attributes.SetFlag(MethodAttributes.RuntimeSpecialName, value); }
        }

        public bool HasSecurity
        {
            get { return Attributes.HasFlag(MethodAttributes.HasSecurity); }
            set { Attributes = Attributes.SetFlag(MethodAttributes.HasSecurity, value); }
        }

        public bool IsIL
        {
            get { return GetMethodCodeTypeAttribute(MethodImplAttributes.IL); }
            set { SetMethodCodeTypeAttribute(MethodImplAttributes.IL, value); }
        }

        public bool IsNative
        {
            get { return GetMethodCodeTypeAttribute(MethodImplAttributes.Native); }
            set { SetMethodCodeTypeAttribute(MethodImplAttributes.Native, value); }
        }

        public bool IsOPTIL
        {
            get { return GetMethodCodeTypeAttribute(MethodImplAttributes.OPTIL); }
            set { SetMethodCodeTypeAttribute(MethodImplAttributes.OPTIL, value); }
        }

        public bool IsRuntime
        {
            get { return GetMethodCodeTypeAttribute(MethodImplAttributes.Runtime); }
            set { SetMethodCodeTypeAttribute(MethodImplAttributes.Runtime, value); }
        }

        public bool IsManaged
        {
            get { return !IsUnmanaged; }
            set { IsUnmanaged = !value; }
        }

        public bool IsUnmanaged
        {
            get { return ImplAttributes.HasFlag(MethodImplAttributes.Unmanaged); }
            set { ImplAttributes = ImplAttributes.SetFlag(MethodImplAttributes.Unmanaged, value); }
        }

        public bool IsForwardRef
        {
            get { return ImplAttributes.HasFlag(MethodImplAttributes.ForwardRef); }
            set { ImplAttributes = ImplAttributes.SetFlag(MethodImplAttributes.ForwardRef, value); }
        }

        public bool NoOptimization
        {
            get { return ImplAttributes.HasFlag(MethodImplAttributes.NoOptimization); }
            set { ImplAttributes = ImplAttributes.SetFlag(MethodImplAttributes.NoOptimization, value); }
        }

        public bool PreserveSig
        {
            get { return ImplAttributes.HasFlag(MethodImplAttributes.PreserveSig); }
            set { ImplAttributes = ImplAttributes.SetFlag(MethodImplAttributes.PreserveSig, value); }
        }

        public bool IsInternalCall
        {
            get { return ImplAttributes.HasFlag(MethodImplAttributes.InternalCall); }
            set { ImplAttributes = ImplAttributes.SetFlag(MethodImplAttributes.InternalCall, value); }
        }

        public bool IsSynchronized
        {
            get { return ImplAttributes.HasFlag(MethodImplAttributes.Synchronized); }
            set { ImplAttributes = ImplAttributes.SetFlag(MethodImplAttributes.Synchronized, value); }
        }

        public bool NoInlining
        {
            get { return ImplAttributes.HasFlag(MethodImplAttributes.NoInlining); }
            set { ImplAttributes = ImplAttributes.SetFlag(MethodImplAttributes.NoInlining, value); }
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

        private bool GetMethodCodeTypeAttribute(MethodImplAttributes attribute)
        {
            return ((uint)ImplAttributes).GetMaskedAttribute((uint)MethodImplAttributes.CodeTypeMask,
                (uint)attribute);
        }

        private void SetMethodCodeTypeAttribute(MethodImplAttributes attribute, bool value)
        {
            ImplAttributes = (MethodImplAttributes)((uint)ImplAttributes).SetMaskedAttribute((uint)MethodImplAttributes.CodeTypeMask,
                (uint)attribute, value);
        }

        public override string ToString()
        {
            return FullName;
        }

        public IMetadataMember Resolve()
        {
            return this;
        }

        // TODO
        //IGenericParameterProvider IGenericContext.Type
        //{
        //    get { return DeclaringType; }
        //}

        //IGenericParameterProvider IGenericContext.Method
        //{
        //    get { return this; }
        //}
    }
}