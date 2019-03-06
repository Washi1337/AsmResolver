namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents a signature that contains a single embedded type signature.
    /// </summary>
    public interface IHasTypeSignature
    {
        TypeSignature TypeSignature
        {
            get;
        }
    }
}
