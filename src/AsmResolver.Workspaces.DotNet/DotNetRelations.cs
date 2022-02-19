using System;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.Workspaces.DotNet
{
    /// <summary>
    /// Defines default relations between objects in a .NET assembly.
    /// </summary>
    public static class DotNetRelations
    {
        /// <summary>
        /// Describes the relationship between a base type and a derived type. This includes both abstract classes
        /// as well as interfaces.
        /// </summary>
        public static readonly ObjectRelation<TypeDefinition, ITypeDefOrRef> BaseType = new(
            "BaseType",
            new Guid("B74FDD84-C147-4B23-B81E-CD63519CFD65"));

        /// <summary>
        /// Describes the relationship between a base method and an overriding method. This includes both abstract
        /// methods, as well as interface implementations.
        /// </summary>
        public static readonly ObjectRelation<MethodDefinition, IMethodDescriptor> ImplementationMethod = new(
            "ImplementationMethod",
            new Guid("2DE7DFED-2EAB-458A-BBA4-7F75A7DE199F"));

        /// <summary>
        /// Describes the relationship between a base property or event and an overriding property or event. This
        /// includes both abstract properties and events, as well as interface implementations.
        /// </summary>
        public static readonly ObjectRelation<IHasSemantics, IHasSemantics> ImplementationSemantics = new(
            "ImplementationSemantics",
            new Guid("d2cfdd88-a701-4051-b9ae-2d5d30e70905"));

        /// <summary>
        /// Describes the relationship between a assembly definition and its reference.
        /// </summary>
        public static readonly ObjectRelation<AssemblyDefinition, AssemblyReference> ReferenceAssembly = new(
            "ReferenceAssembly",
            new Guid("52a81339-0850-4f81-b059-30d7aacc430f"));

        /// <summary>
        /// Describes the relationship between a method or field definition and its reference.
        /// </summary>
        public static readonly ObjectRelation<IMemberDefinition, MemberReference> ReferenceMember = new(
            "ReferenceMember",
            new Guid("ce11d2f6-a423-429d-ad37-2f073fdf63be"));

        /// <summary>
        /// Describes the relationship between a method and its specification.
        /// </summary>
        public static readonly ObjectRelation<IMethodDefOrRef, MethodSpecification> ReferenceMethodSpecification = new(
            "ReferenceMethodSpecification",
            new Guid("3824566f-3df4-4188-8e67-ed2d745df84a"));


        /// <summary>
        /// Describes the relationship between a type definition and its reference.
        /// </summary>
        public static readonly ObjectRelation<TypeDefinition, TypeReference> ReferenceType = new(
            "ReferenceType",
            new Guid("3cc86779-338c-4165-a00c-da547a2e8549"));

        /// <summary>
        /// Describes the relationship between a type definition and its specification.
        /// </summary>
        public static readonly ObjectRelation<TypeDefinition, TypeSpecification> ReferenceTypeSpecification = new(
            "ReferenceTypeSpecification",
            new Guid("e60a2deb-b608-4775-93ad-c073eac934e5"));

        /// <summary>
        /// Describes the relationship between a exported type and its definition.
        /// </summary>
        public static readonly ObjectRelation<TypeDefinition, ExportedType> ReferenceExportedType = new(
            "ReferenceExportedType",
            new Guid("4a97daf4-8145-4ae9-a1c4-5e0b0ebcc864"));

        /// <summary>
        /// Describes the relationship between field or property definition and its a custom attribute named argument.
        /// </summary>
        public static readonly ObjectRelation<IMemberDefinition, CustomAttributeNamedArgument> ReferenceArgument = new(
            "ReferenceArgument",
            new Guid("36863f58-b4ab-40b4-bee9-7ab107ea75ce"));
    }
}
