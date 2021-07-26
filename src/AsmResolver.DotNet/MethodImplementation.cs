namespace AsmResolver.DotNet
{
    /// <summary>
    /// Defines an explicit implementation of a method defined by an interface.
    /// </summary>
    public readonly struct MethodImplementation
    {
        /// <summary>
        /// Creates a new explicit implementation of a method.
        /// </summary>
        /// <param name="declaration">The interface method that is implemented.</param>
        /// <param name="body">The method implementing the base method.</param>
        public MethodImplementation(IMethodDefOrRef? declaration, IMethodDefOrRef? body)
        {
            Declaration = declaration;
            Body = body;
        }

        /// <summary>
        /// Gets the interface method that is implemented.
        /// </summary>
        public IMethodDefOrRef? Declaration
        {
            get;
        }

        /// <summary>
        /// Gets the method that implements the base method.
        /// </summary>
        public IMethodDefOrRef? Body
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString() =>
            $".override {Declaration} with {Body}";
    }
}
