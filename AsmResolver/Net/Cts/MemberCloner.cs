using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Cil;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Provides a mechanism for cloning members from a source assembly and importing all referenced members
    /// in the target assembly.
    /// </summary>
    public class MemberCloner
    {
        /// <summary>
        /// Provides a member reference importer that captures references to previously cloned members.
        /// </summary>
        private sealed class MemberClonerReferenceImporter : ReferenceImporter
        {
            private readonly MemberCloner _memberCloner;

            public MemberClonerReferenceImporter(MemberCloner memberCloner, MetadataImage image) 
                : base(image)
            {
                _memberCloner = memberCloner;
            }

            /// <inheritdoc />
            public override ITypeDefOrRef ImportType(ITypeDefOrRef reference)
            {
                return _memberCloner._createdMembers.TryGetValue(reference , out var newType)
                    ? (ITypeDefOrRef) newType
                    : base.ImportType(reference);
            }

            /// <inheritdoc />
            public override ITypeDefOrRef ImportType(TypeDefinition type)
            {
                return _memberCloner._createdMembers.TryGetValue(type, out var newType)
                    ? (ITypeDefOrRef) newType
                    : base.ImportType(type);
            }

            /// <inheritdoc />
            public override IMethodDefOrRef ImportMethod(MethodDefinition method)
            {
                return _memberCloner._createdMembers.TryGetValue(method, out var newMember)
                    ? (IMethodDefOrRef) newMember
                    : base.ImportMethod(method);
            }

            /// <inheritdoc />
            public override IMemberReference ImportField(FieldDefinition field)
            {
                return _memberCloner._createdMembers.TryGetValue(field, out var newMember)
                    ? newMember
                    : base.ImportField(field);
            }

            /// <inheritdoc />
            public override MemberReference ImportMember(MemberReference reference)
            {
                return _memberCloner._createdMembers.TryGetValue(reference, out var newMember)
                    ? (MemberReference) newMember
                    : base.ImportMember(reference);
            }

            /// <inheritdoc />
            public override IMemberReference ImportReference(IMemberReference reference)
            {
                return _memberCloner._createdMembers.TryGetValue(reference, out var newMember)
                    ? newMember
                    : base.ImportReference(reference);
            }
        }

        // Cache for created members.
        private readonly IDictionary<IMemberReference, IMemberReference> _createdMembers =
            new Dictionary<IMemberReference, IMemberReference>(new SignatureComparer());
        private readonly IReferenceImporter _importer;

        /// <summary>
        /// Creates a new member cloner targeting the provided image.
        /// </summary>
        /// <param name="targetImage">The image to target.</param>
        /// <exception cref="ArgumentNullException">Occurs when <paramref name="targetImage"/> is <c>null</c></exception>
        public MemberCloner(MetadataImage targetImage)
        {
            if (targetImage == null)
                throw new ArgumentNullException(nameof(targetImage));
            _importer = new MemberClonerReferenceImporter(this, targetImage);
        }

        /// <summary>
        /// Clones a collection of types, and replaces all cross references made in those types to the newly created members.
        /// </summary>
        /// <param name="types">The types to clone.</param>
        /// <returns>The cloned types.</returns>
        public IList<TypeDefinition> CloneTypes(IList<TypeDefinition> types)
        {
            // First create stubs of the types and members, so that we can make references to them.
            var newTypes = types.Select(CreateTypeStub).ToArray();
            for (var index = 0; index < newTypes.Length; index++)
                DeclareMemberStubs(types[index], newTypes[index]);
            
            // Fill in all the gaps.
            for (var index = 0; index < newTypes.Length; index++)
                FinalizeTypeStub(types[index], newTypes[index]);

            return newTypes;
        }
        
        /// <summary>
        /// Clones a single type and imports all references used by the type into the image.
        /// </summary>
        /// <param name="type">The type to clone.</param>
        /// <returns>The cloned type.</returns>
        public TypeDefinition CloneType(TypeDefinition type)
        {
            var newType = CreateTypeStub(type);
            
            // First declare member stubs so that we can make references to them.
            DeclareMemberStubs(type, newType);
            
            // Fill in all the gaps afterwards.
            FinalizeTypeStub(type, newType);
            
            return newType;
        }

        /// <summary>
        /// Clones a single field and imports all references used by the field into the image.
        /// </summary>
        /// <param name="field">The field to clone.</param>
        /// <returns>The cloned field.</returns>
        private FieldDefinition CloneField(FieldDefinition field)
        {
            var newField = new FieldDefinition(field.Name, field.Attributes, _importer.ImportFieldSignature(field.Signature));

            if (newField.Constant != null)
                newField.Constant = new Constant(field.Constant.ConstantType, new DataBlobSignature(field.Constant.Value.Data));
            
            if (newField.PInvokeMap != null)
                newField.PInvokeMap = CloneImplementationMap(field.PInvokeMap);

            _createdMembers.Add(field, newField);
            CloneCustomAttributes(field, newField);
            
            return newField;
        }

        /// <summary>
        /// Creates a stub of the type, including just the namespace, name and attributes of the type.
        /// </summary>
        /// <param name="type">The type to clone.</param>
        /// <returns>The stub of the cloned type.</returns>
        private TypeDefinition CreateTypeStub(TypeDefinition type)
        {
            var newType = new TypeDefinition(type.Namespace, type.Name, type.Attributes);
            _createdMembers.Add(type, newType);
            
            foreach (var nestedType in type.NestedClasses)
                newType.NestedClasses.Add(new NestedClass(CreateTypeStub(nestedType.Class)));

            return newType;
        }

        /// <summary>
        /// Creates a stub of all the members defined by the type.
        /// </summary>
        /// <param name="type">The type to clone the members from.</param>
        /// <param name="stub">The stub of the cloned type to add the member stubs to.</param>
        private void DeclareMemberStubs(TypeDefinition type, TypeDefinition stub)
        {
            // Process nested classes.
            foreach (var nestedType in type.NestedClasses)
                DeclareMemberStubs(nestedType.Class, (TypeDefinition) _createdMembers[nestedType.Class]);
            
            // Fields
            foreach (var field in type.Fields)
                stub.Fields.Add(CloneField(field));
            
            // Methods
            foreach (var method in type.Methods)
                stub.Methods.Add(CreateMethodStub(method));
            
            // Properties
            if (type.PropertyMap != null)
            {
                stub.PropertyMap = new PropertyMap();
                foreach (var property in type.PropertyMap.Properties)
                    stub.PropertyMap.Properties.Add(CloneProperty(property));
            }
            
            // Events.
            if (type.EventMap != null)
            {
                stub.EventMap = new EventMap();
                foreach (var @event in type.EventMap.Events)
                    stub.EventMap.Events.Add(CloneEvent(@event));
            }
        }

        /// <summary>
        /// Finalizes the entire type stub, including the member stubs.
        /// </summary>
        /// <param name="type">The type to be cloned.</param>
        /// <param name="stub">The stub of the cloned type.</param>
        private void FinalizeTypeStub(TypeDefinition type, TypeDefinition stub)
        {
            foreach (var nestedType in type.NestedClasses)
                FinalizeTypeStub(nestedType.Class, (TypeDefinition) _createdMembers[nestedType.Class]);
            
            foreach (var method in type.Methods)
                FinalizeMethod(method, (MethodDefinition) _createdMembers[method]);
            
            if (type.BaseType != null)
                stub.BaseType = _importer.ImportType(type.BaseType);

            if (type.ClassLayout != null)
                stub.ClassLayout = new ClassLayout(type.ClassLayout.ClassSize, type.ClassLayout.PackingSize);

            CloneGenericParameters(type, stub);
            CloneCustomAttributes(type, stub);
        }

        /// <summary>
        /// Clones a single event definition.
        /// </summary>
        /// <param name="event">The event to be cloned.</param>
        /// <returns>The cloned event.</returns>
        private EventDefinition CloneEvent(EventDefinition @event)
        {
            var newEvent = new EventDefinition(
                @event.Name,
                _importer.ImportType(@event.EventType));

            CloneSemantics(@event, newEvent);
            CloneCustomAttributes(@event, newEvent);
            
            return newEvent;
        }

        /// <summary>
        /// Clones a single property definition.
        /// </summary>
        /// <param name="property">The property to be cloned.</param>
        /// <returns>The cloned property.</returns>
        private PropertyDefinition CloneProperty(PropertyDefinition property)
        {
            var newProperty = new PropertyDefinition(
                property.Name,
                _importer.ImportPropertySignature(property.Signature));

            CloneSemantics(property, newProperty);
            CloneCustomAttributes(property, newProperty);
            
            return newProperty;
        }

        /// <summary>
        /// Clones the semantic members of a property or an event.
        /// </summary>
        /// <param name="owner">The property or event.</param>
        /// <param name="newOwner">The cloned property or event to add the semantic members to.</param>
        private void CloneSemantics(IHasSemantics owner, IHasSemantics newOwner)
        {
            foreach (var semantic in owner.Semantics)
            {
                newOwner.Semantics.Add(new MethodSemantics(
                    (MethodDefinition) _createdMembers[semantic.Method],
                    semantic.Attributes));
            }
        }

        /// <summary>
        /// Creates a stub of a method definition, including the name and signature of the method.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private MethodDefinition CreateMethodStub(MethodDefinition method)
        {
            var newMethod = new MethodDefinition(
                method.Name,
                method.Attributes,
                _importer.ImportMethodSignature(method.Signature));

            foreach (var parameter in method.Parameters)
                newMethod.Parameters.Add(new ParameterDefinition(parameter.Sequence, parameter.Name, parameter.Attributes));

            if (method.PInvokeMap != null)
                newMethod.PInvokeMap = CloneImplementationMap(method.PInvokeMap);
            
            _createdMembers.Add(method, newMethod);
            return newMethod;
        }

        /// <summary>
        /// Finalizes a method stub by cloning the method body and other associated properties.
        /// </summary>
        /// <param name="method">The original method to be cloned.</param>
        /// <param name="stub">The stub of the cloned method.</param>
        private void FinalizeMethod(MethodDefinition method, MethodDefinition stub)
        {
            CloneGenericParameters(method, stub);
            CloneCustomAttributes(method, stub);
            if (method.CilMethodBody != null)
                stub.CilMethodBody = CloneCilMethodBody(method.CilMethodBody, stub);
        }

        /// <summary>
        /// Clones an implementation mapping used by P/Invoke members.
        /// </summary>
        /// <param name="map">The mapping to clone.</param>
        /// <returns>The cloned mapping.</returns>
        private ImplementationMap CloneImplementationMap(ImplementationMap map)
        {
            return new ImplementationMap(_importer.ImportModule(map.ImportScope), map.ImportName, map.Attributes);
        }
        
        /// <summary>
        /// Clones a collection of generic parameters of a member. 
        /// </summary>
        /// <param name="source">The member containing the generic parameters to clone.</param>
        /// <param name="newOwner">The member to add the cloned parameters to.</param>
        private void CloneGenericParameters(IGenericParameterProvider source, IGenericParameterProvider newOwner)
        {
            foreach (var parameter in source.GenericParameters)
            {
                var newParameter = new GenericParameter(parameter.Index, parameter.Name, parameter.Attributes);
                
                // Constraints
                foreach (var constraint in newParameter.Constraints)
                {
                    newParameter.Constraints.Add(new GenericParameterConstraint(
                        _importer.ImportType(constraint.Constraint)));
                }
                
                newOwner.GenericParameters.Add(newParameter);
            }
        }

        /// <summary>
        /// Clones a collection of custom attributes of a member. 
        /// </summary>
        /// <param name="source">The member containing the custom attributes to clone.</param>
        /// <param name="newOwner">The member to add the cloned attributes to.</param>
        private void CloneCustomAttributes(IHasCustomAttribute source, IHasCustomAttribute newOwner)
        {
            foreach (var attribute in source.CustomAttributes)
            {
                var signature = new CustomAttributeSignature();
                
                // Fixed args.
                foreach (var argument in attribute.Signature.FixedArguments)
                    signature.FixedArguments.Add(CloneAttributeArgument(argument));
                
                // Named args.
                foreach (var argument in attribute.Signature.NamedArguments)
                {
                    signature.NamedArguments.Add(new CustomAttributeNamedArgument(argument.ArgumentMemberType,
                        _importer.ImportTypeSignature(argument.ArgumentType), argument.MemberName,
                        CloneAttributeArgument(argument.Argument)));
                }

                var newAttribute = new CustomAttribute((ICustomAttributeType) _importer.ImportReference(attribute.Constructor), signature);
                newOwner.CustomAttributes.Add(newAttribute);
            }
        }

        /// <summary>
        /// Clones a single argument used in a custom attribute.
        /// </summary>
        /// <param name="argument">The argument to clone.</param>
        /// <returns>The cloned argument.</returns>
        private CustomAttributeArgument CloneAttributeArgument(CustomAttributeArgument argument)
        {
            var newArgument = new CustomAttributeArgument(_importer.ImportTypeSignature(argument.ArgumentType));
            foreach (var element in argument.Elements)
                newArgument.Elements.Add(new ElementSignature(element.Value));
            return newArgument;
        }

        /// <summary>
        /// Clones a CIL method body.
        /// </summary>
        /// <param name="body">The method body to clone.</param>
        /// <param name="newOwner">The method to add the cloned method body to.</param>
        /// <returns>The cloned method body.</returns>
        private CilMethodBody CloneCilMethodBody(CilMethodBody body, MethodDefinition newOwner)
        {
            var newBody = new CilMethodBody(newOwner)
            {
                InitLocals = body.InitLocals,
                MaxStack = body.MaxStack,
            };

            if (body.Signature != null)
                newBody.Signature = _importer.ImportStandAloneSignature(body.Signature);

            CloneInstructions(body, newBody);
            CloneExceptionHandlers(body, newBody);

            return newBody;
        }

        /// <summary>
        /// Clones the instructions of a method body.
        /// </summary>
        /// <param name="body">The method body to copy the instructions from.</param>
        /// <param name="newBody">The cloned method body to add the cloned instructions to.</param>
        private void CloneInstructions(CilMethodBody body, CilMethodBody newBody)
        {
            var branchInstructions = new List<CilInstruction>();
            var switchInstructions = new List<CilInstruction>();
            foreach (var instruction in body.Instructions)
            {
                var operand = instruction.Operand;
                
                // Make necessary changes to the operand when required.
                switch (operand)
                {
                    case IMemberReference member:
                        // Import reference.
                        operand = _importer.ImportReference((IMemberReference) operand);
                        break;
                    
                    case VariableSignature variable:
                        // Reference the new variable instead.
                        var oldVarSig = (LocalVariableSignature) body.Signature.Signature;
                        var newVarSig = (LocalVariableSignature) newBody.Signature.Signature;
                        operand = newVarSig.Variables[oldVarSig.Variables.IndexOf(variable)];
                        break;
                        
                    case ParameterSignature parameter:
                        // Reference the new parameter instead.
                        var oldParameters = body.Method.Signature.Parameters;
                        var newParameters = newBody.Method.Signature.Parameters;
                        operand = newParameters[oldParameters.IndexOf((ParameterSignature) operand)];
                        break;
                }

                var newInstruction = new CilInstruction(instruction.Offset, instruction.OpCode, operand);
                newBody.Instructions.Add(newInstruction);
                
                // Keep track of branches that we need to update later.
                switch (instruction.OpCode.OperandType)
                {
                    case CilOperandType.InlineBrTarget:
                    case CilOperandType.ShortInlineBrTarget:
                        branchInstructions.Add(newInstruction);
                        break;
                    case CilOperandType.InlineSwitch:
                        switchInstructions.Add(newInstruction);
                        break;
                }
            }

            // Update branch targets.
            foreach (var branch in branchInstructions)
                branch.Operand = newBody.Instructions.GetByOffset(((CilInstruction) branch.Operand).Offset);

            // Update switch targets.
            foreach (var @switch in switchInstructions)
            {
                var targets = (IEnumerable<CilInstruction>) @switch.Operand;
                var newTargets = new List<CilInstruction>();
                foreach (var target in targets)
                    newTargets.Add(newBody.Instructions.GetByOffset(target.Offset));
                @switch.Operand = newTargets;
            }
        }

        /// <summary>
        /// Clones a collection of exception handlers defined by the provided method body.
        /// </summary>
        /// <param name="body">The method body containing the exception handlers to clone.</param>
        /// <param name="newBody">The cloned method body to add the cloned exception handlers to.</param>
        private void CloneExceptionHandlers(CilMethodBody body, CilMethodBody newBody)
        {
            foreach (var handler in body.ExceptionHandlers)
            {
                var newHandler = new ExceptionHandler(handler.HandlerType)
                {
                    TryStart = newBody.Instructions.GetByOffset(handler.TryStart.Offset),
                    TryEnd = newBody.Instructions.GetByOffset(handler.TryEnd.Offset),
                    HandlerStart = newBody.Instructions.GetByOffset(handler.HandlerStart.Offset),
                    HandlerEnd = newBody.Instructions.GetByOffset(handler.HandlerEnd.Offset),
                };
                
                if (handler.FilterStart != null)
                    newHandler.FilterStart = newBody.Instructions.GetByOffset(handler.FilterStart.Offset);
                if (handler.CatchType != null)
                    newHandler.CatchType = _importer.ImportType(handler.CatchType);
                
                newBody.ExceptionHandlers.Add(newHandler);
            }
        }
    }
}