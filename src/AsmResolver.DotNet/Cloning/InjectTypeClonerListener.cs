namespace AsmResolver.DotNet.Cloning
{
    /// <summary>
    /// Implements a <see cref="IMemberClonerListener"/> that injects all non-nested types into the target module.
    /// </summary>
    public class InjectTypeClonerListener : MemberClonerListener
    {
        /// <summary>
        /// Creates a new instance of the <see cref="InjectTypeClonerListener"/> type.
        /// </summary>
        /// <param name="targetModule">The target module to inject into.</param>
        public InjectTypeClonerListener(ModuleDefinition targetModule)
        {
            TargetModule = targetModule;
        }

        /// <summary>
        /// Gets the target module to inject the types in.
        /// </summary>
        public ModuleDefinition TargetModule
        {
            get;
        }

        /// <inheritdoc />
        public override void OnClonedType(TypeDefinition original, TypeDefinition cloned)
        {
            if (!original.IsNested)
                TargetModule.TopLevelTypes.Add(cloned);

            base.OnClonedType(original, cloned);
        }
    }
}
