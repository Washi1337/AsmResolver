using System;

namespace AsmResolver.DotNet.Cloning
{
    /// <summary>
    /// This implementation that calls the <see cref="MemberClonerListener.OnClonedMember"/> to a callback action.
    /// </summary>
    public class CallbackClonerListener : MemberClonerListener
    {
        private readonly Action<IMemberDefinition, IMemberDefinition> _callback;

        /// <summary>
        /// Creates a new instance of the <see cref="CallbackClonerListener"/> class.
        /// </summary>
        /// <param name="callback">The Callback used.</param>
        public CallbackClonerListener(Action<IMemberDefinition, IMemberDefinition> callback) =>
            _callback = callback;

        /// <summary>
        /// Gets a singleton instance of the <see cref="CallbackClonerListener"/> class that performs no operation
        /// on any of the cloning procedure notifications.
        /// </summary>
        public static CallbackClonerListener EmptyInstance
        {
            get;
        } = new((_, _) => { });

        /// <inheritdoc/>
        public override void OnClonedMember(IMemberDefinition original, IMemberDefinition cloned) =>
            _callback(original, cloned);

    }
}
