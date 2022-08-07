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

        /// <inheritdoc/>
        public override void OnClonedMember(IMetadataMember original, IMetadataMember cloned) =>
            _callback(original, cloned);
    }
}
