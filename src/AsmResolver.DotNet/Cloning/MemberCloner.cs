using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.DotNet.Cloning
{
    /// <summary>
    /// Provides a mechanism for deep-copying metadata members from external .NET modules into another module. 
    /// </summary>
    /// <remarks>
    /// When multiple members are cloned in one go, the member cloner will fix up any references between the cloned members. 
    /// For example, if a type or member is referenced in a method body, and this type or member is also included in the
    /// cloning process, the reference will be updated to the cloned member instead of imported. 
    /// </remarks>
    public partial class MemberCloner
    {
        private readonly ModuleDefinition _targetModule;

        private readonly HashSet<TypeDefinition> _typesToClone = new HashSet<TypeDefinition>();
        private readonly HashSet<MethodDefinition> _methodsToClone = new HashSet<MethodDefinition>();
        private readonly HashSet<FieldDefinition> _fieldsToClone = new HashSet<FieldDefinition>();
        private readonly HashSet<PropertyDefinition> _propertiesToClone = new HashSet<PropertyDefinition>();
        private readonly HashSet<EventDefinition> _eventsToClone = new HashSet<EventDefinition>();

        /// <summary>
        /// Creates a new instance of the <see cref="MemberCloner"/> class.
        /// </summary>
        /// <param name="targetModule">The target module to copy the members into.</param>
        public MemberCloner(ModuleDefinition targetModule)
        {
            _targetModule = targetModule ?? throw new ArgumentNullException(nameof(targetModule));
        }

        /// <summary>
        /// Adds the provided type, and all its members and nested types, to the list of members to clone.
        /// </summary>
        /// <param name="type">The type to include.</param>
        /// <returns>The metadata cloner that this type was added to.</returns>
        public MemberCloner Include(TypeDefinition type) => Include(type, true);

        /// <summary>
        /// Adds the provided type, and all its members, to the list of members to clone.
        /// </summary>
        /// <param name="type">The type to include.</param>
        /// <param name="recursive">Indicates whether all nested types should be included as well.</param>
        /// <returns>The metadata cloner that this type was added to.</returns>
        public MemberCloner Include(TypeDefinition type, bool recursive)
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

                // Include events.
                foreach (var @event in type.Events)
                    Include(@event);

                if (recursive)
                {
                    // Include remaining nested types.
                    foreach (var nestedType in type.NestedTypes)
                        agenda.Push(nestedType);
                }
            }

            return this;
        }

        /// <summary>
        /// Adds each type in the provided collection of types, and all their members and nested types, to the list
        /// of members to clone.
        /// </summary>
        /// <param name="types">The types to include.</param>
        /// <returns>The metadata cloner that the types were added to.</returns>
        public MemberCloner Include(params TypeDefinition[] types) => 
            Include((IEnumerable<TypeDefinition>) types);
        
        /// <summary>
        /// Adds each type in the provided collection of types, and all their members and nested types, to the list
        /// of members to clone.
        /// </summary>
        /// <param name="types">The types to include.</param>
        /// <returns>The metadata cloner that the types were added to.</returns>
        public MemberCloner Include(IEnumerable<TypeDefinition> types)
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
        public MemberCloner Include(MethodDefinition method)
        {
            _methodsToClone.Add(method);
            return this;
        }

        /// <summary>
        /// Adds a single field to the list of members to clone.
        /// </summary>
        /// <param name="field">The field to include.</param>
        /// <returns>The metadata cloner that the field is added to.</returns>
        public MemberCloner Include(FieldDefinition field)
        {
            _fieldsToClone.Add(field);
            return this;
        }

        /// <summary>
        /// Adds a single property to the list of members to clone.
        /// </summary>
        /// <param name="property">The property to include.</param>
        /// <returns>The metadata cloner that the property is added to.</returns>
        public MemberCloner Include(PropertyDefinition property)
        {
            _propertiesToClone.Add(property);
            return this;
        }

        /// <summary>
        /// Adds a single event to the list of members to clone.
        /// </summary>
        /// <param name="event">The event to include.</param>
        /// <returns>The metadata cloner that the event is added to.</returns>
        public MemberCloner Include(EventDefinition @event)
        {
            _eventsToClone.Add(@event);
            return this;
        }
        
        /// <summary>
        /// Clones all included members.
        /// </summary>
        /// <returns>An object representing the result of the cloning process.</returns>
        public MemberCloneResult Clone()
        {
            var context = new MemberCloneContext(_targetModule);

            CreateMemberStubs(context);
            DeepCopyMembers(context);

            return new MemberCloneResult(context.ClonedMembers.Values);
        }

        private void CreateMemberStubs(MemberCloneContext context)
        {
            CreateTypeStubs(context);
            CreateMethodStubs(context);
            CreateFieldStubs(context);
        }

        private void CreateTypeStubs(MemberCloneContext context)
        {
            foreach (var type in _typesToClone)
                CreateTypeStub(context, type);
        }

        private void CreateTypeStub(MemberCloneContext context, TypeDefinition type)
        {
            var typeStub = new TypeDefinition(type.Namespace, type.Name, type.Attributes);
            context.ClonedMembers.Add(type, typeStub);
        }

        private void DeepCopyMembers(MemberCloneContext context)
        {
            DeepCopyTypes(context);
            DeepCopyMethods(context);
            DeepCopyFields(context);
            DeepCopyProperties(context);
            DeepCopyEvents(context);
        }

        private void DeepCopyTypes(MemberCloneContext context)
        {
            foreach (var type in _typesToClone)
                DeepCopyType(context, type);
        }

        private void DeepCopyType(MemberCloneContext context, TypeDefinition type)
        {
            var clonedType = (TypeDefinition) context.ClonedMembers[type];
            clonedType.BaseType = context.Importer.ImportType(type.BaseType);
            
            // If the type is nested and the declaring type is cloned as well, we should add it to the cloned type. 
            if (type.IsNested 
                && context.ClonedMembers.TryGetValue(type.DeclaringType, out var member)
                && member is TypeDefinition clonedDeclaringType)
            {
                clonedDeclaringType.NestedTypes.Add(clonedType);
            }

            CloneCustomAttributes(context, type, clonedType);
        }

        private void CloneCustomAttributes(
            MemberCloneContext context, 
            IHasCustomAttribute sourceProvider,
            IHasCustomAttribute clonedProvider)
        {
            foreach (var attribute in sourceProvider.CustomAttributes)
                clonedProvider.CustomAttributes.Add(CloneCustomAttribute(context, attribute));
        }

        private CustomAttribute CloneCustomAttribute(MemberCloneContext context, CustomAttribute attribute)
        {
            var clonedSignature = new CustomAttributeSignature();

            // Fixed args.
            foreach (var argument in attribute.Signature.FixedArguments)
                clonedSignature.FixedArguments.Add(CloneCustomAttributeArgument(context, argument, clonedSignature));

            // Named args.
            foreach (var namedArgument in attribute.Signature.NamedArguments)
            {
                var clonedArgument = new CustomAttributeNamedArgument(
                    namedArgument.MemberType,
                    namedArgument.MemberName, 
                    namedArgument.ArgumentType,
                    CloneCustomAttributeArgument(context, namedArgument.Argument, clonedSignature));
                
                clonedSignature.NamedArguments.Add(clonedArgument);
            }

            var clonedAttribute = new CustomAttribute(
                (ICustomAttributeType) context.Importer.ImportMethod(attribute.Constructor),
                clonedSignature);
            return clonedAttribute;
        }

        private static CustomAttributeArgument CloneCustomAttributeArgument(MemberCloneContext context, CustomAttributeArgument argument,
            CustomAttributeSignature clonedSignature)
        {
            var clonedArgument = new CustomAttributeArgument(context.Importer.ImportTypeSignature(argument.ArgumentType));
            clonedArgument.IsNullArray = argument.IsNullArray;
            
            // Copy all elements.
            foreach (var element in argument.Elements)
                clonedArgument.Elements.Add(new CustomAttributeArgumentElement(element.Value));
            
            return clonedArgument;
        }

        private ImplementationMap CloneImplementationMap(MemberCloneContext context, ImplementationMap map)
        {
            return map != null
                ? new ImplementationMap(context.Importer.ImportModule(map.Scope), map.Name, map.Attributes)
                : null;
        }
    }
}