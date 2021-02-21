using System;
using AsmResolver.DotNet;

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
        public static readonly ObjectRelation<ITypeDefOrRef> BaseType = new(
            "BaseType",
            new Guid("B74FDD84-C147-4B23-B81E-CD63519CFD65"));

        /// <summary>
        /// Describes the relationship between a base method and an overriding method. This includes both abstract
        /// methods, as well as interface implementations.
        /// </summary>
        public static readonly ObjectRelation<MethodDefinition> ImplementationMethod = new(
            "ImplementationMethod",
            new Guid("2DE7DFED-2EAB-458A-BBA4-7F75A7DE199F"));

        /// <summary>
        /// Describes the relationship between a base property and an overriding property. This includes both abstract
        /// properties, as well as interface implementations.
        /// </summary>
        public static readonly ObjectRelation<IHasSemantics> ImplementationSemantics = new(
            "ImplementationSemantics",
            new Guid("d2cfdd88-a701-4051-b9ae-2d5d30e70905"));
    }
}
