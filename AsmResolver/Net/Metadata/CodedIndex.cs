using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Metadata
{
    public enum CodedIndex
    {
        TypeDefOrRef,
        HasConstant,
        HasCustomAttribute,
        HasFieldMarshal,
        HasDeclSecurity,
        MemberRefParent,
        HasSemantics,
        MethodDefOrRef,
        MemberForwarded,
        Implementation,
        CustomAttributeType,
        ResolutionScope,
        TypeOrMethodDef,
    }

    public interface IMetadataMember
    {
        MetadataRow MetadataRow
        {
            get;
        }

        MetadataToken MetadataToken
        {
            get;
        }

        MetadataHeader Header
        {
            get;
        }
    }

    public interface ITypeDefOrRef : IMemberReference, IMemberRefParent, ITypeDescriptor
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
        FieldMarshal FieldMarshal
        {
            get;
        }
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
        MethodSemanticsCollection Semantics
        {
            get;
        }
    }

    public interface IMethodDefOrRef : IMemberReference
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
        GenericParameterCollection GenericParameters
        {
            get;
        }
    }
}
