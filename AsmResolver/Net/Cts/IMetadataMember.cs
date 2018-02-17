using System;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    public interface IMetadataMember
    {
        MetadataImage Image
        {
            get;
        }

        MetadataToken MetadataToken
        {
            get;
        }
    }

    public abstract class MetadataMember<TRow> : IMetadataMember
        where TRow : MetadataRow
    {
        private MetadataToken _metadataToken;

        protected MetadataMember(MetadataImage image, MetadataToken token)
        {
            Image = image;
            _metadataToken = token;
        }

        public bool IsReadOnly
        {
            get { return Image != null && !Image.Header.GetStream<TableStream>().IsReadOnly; }
        }

        public MetadataImage Image
        {
            get;
            internal set;
        }

        public MetadataToken MetadataToken
        {
            get { return _metadataToken; }
            set
            {
                AssertIsWriteable();
                _metadataToken = value;
            }
        }
        
        protected void AssertIsWriteable()
        {
            if (IsReadOnly)
                throw new InvalidOperationException("Metadata member cannot be modified in read-only mode.");
        }
    }
    
    public interface ITypeDefOrRef : IMemberReference, IMemberRefParent, ITypeDescriptor, IResolvable
    {
    }

    public interface IHasConstant : IMetadataMember
    {
        Constant Constant
        {
            get;
        }
    }

    public interface IHasCustomAttribute : IMetadataMember
    {
        CustomAttributeCollection CustomAttributes
        {
            get;
        }
    }

    public interface IHasFieldMarshal : IMetadataMember
    {
        //FieldMarshal FieldMarshal
        //{
        //    get;
        //}
    }

    public interface IHasSecurityAttribute : IMetadataMember
    {
        SecurityDeclarationCollection SecurityDeclarations
        {
            get;
        }
    }

    public interface IMemberRefParent : IMetadataMember, INameProvider
    {
    }

    public interface IHasSemantics : IMemberReference
    {
        //MethodSemanticsCollection Semantics
        //{
        //    get;
        //}
    }

    public interface IMethodDefOrRef : IMemberReference, IResolvable
    {
        MemberSignature Signature
        {
            get;
        }
    }

    public interface IMemberForwarded : IMemberReference
    {

    }

    public interface IImplementation : IMetadataMember
    {
        string Name
        {
            get;
        }
    }

    public interface ICustomAttributeType : IMethodDefOrRef
    {
    }

    public interface IResolutionScope : IMetadataMember, INameProvider
    {

    }

    public interface IGenericParameterProvider : IMemberReference
    {
        //GenericParameterCollection GenericParameters
        //{
        //    get;
        //}
    }
}
