namespace AsmResolver.DotNet.Cloning
{
    /// <summary>
    /// <see cref="MemberCloner"/> Callback Listener.
    /// </summary>
    public interface IMemberClonerListener
    {
        /// <summary>
        /// This function is called for every member got cloned.
        /// </summary>
        /// <param name="original">original member.</param>
        /// <param name="cloned">cloned member.</param>
        public void OnClonedMember(IMetadataMember original, IMetadataMember cloned);
    }
}
