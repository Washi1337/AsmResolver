namespace AsmResolver.DotNet.Code
{
    /// <summary>
    /// Provides members for serializing a method body defined in a .NET module to a file segment. 
    /// </summary>
    public interface IMethodBodySerializer
    {
        /// <summary>
        /// Serializes the body of the provided method definition into a segment that can be added to a PE image.  
        /// </summary>
        /// <param name="context">The context in which the serializer is situated in.</param>
        /// <param name="method">The method to serialize the method body for.</param>
        /// <returns>A reference to a segment that encodes the method body.</returns>
        ISegmentReference SerializeMethodBody(MethodBodySerializationContext context, MethodDefinition method);
    }
}