namespace AsmResolver.DotNet.Cloning
{
    /// <inheritdoc/>
    public abstract class MemberClonerListener : IMemberClonerListener
    {
        /// <inheritdoc/>
        public virtual void OnClonedMember(IMetadataMember original, IMetadataMember cloned) { }
        /// <inheritdoc/>
        public virtual void OnClonedEvent(EventDefinition original, EventDefinition cloned) { }
        /// <inheritdoc/>
        public virtual void OnClonedField(FieldDefinition original, FieldDefinition cloned) { }
        /// <inheritdoc/>
        public virtual void OnClonedMethod(MethodDefinition original, MethodDefinition cloned) { }
        /// <inheritdoc/>
        public virtual void OnClonedProperty(PropertyDefinition original, PropertyDefinition cloned) { }
        /// <inheritdoc/>
        public virtual void OnClonedType(TypeDefinition original, TypeDefinition cloned) { }
    }
}
