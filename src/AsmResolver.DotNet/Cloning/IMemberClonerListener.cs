namespace AsmResolver.DotNet.Cloning
{
    /// <summary>
    /// <see cref="MemberCloner"/> Callback listener that receives calls after cloning process.
    /// </summary>
    public interface IMemberClonerListener
    {
        /// <summary>
        /// This function is called for every member that got cloned.
        /// </summary>
        /// <param name="original">The original member.</param>
        /// <param name="cloned">The cloned member.</param>
        public void OnClonedMember(IMetadataMember original, IMetadataMember cloned);
        /// <summary>
        /// This function is called for every type that got cloned.
        /// </summary>
        /// <param name="original">The original type.</param>
        /// <param name="cloned">The cloned type.</param>
        public void OnClonedType(TypeDefinition original, TypeDefinition cloned);
        /// <summary>
        /// This function is called for every method that got cloned.
        /// </summary>
        /// <param name="original">The original method.</param>
        /// <param name="cloned">The cloned method.</param>
        public void OnClonedMethod(MethodDefinition original, MethodDefinition cloned);
        /// <summary>
        /// This function is called for every field that got cloned.
        /// </summary>
        /// <param name="original">The original field.</param>
        /// <param name="cloned">The cloned field.</param>
        public void OnClonedField(FieldDefinition original, FieldDefinition cloned);
        /// <summary>
        /// This function is called for every property that got cloned.
        /// </summary>
        /// <param name="original">The original property.</param>
        /// <param name="cloned">The cloned property.</param>
        public void OnClonedProperty(PropertyDefinition original, PropertyDefinition cloned);
        /// <summary>
        /// This function is called for every event that got cloned.
        /// </summary>
        /// <param name="original">The original event.</param>
        /// <param name="cloned">The cloned event.</param>
        public void OnClonedEvent(EventDefinition original, EventDefinition cloned);
    }
}
