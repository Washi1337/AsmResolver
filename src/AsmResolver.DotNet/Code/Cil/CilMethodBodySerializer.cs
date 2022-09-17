using System;
using System.Collections.Generic;
using System.IO;
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
        /// When this property is set to <c>true</c>, the maximum stack depth of all method bodies will be recaculated.
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
        }

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
            if (method.CilMethodBody == null)
                return SegmentReference.Null;

            var body = method.CilMethodBody;

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
                token = context.TokenProvider.GetStandAloneSignatureToken(standAloneSig);
            }

            var fatBody = new CilRawFatMethodBody(CilMethodBodyAttributes.Fat, (ushort) body.MaxStack, token, new DataSegment(code));
            fatBody.InitLocals = body.InitializeLocals;

            // Build up EH table section.
            if (body.ExceptionHandlers.Count > 0)
            {
                fatBody.HasSections = true;
                bool needsFatFormat = body.ExceptionHandlers.Any(e => e.IsFat);

                var attributes = CilExtraSectionAttributes.EHTable;
                if (needsFatFormat)
                    attributes |= CilExtraSectionAttributes.FatFormat;

                var rawSectionData = SerializeExceptionHandlers(context, body.ExceptionHandlers, needsFatFormat);
                var section = new CilExtraSection(attributes, rawSectionData);
                fatBody.ExtraSections.Add(section);
            }

            return fatBody;
        }

        private byte[] BuildRawCodeStream(MethodBodySerializationContext context, CilMethodBody body)
        {
            var bag = context.ErrorListener;

            using var rentedWriter = _writerPool.Rent();
            var assembler = new CilAssembler(
                rentedWriter.Writer,
                new CilOperandBuilder(context.TokenProvider, bag),
                body.Owner.SafeToString,
                bag);

            assembler.WriteInstructions(body.Instructions);

            return rentedWriter.GetData();
        }

        private byte[] SerializeExceptionHandlers(MethodBodySerializationContext context, IList<CilExceptionHandler> exceptionHandlers, bool needsFatFormat)
        {
            using var rentedWriter = _writerPool.Rent();

            for (int i = 0; i < exceptionHandlers.Count; i++)
            {
                var handler = exceptionHandlers[i];
                WriteExceptionHandler(context, rentedWriter.Writer, handler, needsFatFormat);
            }

            return rentedWriter.GetData();
        }

        private void WriteExceptionHandler(MethodBodySerializationContext context, IBinaryStreamWriter writer, CilExceptionHandler handler, bool useFatFormat)
        {
            if (handler.IsFat && !useFatFormat)
                throw new InvalidOperationException("Can only serialize fat exception handlers in fat format.");

            uint tryStart = (uint) (handler.TryStart?.Offset ?? 0);
            uint tryEnd= (uint) (handler.TryEnd?.Offset ?? 0);
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
                        TypeReference typeReference => provider.GetTypeReferenceToken(typeReference),
                        TypeDefinition typeDefinition => provider.GetTypeDefinitionToken(typeDefinition),
                        TypeSpecification typeSpecification => provider.GetTypeSpecificationToken(typeSpecification),
                        _ => context.ErrorListener.RegisterExceptionAndReturnDefault<MetadataToken>(
                            new ArgumentOutOfRangeException(
                                $"Invalid or unsupported exception type ({handler.ExceptionType.SafeToString()})"))
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
