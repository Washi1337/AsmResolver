namespace AsmResolver.DotNet.Code
{
    /// <summary>
    /// Represents a method body that was not resolved into
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
