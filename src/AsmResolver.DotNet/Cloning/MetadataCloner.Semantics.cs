namespace AsmResolver.DotNet.Cloning
{
    public partial class MetadataCloner
    {
        private void DeepCopyProperties(MetadataCloneContext context)
        {
            foreach (var property in _propertiesToClone)
            {
                var clonedProperty = DeepCopyProperty(context, property);
                
                // If property's declaring type is cloned as well, add the cloned property to the cloned type.
                if (context.ClonedMembers.TryGetValue(property.DeclaringType, out var member)
                    && member is TypeDefinition declaringType)
                {
                    declaringType.Properties.Add(clonedProperty);
                }
            }
        }

        private PropertyDefinition DeepCopyProperty(MetadataCloneContext context, PropertyDefinition property)
        {
            var clonedProperty = new PropertyDefinition(property.Name,
                property.Attributes,
                context.Importer.ImportPropertySignature(property.Signature));

            CloneSemantics(context, property, clonedProperty);

            return clonedProperty;
        }
        
        private void DeepCopyEvents(MetadataCloneContext context)
        {
            foreach (var @event in _eventsToClone)
            {
                var clonedEvent = DeepCopyEvent(context, @event);
                
                // If event's declaring type is cloned as well, add the cloned event to the cloned type.
                if (context.ClonedMembers.TryGetValue(@event.DeclaringType, out var member)
                    && member is TypeDefinition declaringType)
                {
                    declaringType.Events.Add(clonedEvent);
                }
            }
        }

        private EventDefinition DeepCopyEvent(MetadataCloneContext context, EventDefinition @event)
        {
            var clonedEvent = new EventDefinition(@event.Name,
                @event.Attributes,
                context.Importer.ImportType(@event.EventType));

            CloneSemantics(context, @event, clonedEvent);

            return clonedEvent;
        }

        private static void CloneSemantics(MetadataCloneContext context, IHasSemantics semanticsProvider,
            IHasSemantics clonedProvider)
        {
            foreach (var semantics in semanticsProvider.Semantics)
            {
                clonedProvider.Semantics.Add(new MethodSemantics(
                    (MethodDefinition) context.ClonedMembers[semantics.Method],
                    semantics.Attributes));
            }
        }
    }
}