using System;
using System.Collections.Generic;

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
        private readonly HashSet<FieldDefinition> _fieldsToClone = new HashSet<FieldDefinition>();
        private readonly HashSet<PropertyDefinition> _propertiesToClone = new HashSet<PropertyDefinition>();
        
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

                // Include fields.
                foreach (var field in type.Fields)
                    Include(field);

                // Include properties.
                foreach (var property in type.Properties)
                    Include(property);

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
        /// <returns>The metadata cloner that the method is added to.</returns>
        public MetadataCloner Include(MethodDefinition method)
        {
            _methodsToClone.Add(method);
            return this;
        }

        /// <summary>
        /// Adds a single field to the list of members to clone.
        /// </summary>
        /// <param name="field">The field to include.</param>
        /// <returns>The metadata cloner that the field is added to.</returns>
        public MetadataCloner Include(FieldDefinition field)
        {
            _fieldsToClone.Add(field);
            return this;
        }

        public MetadataCloner Include(PropertyDefinition property)
        {
            _propertiesToClone.Add(property);
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
            CreateFieldStubs(context);
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

        private void DeepCopyMembers(MetadataCloneContext context)
        {
            DeepCopyTypes(context);
            DeepCopyMethods(context);
            DeepCopyFields(context);
            DeepCopyProperties(context);
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

    }
}