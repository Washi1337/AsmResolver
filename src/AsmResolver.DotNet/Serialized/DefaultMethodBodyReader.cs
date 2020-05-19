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
        /// <summary>
        /// Gets or sets a value indicating whether invalid method bodies should be ignored and <c>null</c> should be
        /// returned instead.
        /// </summary>
        public bool ThrowOnInvalidMethodBody
        {
            get;
            set;
        } = true;
        
        /// <inheritdoc />
        public MethodBody ReadMethodBody(MethodDefinition owner, MethodDefinitionRow row)
        {
            try
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
                else if (row.Body.IsBounded && row.Body.GetSegment() is CilRawMethodBody rawMethodBody)
                {
                    return CilMethodBody.FromRawMethodBody(owner, rawMethodBody);
                }
            }
            catch when (!ThrowOnInvalidMethodBody)
            {
                return null;
            }

            return null;
        }
    }
}