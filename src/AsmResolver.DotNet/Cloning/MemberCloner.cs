using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Marshal;
using AsmResolver.DotNet.Signatures.Security;
using AsmResolver.PE.DotNet.Metadata.Tables;

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

        private readonly HashSet<TypeDefinition> _typesToClone = new();
        private readonly HashSet<MethodDefinition> _methodsToClone = new();
        private readonly HashSet<FieldDefinition> _fieldsToClone = new();
        private readonly HashSet<PropertyDefinition> _propertiesToClone = new();
        private readonly HashSet<EventDefinition> _eventsToClone = new();

        /// <summary>
        /// Creates a new instance of the <see cref="MemberCloner"/> class.
        /// </summary>
        /// <param name="targetModule">The target module to copy the members into.</param>
        public MemberCloner(ModuleDefinition targetModule)
        {
            _targetModule = targetModule ?? throw new ArgumentNullException(nameof(targetModule));
        }

        /// <summary>
        /// Gets or sets the object responsible for cloning a data segment of a field RVA (initialization data) entry.
        /// </summary>
        public IFieldRvaCloner FieldRvaCloner
        {
            get;
            set;
        } = new FieldRvaCloner();

        /// <summary>
        /// Adds the provided member definition to the list of members to clone.
        /// </summary>
        /// <param name="member">The member to clone.</param>
        /// <returns>The metadata cloner that this member was added to.</returns>
        /// <remarks>
        /// If <paramref name="member"/> refers to a <see cref="TypeDefinition"/>, all nested types will be included
        /// as well.
        /// </remarks>
        public MemberCloner Include(IMemberDefinition member) => Include(member, true);

        /// <summary>
        /// Adds the provided member definition to the list of members to clone.
        /// </summary>
        /// <param name="member">The member to clone.</param>
        /// <param name="recursive">
        /// If <paramref name="member"/> refers to a <see cref="TypeDefinition"/>, indicates
        /// whether all nested types should be included as well.</param>
        /// <returns>The metadata cloner that this member was added to.</returns>
        public MemberCloner Include(IMemberDefinition member, bool recursive) => member.MetadataToken.Table switch
        {
            TableIndex.TypeDef => Include((TypeDefinition) member, recursive),
            TableIndex.Field => Include((FieldDefinition) member),
            TableIndex.Method => Include((MethodDefinition) member),
            TableIndex.Event => Include((EventDefinition) member),
            TableIndex.Property => Include((PropertyDefinition) member),
            _ => throw new ArgumentOutOfRangeException()
        };

        /// <summary>
        /// Adds each member in the provided collection of member.
        /// </summary>
        /// <param name="members">The members to include.</param>
        /// <returns>The metadata cloner that the members were added to.</returns>
        /// <remarks>If a member in the list is a type, all their members and nested types will be included as well.</remarks>
        public MemberCloner Include(params IMemberDefinition[] members)
        {
            foreach (var member in members)
                Include(member);
            return this;
        }

        /// <summary>
        /// Adds each member in the provided collection of member.
        /// </summary>
        /// <param name="members">The members to include.</param>
        /// <returns>The metadata cloner that the members were added to.</returns>
        /// <remarks>If a member in the list is a type, all their members and nested types will be included as well.</remarks>
        public MemberCloner Include(IEnumerable<IMemberDefinition> members)
        {
            foreach (var member in members)
                Include(member);
            return this;
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

            return new MemberCloneResult(context.ClonedMembers);
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

        private static void CreateTypeStub(MemberCloneContext context, TypeDefinition type)
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

            // Copy base type.
            if (type.BaseType is { } baseType)
                clonedType.BaseType = context.Importer.ImportType(baseType);

            // Copy interface implementations.
            foreach (var implementation in type.Interfaces)
                clonedType.Interfaces.Add(CloneInterfaceImplementation(context, implementation));

            // Copy method implementations.
            foreach (var implementation in type.MethodImplementations)
            {
                clonedType.MethodImplementations.Add(new MethodImplementation(
                    context.Importer.ImportMethod(implementation.Declaration),
                    context.Importer.ImportMethod(implementation.Body)
                ));
            }

            // If the type is nested and the declaring type is cloned as well, we should add it to the cloned type.
            if (type.IsNested
                && context.ClonedMembers.TryGetValue(type.DeclaringType, out var member)
                && member is TypeDefinition clonedDeclaringType)
            {
                clonedDeclaringType.NestedTypes.Add(clonedType);
            }

            // Clone class layout.
            if (type.ClassLayout is { } layout)
                clonedType.ClassLayout = new ClassLayout(layout.PackingSize, layout.ClassSize);

            // Clone remaining metadata.
            CloneCustomAttributes(context, type, clonedType);
            CloneGenericParameters(context, type, clonedType);
            CloneSecurityDeclarations(context, type, clonedType);
        }

        private static InterfaceImplementation CloneInterfaceImplementation(
            MemberCloneContext context,
            InterfaceImplementation implementation)
        {
            var clonedImplementation = new InterfaceImplementation(context.Importer.ImportType(implementation.Interface));
            CloneCustomAttributes(context, implementation, clonedImplementation);
            return clonedImplementation;
        }

        private static void CloneCustomAttributes(
            MemberCloneContext context,
            IHasCustomAttribute sourceProvider,
            IHasCustomAttribute clonedProvider)
        {
            foreach (var attribute in sourceProvider.CustomAttributes)
                clonedProvider.CustomAttributes.Add(CloneCustomAttribute(context, attribute));
        }

        private static CustomAttribute CloneCustomAttribute(MemberCloneContext context, CustomAttribute attribute)
        {
            var clonedSignature = new CustomAttributeSignature();

            if (attribute.Signature is not null)
            {
                // Fixed args.
                foreach (var argument in attribute.Signature.FixedArguments)
                    clonedSignature.FixedArguments.Add(CloneCustomAttributeArgument(context, argument));

                // Named args.
                foreach (var namedArgument in attribute.Signature.NamedArguments)
                {
                    var clonedArgument = new CustomAttributeNamedArgument(
                        namedArgument.MemberType,
                        namedArgument.MemberName,
                        namedArgument.ArgumentType,
                        CloneCustomAttributeArgument(context, namedArgument.Argument));

                    clonedSignature.NamedArguments.Add(clonedArgument);
                }
            }

            return new CustomAttribute(
                (ICustomAttributeType) context.Importer.ImportMethod(attribute.Constructor),
                clonedSignature);
        }

        private static CustomAttributeArgument CloneCustomAttributeArgument(MemberCloneContext context, CustomAttributeArgument argument)
        {
            var clonedArgument = new CustomAttributeArgument(context.Importer.ImportTypeSignature(argument.ArgumentType));
            clonedArgument.IsNullArray = argument.IsNullArray;

            // Copy all elements.
            for (int i = 0; i < argument.Elements.Count; i++)
                clonedArgument.Elements.Add(argument.Elements[i]);

            return clonedArgument;
        }

        private static ImplementationMap CloneImplementationMap(MemberCloneContext context, ImplementationMap? map)
        {
            return map is not null
                ? new ImplementationMap(context.Importer.ImportModule(map.Scope), map.Name, map.Attributes)
                : null;
        }

        private static Constant CloneConstant(MemberCloneContext context, Constant? constant)
        {
            return constant is not null
                ? new Constant(constant.Type,
                    constant.Value is null
                    ? null
                    : new DataBlobSignature(constant.Value.Data))
                : null;
        }

        private void CloneGenericParameters(
            MemberCloneContext context,
            IHasGenericParameters sourceProvider,
            IHasGenericParameters clonedProvider)
        {
            foreach (var parameter in sourceProvider.GenericParameters)
                clonedProvider.GenericParameters.Add(CloneGenericParameter(context, parameter));
        }

        private static GenericParameter CloneGenericParameter(MemberCloneContext context, GenericParameter parameter)
        {
            var clonedParameter = new GenericParameter(parameter.Name, parameter.Attributes);

            foreach (var constraint in parameter.Constraints)
                clonedParameter.Constraints.Add(CloneGenericParameterConstraint(context, constraint));

            CloneCustomAttributes(context, parameter, clonedParameter);
            return clonedParameter;
        }

        private static GenericParameterConstraint CloneGenericParameterConstraint(
            MemberCloneContext context,
            GenericParameterConstraint constraint)
        {
            var clonedConstraint = new GenericParameterConstraint(context.Importer.ImportType(constraint.Constraint));

            CloneCustomAttributes(context, constraint, clonedConstraint);
            return clonedConstraint;
        }

        private static MarshalDescriptor CloneMarshalDescriptor(
            MemberCloneContext context,
            MarshalDescriptor? marshalDescriptor)
        {
            return marshalDescriptor switch
            {
                null => null,

                ComInterfaceMarshalDescriptor com => new ComInterfaceMarshalDescriptor(com.NativeType),

                CustomMarshalDescriptor custom => new CustomMarshalDescriptor(
                    custom.Guid, custom.NativeTypeName,
                    context.Importer.ImportTypeSignature(custom.MarshalType),
                    custom.Cookie),

                FixedArrayMarshalDescriptor fixedArray => new FixedArrayMarshalDescriptor
                {
                    Size = fixedArray.Size,
                    ArrayElementType = fixedArray.ArrayElementType
                },

                FixedSysStringMarshalDescriptor fixedSysString => new FixedSysStringMarshalDescriptor(
                    fixedSysString.Size),

                LPArrayMarshalDescriptor lpArray => new LPArrayMarshalDescriptor(lpArray.ArrayElementType),

                SafeArrayMarshalDescriptor safeArray => new SafeArrayMarshalDescriptor(
                    safeArray.VariantType, safeArray.VariantTypeFlags,
                    context.Importer.ImportTypeSignature(safeArray.UserDefinedSubType)),

                SimpleMarshalDescriptor simple => new SimpleMarshalDescriptor(simple.NativeType),

                _ => throw new ArgumentOutOfRangeException(nameof(marshalDescriptor))
            };
        }

        private static void CloneSecurityDeclarations(
            MemberCloneContext context,
            IHasSecurityDeclaration sourceProvider,
            IHasSecurityDeclaration clonedProvider)
        {
            foreach (var declaration in sourceProvider.SecurityDeclarations)
                clonedProvider.SecurityDeclarations.Add(CloneSecurityDeclaration(context, declaration));
        }

        private static SecurityDeclaration CloneSecurityDeclaration(MemberCloneContext context, SecurityDeclaration declaration)
        {
            return new(declaration.Action, ClonePermissionSet(context, declaration.PermissionSet));
        }

        private static PermissionSetSignature ClonePermissionSet(MemberCloneContext context, PermissionSetSignature permissionSet)
        {
            var result = new PermissionSetSignature();
            foreach (var attribute in permissionSet.Attributes)
                result.Attributes.Add(CloneSecurityAttribute(context, attribute));
            return result;
        }

        private static SecurityAttribute CloneSecurityAttribute(MemberCloneContext context, SecurityAttribute attribute)
        {
            var result = new SecurityAttribute(context.Importer.ImportTypeSignature(attribute.AttributeType));

            foreach (var argument in attribute.NamedArguments)
            {
                var newArgument = new CustomAttributeNamedArgument(
                    argument.MemberType,
                    argument.MemberName,
                    context.Importer.ImportTypeSignature(argument.ArgumentType),
                    CloneCustomAttributeArgument(context, argument.Argument));

                result.NamedArguments.Add(newArgument);
            }

            return result;
        }

    }
}
