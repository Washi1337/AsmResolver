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

            foreach (var semantics in property.Semantics)
            {
                clonedProperty.Semantics.Add(new MethodSemantics(
                    (MethodDefinition) context.ClonedMembers[semantics.Method],
                    semantics.Attributes));
            }

            return clonedProperty;
        }
    }
}