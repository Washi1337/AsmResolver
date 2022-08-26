using System;

namespace AsmResolver.DotNet.Cloning
{
    /// <summary>
    /// This implementation that calls the <see cref="MemberClonerListener.OnClonedMember"/> to a callback action.
    /// </summary>
    public class CallbackCloneListener : MemberClonerListener
    {
        private readonly Action<IMetadataMember, IMetadataMember> _callback;

        /// <summary>
        /// Creates a new instance of the <see cref="CallbackCloneListener"/> class.
        /// </summary>
        /// <param name="callback">The Callback used.</param>
        public CallbackCloneListener(Action<IMetadataMember, IMetadataMember> callback) =>
            _callback = callback;

        /// <summary>
        /// Gets a singleton instance of the <see cref="CallbackCloneListener"/> class that performs no operation
        /// on any of the cloning procedure notifications.
        /// </summary>
        public static CallbackCloneListener EmptyInstance
        {
            get;
        } = new((_, _) => { });

        /// <inheritdoc/>
        public override void OnClonedMember(IMetadataMember original, IMetadataMember cloned) =>
            _callback(original, cloned);

    }
}
