using AsmResolver.DotNet.Code;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Provides a default implementation of a <see cref="IMethodBodyReader"/>, which reads CIL method bodies using the
    /// <see cref="CilRawMethodBody"/> class.
    /// </summary>
    public class DefaultMethodBodyReader : IMethodBodyReader
    {
        /// <inheritdoc />
        public MethodBody ReadMethodBody(MethodDefinition owner, MethodDefinitionRow row)
        {
            if (row.Body.CanRead)
            {
                if (owner.IsIL)
                {
                    var rawBody = CilRawMethodBody.FromReader(row.Body.CreateReader());
                    return CilMethodBody.FromRawMethodBody(owner, rawBody);
                }
                else
                {
                    // TODO: handle native method bodies.
                }
            }

            return null;
        }
    }
}