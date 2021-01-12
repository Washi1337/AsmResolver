using System;
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
        public MethodBody ReadMethodBody(ModuleReadContext context, MethodDefinition owner, in MethodDefinitionRow row)
        {
            try
            {
                if (row.Body.CanRead)
                {
                    if (owner.IsIL)
                    {
                        var rawBody = CilRawMethodBody.FromReader(context, row.Body.CreateReader());
                        return CilMethodBody.FromRawMethodBody(context, owner, rawBody);
                    }
                    else
                    {
                        context.NotSupported($"Body of method {owner.MetadataToken} is native and unbounded which is not supported.");
                        // TODO: handle native method bodies.
                    }
                }
                else if (row.Body.IsBounded && row.Body.GetSegment() is CilRawMethodBody rawMethodBody)
                {
                    return CilMethodBody.FromRawMethodBody(context, owner, rawMethodBody);
                }
            }
            catch (Exception ex)
            {
                context.RegisterException(new BadImageFormatException($"Failed to parse the method body of {owner.MetadataToken}.", ex));
            }

            return null;
        }
    }
}