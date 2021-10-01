using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Provides an implementation of the <see cref="ICilOperandResolver"/> that always returns <c>null</c>.
    /// </summary>
    public sealed class EmptyOperandResolver : ICilOperandResolver
    {
        private EmptyOperandResolver()
        {
        }

        /// <summary>
        /// Gets the singleton instance of the empty operand resolver.
        /// </summary>
        public static EmptyOperandResolver Instance
        {
            get;
        } = new();

        /// <inheritdoc />
        public object? ResolveMember(MetadataToken token) => null;

        /// <inheritdoc />
        public object? ResolveString(MetadataToken token) => null;

        /// <inheritdoc />
        public object? ResolveLocalVariable(int index) => null;

        /// <inheritdoc />
        public object? ResolveParameter(int index) => null;
    }
}
