using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.DotNet.Cloning
{
    /// <summary>
    /// Provides a mechanism for deep-copying metadata members from external .NET modules into another module. 
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public partial class MetadataCloner
    {
        private readonly ModuleDefinition _targetModule;
        
        private readonly HashSet<TypeDefinition> _typesToClone = new HashSet<TypeDefinition>();
        private readonly HashSet<MethodDefinition> _methodsToClone = new HashSet<MethodDefinition>();
        
        /// <summary>
        /// Creates a new instance of the <see cref="MetadataCloner"/> class.
        /// </summary>
        /// <param name="targetModule">The target module to copy the members into.</param>
        public MetadataCloner(ModuleDefinition targetModule)
        {
            _targetModule = targetModule ?? throw new ArgumentNullException(nameof(targetModule));
        }

        /// <summary>
        /// Adds the provided type, and all its members and nested types, to the list of members to clone.
        /// </summary>
        /// <param name="type">The type to include.</param>
        /// <returns>The metadata cloner that this type was added to.</returns>
        public MetadataCloner Include(TypeDefinition type)
        {
            var agenda = new Stack<TypeDefinition>();
            agenda.Push(type);

            while (agenda.Count > 0)
            {
                type = agenda.Pop();
                _typesToClone.Add(type);

                // Include methods.
                foreach (var method in type.Methods)
                    Include(method);

                // Include remaining nested types.
                foreach (var nestedType in type.NestedTypes)
                    agenda.Push(nestedType);
            }

            return this;
        }

        /// <summary>
        /// Adds each type in the provided collection of types, and all their members and nested types, to the list
        /// of members to clone.
        /// </summary>
        /// <param name="types">The types to include.</param>
        /// <returns>The metadata cloner that the types were added to.</returns>
        public MetadataCloner Include(params TypeDefinition[] types) => 
            Include((IEnumerable<TypeDefinition>) types);
        
        /// <summary>
        /// Adds each type in the provided collection of types, and all their members and nested types, to the list
        /// of members to clone.
        /// </summary>
        /// <param name="types">The types to include.</param>
        /// <returns>The metadata cloner that the types were added to.</returns>
        public MetadataCloner Include(IEnumerable<TypeDefinition> types)
        {
            foreach (var type in types)
                Include(type);
            return this;
        }

        /// <summary>
        /// Adds a single method to the list of members to clone.
        /// </summary>
        /// <param name="method">The method to include.</param>
        /// <returns>The metadata cloner that the types were added to.</returns>
        public MetadataCloner Include(MethodDefinition method)
        {
            _methodsToClone.Add(method);
            return this;
        }

        /// <summary>
        /// Clones all included members.
        /// </summary>
        /// <returns>An object representing the result of the cloning process.</returns>
        public MetadataCloneResult Clone()
        {
            var context = new MetadataCloneContext(_targetModule);

            CreateMemberStubs(context);
            DeepCopyMembers(context);

            return new MetadataCloneResult(context.ClonedMembers.Values);
        }

        private void CreateMemberStubs(MetadataCloneContext context)
        {
            CreateTypeStubs(context);
            CreateMethodStubs(context);
        }

        private void CreateTypeStubs(MetadataCloneContext context)
        {
            foreach (var type in _typesToClone)
                CreateTypeStub(context, type);
        }

        private void CreateTypeStub(MetadataCloneContext context, TypeDefinition type)
        {
            var typeStub = new TypeDefinition(type.Namespace, type.Name, type.Attributes);
            context.ClonedMembers.Add(type, typeStub);
        }

        private void CreateMethodStubs(MetadataCloneContext context)
        {
            foreach (var method in _methodsToClone)
            {
                var stub = CreateMethodStub(context, method);
                
                // If method's declaring type is cloned as well, add the cloned method to the cloned type.
                if (context.ClonedMembers.TryGetValue(method.DeclaringType, out var member)
                    && member is TypeDefinition declaringType)
                {
                    declaringType.Methods.Add(stub);
                }
            }
        }

        private void DeepCopyMembers(MetadataCloneContext context)
        {
            DeepCopyTypes(context);
            DeepCopyMethods(context);
        }

        private void DeepCopyTypes(MetadataCloneContext context)
        {
            foreach (var type in _typesToClone)
                DeepCopyType(context, type);
        }

        private void DeepCopyType(MetadataCloneContext context, TypeDefinition type)
        {
            var clonedType = (TypeDefinition) context.ClonedMembers[type];
            clonedType.BaseType = context.Importer.ImportType(type.BaseType);
        }

        private void DeepCopyMethods(MetadataCloneContext context)
        {
            foreach (var method in _methodsToClone)
                DeepCopyMethod(context, method);
        }

    }
}