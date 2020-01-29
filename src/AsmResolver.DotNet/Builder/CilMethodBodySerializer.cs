using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;

namespace AsmResolver.DotNet.Builder
{
    public class CilMethodBodySerializer : IMethodBodySerializer
    {
        public ISegmentReference SerializeMethodBody(DotNetDirectoryBuffer buffer, MethodDefinition method)
        {
            if (method.CilMethodBody == null)
                return null;

            var body = method.CilMethodBody;
            
            body.Instructions.CalculateOffsets();
            if (body.ComputeMaxStackOnBuild)
                body.MaxStack = body.ComputeMaxStack();
            
            var code = BuildRawCodeStream(buffer, body);

            var rawBody = body.IsFat ? BuildFatMethodBody(buffer, body, code) : BuildTinyMethodBody(code);

            return new SegmentReference(rawBody);
        }

        private static CilRawMethodBody BuildTinyMethodBody(byte[] code)
        {
            return new CilRawTinyMethodBody(code);
        }

        private CilRawMethodBody BuildFatMethodBody(DotNetDirectoryBuffer buffer, CilMethodBody body, byte[] code)
        {
            var localVarSig = new LocalVariablesSignature(body.LocalVariables.Select(v => v.VariableType));
            var standAloneSig = new StandAloneSignature(localVarSig);
            var token = buffer.AddStandAloneSignature(standAloneSig);

            var fatBody = new CilRawFatMethodBody(CilMethodBodyAttributes.Fat, (ushort) body.MaxStack, token, code);

            if (body.ExceptionHandlers.Count > 0)
            {
                fatBody.HasSections = true;
                bool needsFatFormat = body.ExceptionHandlers.Any(e => e.IsFat);

                var attributes = CilExtraSectionAttributes.EHTable;
                if (needsFatFormat)
                    attributes |= CilExtraSectionAttributes.FatFormat;

                var rawData = SerializeExceptionHandlers(buffer, body.ExceptionHandlers, needsFatFormat);

                fatBody.ExtraSections.Add(new CilExtraSection(attributes, rawData));
            }

            return fatBody;
        }

        private static byte[] BuildRawCodeStream(DotNetDirectoryBuffer buffer, CilMethodBody body)
        {
            using var codeStream = new MemoryStream();
            var writer = new BinaryStreamWriter(codeStream);
            var assembler = new CilAssembler(writer, new CilOperandBuilder(buffer));
            assembler.WriteInstructions(body.Instructions);
            return codeStream.ToArray();
        }

        private byte[] SerializeExceptionHandlers(DotNetDirectoryBuffer buffer, IList<CilExceptionHandler> exceptionHandlers, bool needsFatFormat)
        {
            using var sectionStream = new MemoryStream();
            var writer = new BinaryStreamWriter(sectionStream);

            for (int i = 0; i < exceptionHandlers.Count; i++)
            {
                var handler = exceptionHandlers[i];
                WriteExceptionHandler(buffer, writer, handler, needsFatFormat);
            }

            return sectionStream.ToArray();
        }

        private void WriteExceptionHandler(DotNetDirectoryBuffer buffer, IBinaryStreamWriter writer, CilExceptionHandler handler, bool useFatFormat)
        {
            if (handler.IsFat && !useFatFormat)
                throw new InvalidOperationException("Can only serialize fat exception handlers in fat format.");
            
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

            switch (handler.HandlerType)
            {
                case CilExceptionHandlerType.Exception:
                {
                    writer.WriteUInt32(handler.ExceptionType switch
                    {
                        TypeDefinition typeDef => buffer.GetTypeDefinitionToken(typeDef).ToUInt32(),
                        TypeReference typeRef => buffer.AddTypeReference(typeRef).ToUInt32(),
                        TypeSpecification typeSpec => buffer.AddTypeSpecification(typeSpec).ToUInt32(),
                        _ => throw new ArgumentOutOfRangeException()
                    });
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
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}