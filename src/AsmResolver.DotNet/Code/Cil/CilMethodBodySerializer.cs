using System;
using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Code.Cil
{
    /// <summary>
    /// Provides a default implementation of the <see cref="IMethodBodySerializer"/> interface, that serializes all
    /// managed CIL method bodies of type <see cref="CilMethodBody"/> to raw method bodies of type <see cref="CilRawMethodBody"/>.
    /// </summary>
    public class CilMethodBodySerializer : IMethodBodySerializer
    {
        private readonly MemoryStreamWriterPool _writerPool = new();

        /// <summary>
        /// Gets or sets the value of an override switch indicating whether the max stack should always be recalculated
        /// or should always be preserved.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When this property is set to <c>true</c>, the maximum stack depth of all method bodies will be recalculated.
        /// </para>
        /// <para>
        /// When this property is set to <c>false</c>, the maximum stack depth of all method bodies will be preserved.
        /// </para>
        /// <para>
        /// When this property is set to <c>null</c>, the maximum stack depth will only be recalculated if
        /// <see cref="CilMethodBody.ComputeMaxStackOnBuild"/> is set to <c>true</c>.
        /// </para>
        /// </remarks>
        public bool? ComputeMaxStackOnBuildOverride
        {
            get;
            set;
        } = null;

        /// <summary>
        /// Gets or sets the value of an override switch indicating whether labels should always be verified for
        /// validity or not.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When this property is set to <c>true</c>, all method bodies will be verified for branch validity.
        /// </para>
        /// <para>
        /// When this property is set to <c>false</c>, no method body will be verified for branch validity.
        /// </para>
        /// <para>
        /// When this property is set to <c>null</c>, a method body will only be verified if
        /// <see cref="CilMethodBody.VerifyLabelsOnBuild"/> is set to <c>true</c>.
        /// </para>
        /// </remarks>
        public bool? VerifyLabelsOnBuildOverride
        {
            get;
            set;
        } = null;

        /// <inheritdoc />
        public ISegmentReference SerializeMethodBody(MethodBodySerializationContext context, MethodDefinition method)
        {
            if (method.CilMethodBody is null)
                return SegmentReference.Null;

            var body = method.CilMethodBody;

            if (body is SerializedCilMethodBody { IsInitialized: false } serializedBody
                && serializedBody.IsFat == serializedBody.OriginalRawBody.IsFat)
            {
                return FastPatchMethodBody(context, serializedBody);
            }
            else
            {
                return BuildMethodBody(context, body);
            }
        }

        private ISegmentReference FastPatchMethodBody(MethodBodySerializationContext context, SerializedCilMethodBody body)
        {
            var operandBuilder = new CilOperandBuilder(context.TokenProvider, context.ErrorListener);
            var tokenRewriter = (MetadataToken token) => token.Table == TableIndex.String
                ? operandBuilder.GetStringToken(body.OperandResolver.ResolveString(token))
                : operandBuilder.GetMemberToken(body.OperandResolver.ResolveMember(token));

            // Serialize code.
            byte[] code = FastRewriteCodeStream(context, body, tokenRewriter);

            // If we're tiny then method just contains code.
            if (!body.IsFat)
                return new CilRawTinyMethodBody(code).ToReference();

            // Serialize locals.
            var localVarSigToken = BuildLocalVariablesSignatures(context, body);

            // Build skeleton for new fat body.
            var result = new CilRawFatMethodBody(CilMethodBodyAttributes.Fat, (ushort) body.MaxStack, localVarSigToken,
                new DataSegment(code))
            {
                InitLocals = body.InitializeLocals
            };

            // Rewrite sections if any.
            if (body.OriginalRawBody is CilRawFatMethodBody { ExtraSections.Count: > 0 })
            {
                result.HasSections = true;
                FastRewriteExtraSections(context, body, result, tokenRewriter);
            }

            return result.ToReference();
        }

        private byte[] FastRewriteCodeStream(
            MethodBodySerializationContext context,
            SerializedCilMethodBody sourceBody,
            Func<MetadataToken, MetadataToken> tokenRewriter)
        {
            try
            {
                var codeReader = sourceBody.OriginalRawBody.Code.CreateReader();

                using var rentedWriter = _writerPool.Rent();
                FastCilReassembler.RewriteCode(
                    ref codeReader,
                    rentedWriter.Writer,
                    tokenRewriter
                );

                return rentedWriter.GetData();
            }
            catch (Exception ex)
            {
                context.ErrorListener.RegisterException(new BadImageFormatException(
                    $"Method body of {sourceBody.OriginalOwner.SafeToString()} contains invalid extra sections.", ex));
                return sourceBody.OriginalRawBody.Code.ToArray();
            }
        }

        private void FastRewriteExtraSections(
            MethodBodySerializationContext context,
            SerializedCilMethodBody sourceBody,
            CilRawFatMethodBody destinationBody,
            Func<MetadataToken, MetadataToken> tokenRewriter)
        {
            var raw = (CilRawFatMethodBody) sourceBody.OriginalRawBody;
            foreach (var section in raw.ExtraSections)
            {
                byte[] sectionData = section.Data;

                if (section.IsEHTable)
                {
                    try
                    {
                        var reader = new BinaryStreamReader(sectionData);
                        using var rentedWriter = _writerPool.Rent();
                        FastCilReassembler.RewriteExceptionHandlerSection(
                            ref reader,
                            rentedWriter.Writer,
                            tokenRewriter,
                            section.IsFat
                        );

                        sectionData = rentedWriter.GetData();
                    }
                    catch (Exception ex)
                    {
                        context.ErrorListener.RegisterException(new BadImageFormatException(
                            $"Method body of {sourceBody.OriginalOwner.SafeToString()} contains an invalid CIL code stream.", ex));
                    }
                }

                destinationBody.ExtraSections.Add(new CilExtraSection(section.Attributes, sectionData));
            }
        }

        private ISegmentReference BuildMethodBody(MethodBodySerializationContext context, CilMethodBody body)
        {
            // Serialize CIL stream.
            byte[] code = BuildRawCodeStream(context, body);

            // Build method body.
            var rawBody = body.IsFat
                ? BuildFatMethodBody(context, body, code)
                : BuildTinyMethodBody(code);

            return rawBody.ToReference();
        }

        private static CilRawMethodBody BuildTinyMethodBody(byte[] code) => new CilRawTinyMethodBody(code);

        private CilRawMethodBody BuildFatMethodBody(MethodBodySerializationContext context, CilMethodBody body, byte[] code)
        {
            // Serialize local variables.
            var localVarSigToken = BuildLocalVariablesSignatures(context, body);
            var fatBody = new CilRawFatMethodBody(CilMethodBodyAttributes.Fat, (ushort) body.MaxStack, localVarSigToken, new DataSegment(code))
            {
                InitLocals = body.InitializeLocals
            };

            // Build up EH table section.
            if (body.ExceptionHandlers.Count > 0)
            {
                fatBody.HasSections = true;
                bool needsFatFormat = body.ExceptionHandlers.Any(e => e.IsFat);

                var attributes = CilExtraSectionAttributes.EHTable;
                if (needsFatFormat)
                    attributes |= CilExtraSectionAttributes.FatFormat;

                byte[] rawSectionData = SerializeExceptionHandlers(context, body, needsFatFormat);
                var section = new CilExtraSection(attributes, rawSectionData);
                fatBody.ExtraSections.Add(section);
            }

            return fatBody;
        }

        private static MetadataToken BuildLocalVariablesSignatures(MethodBodySerializationContext context, CilMethodBody body)
        {
            MetadataToken token;
            if (body.LocalVariables.Count == 0)
            {
                token = MetadataToken.Zero;
            }
            else
            {
                var localVarSig = new LocalVariablesSignature();
                for (int i = 0; i < body.LocalVariables.Count; i++)
                    localVarSig.VariableTypes.Add(body.LocalVariables[i].VariableType);

                var standAloneSig = new StandAloneSignature(localVarSig);
                token = context.TokenProvider.GetStandAloneSignatureToken(standAloneSig, body.Owner);
            }

            return token;
        }

        private byte[] BuildRawCodeStream(MethodBodySerializationContext context, CilMethodBody body)
        {
            body.Instructions.CalculateOffsets();

            try
            {
                if (ComputeMaxStackOnBuildOverride ?? body.ComputeMaxStackOnBuild)
                {
                    // Max stack computation requires branches to be correct.
                    body.VerifyLabels(false);
                    body.MaxStack = body.ComputeMaxStack(false);
                }
                else if (VerifyLabelsOnBuildOverride ?? body.VerifyLabelsOnBuild)
                {
                    body.VerifyLabels(false);
                }
            }
            catch (Exception ex)
            {
                context.ErrorListener.RegisterException(ex);
            }

            var bag = context.ErrorListener;

            using var rentedWriter = _writerPool.Rent();
            var assembler = new CilAssembler(
                rentedWriter.Writer,
                new CilOperandBuilder(context.TokenProvider, bag, body.Owner),
                body.Owner.SafeToString,
                bag
            );

            assembler.WriteInstructions(body.Instructions);

            return rentedWriter.GetData();
        }

        private byte[] SerializeExceptionHandlers(MethodBodySerializationContext context, CilMethodBody body, bool needsFatFormat)
        {
            var handlers = body.ExceptionHandlers;

            using var rentedWriter = _writerPool.Rent();

            for (int i = 0; i < handlers.Count; i++)
                WriteExceptionHandler(context, rentedWriter.Writer, body, handlers[i], needsFatFormat);

            return rentedWriter.GetData();
        }

        private static void WriteExceptionHandler(
            MethodBodySerializationContext context,
            BinaryStreamWriter writer,
            CilMethodBody body,
            CilExceptionHandler handler,
            bool useFatFormat)
        {
            if (handler.IsFat && !useFatFormat)
                throw new InvalidOperationException("Can only serialize fat exception handlers in fat format.");

            uint tryStart = (uint) (handler.TryStart?.Offset ?? 0);
            uint tryEnd = (uint) (handler.TryEnd?.Offset ?? 0);
            uint handlerStart = (uint) (handler.HandlerStart?.Offset ?? 0);
            uint handlerEnd = (uint) (handler.HandlerEnd?.Offset ?? 0);

            // Write handler type and boundaries.
            if (useFatFormat)
            {
                writer.WriteUInt32((uint) handler.HandlerType);
                writer.WriteUInt32(tryStart);
                writer.WriteUInt32(tryEnd - tryStart);
                writer.WriteUInt32(handlerStart);
                writer.WriteUInt32(handlerEnd - handlerStart);
            }
            else
            {
                writer.WriteUInt16((ushort)handler. HandlerType);
                writer.WriteUInt16((ushort) tryStart);
                writer.WriteByte((byte) (tryEnd - tryStart));
                writer.WriteUInt16((ushort) handlerStart);
                writer.WriteByte((byte) (handlerEnd - handlerStart));
            }

            // Write handler type or filter start.
            switch (handler.HandlerType)
            {
                case CilExceptionHandlerType.Exception:
                {
                    var provider = context.TokenProvider;

                    var token = handler.ExceptionType switch
                    {
                        TypeReference typeReference => provider.GetTypeReferenceToken(typeReference, body.Owner),
                        TypeDefinition typeDefinition => provider.GetTypeDefinitionToken(typeDefinition, body.Owner),
                        TypeSpecification typeSpecification => provider.GetTypeSpecificationToken(typeSpecification, body.Owner),
                        _ => context.ErrorListener.RegisterExceptionAndReturnDefault<MetadataToken>(
                            new ArgumentOutOfRangeException($"Invalid or unsupported exception type ({handler.ExceptionType.SafeToString()})")
                        )
                    };
                    writer.WriteUInt32(token.ToUInt32());
                    break;
                }

                case CilExceptionHandlerType.Filter:
                    writer.WriteUInt32((uint) (handler.FilterStart?.Offset ?? 0));
                    break;

                case CilExceptionHandlerType.Finally:
                case CilExceptionHandlerType.Fault:
                    writer.WriteUInt32(0);
                    break;

                default:
                    context.ErrorListener.RegisterException(new ArgumentOutOfRangeException(
                            $"Invalid or unsupported handler type ({handler.HandlerType.SafeToString()}"));
                    break;
            }
        }
    }
}
