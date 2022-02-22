using System;
using AsmResolver.DotNet.Code;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
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
        public virtual MethodBody? ReadMethodBody(ModuleReaderContext context, MethodDefinition owner, in MethodDefinitionRow row)
        {
            var bodyReference = row.Body;
            if (bodyReference == SegmentReference.Null)
                return null;

            MethodBody? result = null;

            try
            {
                if (bodyReference.IsBounded)
                {
                    if (bodyReference.GetSegment() is CilRawMethodBody rawMethodBody)
                        result = CilMethodBody.FromRawMethodBody(context, owner, rawMethodBody);
                }
                else if (bodyReference.CanRead && owner.IsIL)
                {
                    var reader = bodyReference.CreateReader();
                    var rawBody = CilRawMethodBody.FromReader(context, ref reader);
                    if (rawBody is not null)
                        result = CilMethodBody.FromRawMethodBody(context, owner, rawBody);
                }
            }
            catch (Exception ex)
            {
                context.RegisterException(new BadImageFormatException($"Failed to parse the method body of {owner.MetadataToken}.", ex));
            }

            if (result is not null)
            {
                result.Address = bodyReference;
                return result;
            }

            return new UnresolvedMethodBody(owner, bodyReference);
        }
    }
}
