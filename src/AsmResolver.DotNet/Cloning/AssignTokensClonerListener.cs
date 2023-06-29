namespace AsmResolver.DotNet.Cloning
{
    /// <summary>
    /// Provides an implementation of a <see cref="IMemberClonerListener"/> that pre-emptively assigns new metadata
    /// tokens to the cloned metadata members using the target module's <see cref="TokenAllocator"/>.
    /// </summary>
    public class AssignTokensClonerListener : MemberClonerListener
    {
        /// <summary>
        /// Creates a new instance of the token allocator listener.
        /// </summary>
        /// <param name="targetModule">The module that will contain the cloned members.</param>
        public AssignTokensClonerListener(ModuleDefinition targetModule)
        {
            TargetModule = targetModule;
        }

        /// <summary>
        /// Gets the module that will contain the cloned members.
        /// </summary>
        public ModuleDefinition TargetModule
        {
            get;
        }

        /// <inheritdoc />
        public override void OnClonedMember(IMemberDefinition original, IMemberDefinition cloned)
        {
            TargetModule.TokenAllocator.AssignNextAvailableToken((MetadataMember) cloned);
            base.OnClonedMember(original, cloned);
        }
    }
}
