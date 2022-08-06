using System;

namespace AsmResolver.DotNet.Cloning
{
    /// <inheritdoc/>
    public class CallbackCloneListener : IMemberClonerListener
    {

        private readonly Action<IMetadataMember, IMetadataMember> _callback;

        /// <summary>
        /// Creates a new instance of the <see cref="CallbackCloneListener"/> class.
        /// </summary>
        /// <param name="callback">The Callback used.</param>
        public CallbackCloneListener(Action<IMetadataMember, IMetadataMember> callback) =>
            _callback = callback;

        /// <inheritdoc/>
        public void OnClonedMember(IMetadataMember original, IMetadataMember cloned) =>
            _callback(original, cloned);
    }
}
