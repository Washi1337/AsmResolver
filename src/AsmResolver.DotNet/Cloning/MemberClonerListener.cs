namespace AsmResolver.DotNet.Cloning
{
    /// <summary>
    /// <see cref="MemberCloner"/> Callback Listener.
    /// </summary>
    public abstract class MemberClonerListener : IMemberClonerListener
    {
        /// <inheritdoc/>
        public abstract void OnClonedMember(IMetadataMember original, IMetadataMember cloned);
        /// <summary>
        /// This function is called for every type got cloned.
        /// </summary>
        /// <param name="original">original type.</param>
        /// <param name="cloned">cloned type.</param>
        public abstract void OnClonedType(TypeDefinition original, TypeDefinition cloned);
        /// <summary>
        /// This function is called for every method got cloned.
        /// </summary>
        /// <param name="original">original method.</param>
        /// <param name="cloned">cloned method.</param>
        public abstract void OnClonedMethod(MethodDefinition original, MethodDefinition cloned);
        /// <summary>
        /// This function is called for every field got cloned.
        /// </summary>
        /// <param name="original">original field.</param>
        /// <param name="cloned">cloned field.</param>
        public abstract void OnClonedField(FieldDefinition original, FieldDefinition cloned);
        /// <summary>
        /// This function is called for every property got cloned.
        /// </summary>
        /// <param name="original">original property.</param>
        /// <param name="cloned">cloned property.</param>
        public abstract void OnClonedProperty(PropertyDefinition original, PropertyDefinition cloned);
        /// <summary>
        /// This function is called for every event got cloned.
        /// </summary>
        /// <param name="original">original event.</param>
        /// <param name="cloned">cloned event.</param>
        public abstract void OnClonedEvent(EventDefinition original, EventDefinition cloned);
    }
}
