using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.DotNet.Signatures;
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
        } = null;
        
        /// <inheritdoc />
        public ISegmentReference SerializeMethodBody(MethodBodySerializationContext context, MethodDefinition method)
        {
            if (method.CilMethodBody == null)
                return SegmentReference.Null;

            var body = method.CilMethodBody;
            
            // Compute max stack when specified, otherwise just calculate offsets only.
            if (ComputeMaxStackOnBuildOverride ?? body.ComputeMaxStackOnBuild)
            {
                try
                {
                    body.MaxStack = body.ComputeMaxStack();
                }
                catch (Exception ex)
                {
                    context.DiagnosticBag.RegisterException(ex);
                    body.Instructions.CalculateOffsets();
                }
            }
            else
            {
                body.Instructions.CalculateOffsets();
            }

            // Serialize CIL stream.
            var code = BuildRawCodeStream(context, body);
            
            // Build method body.
            var rawBody = body.IsFat 
                ? BuildFatMethodBody(context, body, code) 
                : BuildTinyMethodBody(code);

            return new SegmentReference(rawBody);
        }

        private static CilRawMethodBody BuildTinyMethodBody(byte[] code) => 
            new CilRawTinyMethodBody(code);

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
                var localVarSig = new LocalVariablesSignature(body.LocalVariables.Select(v => v.VariableType));
                var standAloneSig = new StandAloneSignature(localVarSig);
                token = context.TokenProvider.GetStandAloneSignatureToken(standAloneSig);
            }

            var fatBody = new CilRawFatMethodBody(CilMethodBodyAttributes.Fat, (ushort) body.MaxStack, token, code);
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

        private static byte[] BuildRawCodeStream(MethodBodySerializationContext context, CilMethodBody body)
        {
            using var codeStream = new MemoryStream();
            
            var writer = new BinaryStreamWriter(codeStream);
            var assembler = new CilAssembler(writer, new CilOperandBuilder(context.TokenProvider, context.DiagnosticBag));
            assembler.WriteInstructions(body.Instructions);
            
            return codeStream.ToArray();
        }

        private byte[] SerializeExceptionHandlers(MethodBodySerializationContext context, IList<CilExceptionHandler> exceptionHandlers, bool needsFatFormat)
        {
            using var sectionStream = new MemoryStream();
            var writer = new BinaryStreamWriter(sectionStream);

            for (int i = 0; i < exceptionHandlers.Count; i++)
            {
                var handler = exceptionHandlers[i];
                WriteExceptionHandler(context, writer, handler, needsFatFormat);
            }

            return sectionStream.ToArray();
        }

        private void WriteExceptionHandler(MethodBodySerializationContext context, IBinaryStreamWriter writer, CilExceptionHandler handler, bool useFatFormat)
        {
            if (handler.IsFat && !useFatFormat)
                throw new InvalidOperationException("Can only serialize fat exception handlers in fat format.");
            
            // Write handler type and boundaries.
            if (useFatFormat)
            {
                writer.WriteUInt32((uint) handler.HandlerType);
                writer.WriteUInt32((uint) handler.TryStart.Offset);
                writer.WriteUInt32((uint) (handler.TryEnd.Offset - handler.TryStart.Offset));
                writer.WriteUInt32((uint) handler.HandlerStart.Offset);
                writer.WriteUInt32((uint) (handler.HandlerEnd.Offset - handler.HandlerStart.Offset));
            }
            else
            {
                writer.WriteUInt16((ushort)handler. HandlerType);
                writer.WriteUInt16((ushort) handler.TryStart.Offset);
                writer.WriteByte((byte) (handler.TryEnd.Offset -handler. TryStart.Offset));
                writer.WriteUInt16((ushort) handler.HandlerStart.Offset);
                writer.WriteByte((byte) (handler.HandlerEnd.Offset - handler.HandlerStart.Offset));
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
                        _ => context.DiagnosticBag.RegisterExceptionAndReturnDefault<MetadataToken>(
                            new ArgumentOutOfRangeException(
                                $"Invalid or unsupported exception type ({handler.ExceptionType.SafeToString()})"))
                    };
                    writer.WriteUInt32(token.ToUInt32());
                    break;
                }

                case CilExceptionHandlerType.Filter:
                    writer.WriteUInt32((uint) handler.FilterStart.Offset);
                    break;
                
                case CilExceptionHandlerType.Finally:
                case CilExceptionHandlerType.Fault:
                    writer.WriteUInt32(0);
                    break;
                
                default:
                    context.DiagnosticBag.RegisterException(new ArgumentOutOfRangeException(
                            $"Invalid or unsupported handler type ({handler.HandlerType.SafeToString()}"));
                    break;
            }
        }
    }
}