using System;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents a single member in the .NET metadata.
    /// </summary>
    public interface IMetadataMember
    {
        /// <summary>
        /// Gets the image that the member is stored in. 
        /// </summary>
        MetadataImage Image
        {
            get;
        }

        /// <summary>
        /// Gets the metadata token associated to the member.
        /// </summary>
        MetadataToken MetadataToken
        {
            get;
        }
    }

    /// <summary>
    /// Provides a base for a single member in the .NET metadata.
    /// </summary>
    /// <typeparam name="TRow"></typeparam>
    public abstract class MetadataMember<TRow> : IMetadataMember
        where TRow : MetadataRow
    {
        private MetadataToken _metadataToken;
        
        protected MetadataMember(MetadataToken token)
        {
            _metadataToken = token;
        }

        /// <summary>
        /// Gets a value indicating whether the member is editable or not.
        /// </summary>
        public bool IsReadOnly
        {
            get { return Image != null && !Image.Header.GetStream<TableStream>().IsReadOnly; }
        }

        /// <inheritdoc />
        public abstract MetadataImage Image
        {
            get;
        }

        /// <inheritdoc />
        public MetadataToken MetadataToken
        {
            get { return _metadataToken; }
            internal set
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
    
    /// <summary>
    /// Represents a member that can be indexed using the TypeDefOrRef coded index, and describes a (reference to a)
    /// type in the metadata.
    /// </summary>
    public interface ITypeDefOrRef : IMemberReference, IMemberRefParent, ITypeDescriptor, IResolvable
    {
    }

    /// <summary>
    /// Represents a member that can be indexed using the HasConstant coded index, and may own a constant value.
    /// </summary>
    public interface IHasConstant : IMetadataMember
    {
        /// <summary>
        /// Gets the constant value associated to this member, if present.
        /// </summary>
        Constant Constant
        {
            get;
        }
    }

    /// <summary>
    /// Represents a member that can be index using the HasCustomAttribute coded index, and may own a collection of
    /// custom attributes. This encompasses a lot of the different types of metadata members. 
    /// </summary>
    public interface IHasCustomAttribute : IMetadataMember
    {
        /// <summary>
        /// Gets a collection of custom attributes assigned to this member.
        /// </summary>
        CustomAttributeCollection CustomAttributes
        {
            get;
        }
    }

    /// <summary>
    /// Represents a member that can be indexed using the HasFieldMarshal coded index, and may own a field marshal
    /// descriptor.
    /// </summary>
    public interface IHasFieldMarshal : IMetadataMember
    {
        /// <summary>
        /// Gets the field marshal descriptor assigned to this member, if present.
        /// </summary>
        FieldMarshal FieldMarshal
        {
            get;
        }
    }

    /// <summary>
    /// Represents a member that can be indexed using the HasSecurityAttribute coded index, and may own a collection of
    /// security attributes.
    /// </summary>
    public interface IHasSecurityAttribute : IMetadataMember
    {
        /// <summary>
        /// Gets a collection of security attributes assigned to this member.
        /// </summary>
        SecurityDeclarationCollection SecurityDeclarations
        {
            get;
        }
    }

    /// <summary>
    /// Represents a member that can be indexed using the MemberRefParent coded index.
    /// </summary>
    public interface IMemberRefParent : IMetadataMember, INameProvider
    {
    }

    public interface IHasSemantics : IMemberReference
    {
        /// <summary>
        /// Gets a collection of method semantic objects that are associated to this member.
        /// </summary>
        MethodSemanticsCollection Semantics
        {
            get;
        }
    }

    /// <summary>
    /// Represents a member that can be indexed using the MethodDefOrRef coded index, and describes (a reference to a)
    /// method in metadata.
    /// </summary>
    public interface IMethodDefOrRef : IMemberReference, IResolvable
    {
        /// <summary>
        /// Gets the signature assigned to the member.
        /// </summary>
        MemberSignature Signature
        {
            get;
        }
    }

    /// <summary>
    /// Represents a member that can be indexed using the MemberForwarded coded index.
    /// </summary>
    public interface IMemberForwarded : IMemberReference
    {

    }

    /// <summary>
    /// Represents a member that can be indexed using the Implementation coded index.
    /// </summary>
    public interface IImplementation : IMetadataMember, INameProvider
    {
    }

    /// <summary>
    /// Represents a member that can be indexed using the CustomAttributeType coded index.
    /// </summary>
    public interface ICustomAttributeType : IMethodDefOrRef
    {
    }

    /// <summary>
    /// Represents a member that can be indexed using the ResolutionScope coded index.
    /// </summary>
    public interface IResolutionScope : IMetadataMember, INameProvider
    {

    }

    /// <summary>
    /// Represents a member that can be indexed using the HasGenericParameter coded index, and describes a member
    /// that may contain generic parameters.
    /// </summary>
    public interface IGenericParameterProvider : IMemberReference
    {
        /// <summary>
        /// Gets a collection of generic parameters this member defines.
        /// </summary>
        GenericParameterCollection GenericParameters
        {
            get;
        }
    }
}
