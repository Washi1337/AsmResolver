using System;

namespace AsmResolver.DotNet.Cloning
{
    public partial class MemberCloner
    {
        private void DeepCopyProperties(MemberCloneContext context)
        {
            foreach (var property in _propertiesToClone)
            {
                var clonedProperty = DeepCopyProperty(context, property);

                // If property's declaring type is cloned as well, add the cloned property to the cloned type.
                if (property.DeclaringType is not null
                    && context.ClonedMembers.TryGetValue(property.DeclaringType, out var member)
                    && member is TypeDefinition declaringType)
                {
                    declaringType.Properties.Add(clonedProperty);
                }
                var clonedMember = clonedProperty;
                _clonerListener.OnClonedMember(property, clonedMember);
                _clonerListener.OnClonedProperty(property, clonedMember);
            }
        }

        private PropertyDefinition DeepCopyProperty(MemberCloneContext context, PropertyDefinition property)
        {
            if (property.Name is null)
                throw new ArgumentException($"Property {property.SafeToString()} has no name.");
            if (property.Signature is null)
                throw new ArgumentException($"Property {property.SafeToString()} has no signature.");

            var clonedProperty = new PropertyDefinition(property.Name,
                property.Attributes,
                context.Importer.ImportPropertySignature(property.Signature));

            CloneSemantics(context, property, clonedProperty);
            CloneCustomAttributes(context, property, clonedProperty);
            property.Constant = CloneConstant(property.Constant);

            return clonedProperty;
        }

        private void DeepCopyEvents(MemberCloneContext context)
        {
            foreach (var @event in _eventsToClone)
            {
                var clonedEvent = DeepCopyEvent(context, @event);

                // If event's declaring type is cloned as well, add the cloned event to the cloned type.
                if (@event.DeclaringType is not null
                    && context.ClonedMembers.TryGetValue(@event.DeclaringType, out var member)
                    && member is TypeDefinition declaringType)
                {
                    declaringType.Events.Add(clonedEvent);
                }
                var clonedMember = clonedEvent;
                _clonerListener.OnClonedMember(@event, clonedMember);
                _clonerListener.OnClonedEvent(@event, clonedMember);
            }
        }

        private static EventDefinition DeepCopyEvent(MemberCloneContext context, EventDefinition @event)
        {
            if (@event.Name is null)
                throw new ArgumentException($"Event {@event.SafeToString()} has no name.");
            if (@event.EventType is null)
                throw new ArgumentException($"Event {@event.SafeToString()} has no event-type.");

            var clonedEvent = new EventDefinition(@event.Name,
                @event.Attributes,
                context.Importer.ImportType(@event.EventType));

            CloneSemantics(context, @event, clonedEvent);
            CloneCustomAttributes(context, @event, clonedEvent);

            return clonedEvent;
        }

        private static void CloneSemantics(MemberCloneContext context, IHasSemantics semanticsProvider,
            IHasSemantics clonedProvider)
        {
            foreach (var semantics in semanticsProvider.Semantics)
            {
                clonedProvider.Semantics.Add(new MethodSemantics(
                    (MethodDefinition) context.ClonedMembers[semantics.Method!],
                    semantics.Attributes));
            }
        }
    }
}
