namespace AsmResolver.DotNet.Code
{
    /// <summary>
    /// Provides a wrapper around a <see cref="ISegmentReference"/>, pointing to the beginning of a method body.
    /// The interpretation of the data behind the pointer was left to the user.
    /// </summary>
    public class UnresolvedMethodBody : MethodBody
    {
        /// <summary>
        /// Creates a new unresolved method body stub.
        /// </summary>
        /// <param name="owner">The owner of the method body.</param>
        /// <param name="address">The reference to the start of the method body.</param>
        public UnresolvedMethodBody(MethodDefinition owner, ISegmentReference address)
            : base(owner)
        {
            Address = address;
        }
    }
}
