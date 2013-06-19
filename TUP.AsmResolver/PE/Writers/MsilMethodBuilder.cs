using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUP.AsmResolver.NET;
using TUP.AsmResolver.NET.Specialized;
using TUP.AsmResolver.NET.Specialized.MSIL;

namespace TUP.AsmResolver.PE.Writers
{
    public class MsilMethodBuilder : RebuildingTask 
    {
        public MsilMethodBuilder(PEConstructor constructor)
            : base(constructor)
        {
        }

        public override void RunProcedure(Workspace workspace)
        {
            foreach (TypeDefinition typeDef in workspace.GetMembers<TypeDefinition>(MetaDataTableType.TypeDef))
                ReconstructMethodBodies(workspace, typeDef);
        }

        private void ReconstructMethodBodies(Workspace workspace, TypeDefinition declaringType)
        {
            if (declaringType.HasMethods)
            {
                foreach (MethodDefinition methodDef in declaringType.Methods)
                {
                    if (methodDef.HasBody)
                    {
                        MethodBody methodBody = methodDef.Body;
                        byte[] serializedBody = SerializeMethodBody(workspace, methodBody);

                        MethodBodyInfo info = new MethodBodyInfo()
                        {
                            Bytes = serializedBody,
                        };

                        workspace.MethodBodyTable.AppendMethodBody(info);
                    }
                }
            }
        }
        
        private byte[] SerializeMethodBody(Workspace workspace, MethodBody methodBody)
        {
            byte[] bytes = null;
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    AppendHeader(methodBody, writer);
                    AppendCode(workspace, methodBody, writer);

                    ASMGlobals.Align(writer, 4);

                    if (methodBody.HasExtraSections)
                        foreach (MethodBodySection section in methodBody.ExtraSections)
                            AppendSection(section, writer);
                }
                bytes = stream.ToArray();
            }
            return bytes;
        }

        private void AppendHeader(MethodBody methodBody, BinaryWriter writer)
        {
            MSILInstruction lastInstruction = methodBody.Instructions[methodBody.Instructions.Length - 1];
            int codeSize = lastInstruction.Offset + lastInstruction.Size;
            
            bool isFat = codeSize > 64 || methodBody.HasExtraSections || methodBody.HasVariables || methodBody.MaxStack > 8;

            if (isFat)
            {
                ushort signature = 0x0003;
                signature |= (byte)(methodBody.HasExtraSections ? 0x08 : 0x00);
                signature |= (byte)(methodBody.InitLocals ? 0x10 : 0x00);
                signature |= (0x03 << 12);
                writer.Write(signature);
                writer.Write((ushort)methodBody.MaxStack); // must update
                writer.Write(codeSize);
                writer.Write(methodBody.LocalVarSig); // must update
            }
            else
            {
                byte signature = (byte)(0x02 | (codeSize << 2));
                writer.Write(signature);
            }
        }

        private void AppendCode(Workspace workspace, MethodBody methodBody, BinaryWriter writer)
        {
            foreach (MSILInstruction instruction in methodBody.Instructions)
            {
                writer.Write(instruction.OpCode.Bytes);

                // TODO: support all instructions
                if (instruction.Operand is string)
                {
                    uint offset = workspace.GetStream<UserStringsHeap>().GetStringOffset(instruction.Operand as string);
                    offset += 0x70000000;
                    instruction.OperandBytes = BitConverter.GetBytes(offset);
                }
                if (instruction.Operand is MetaDataMember)
                {
                    instruction.OperandBytes = BitConverter.GetBytes((instruction.Operand as MetaDataMember).MetaDataToken);
                }
                
                if (instruction.OperandBytes != null)
                    writer.Write(instruction.OperandBytes);
            }
        }

        private void AppendSection(MethodBodySection section, BinaryWriter writer)
        {
            AppendSectionHeader(section, writer);
            foreach (ExceptionHandler handler in section.ExceptionHandlers)
                AppendExceptionHandler(handler, writer, section.IsFat);
        }

        private void AppendSectionHeader(MethodBodySection section, BinaryWriter writer)
        {
            byte signature = 0x01;

            if (section.IsFat)
                signature |= 0x40;
            if (section.HasMoreSections)
                signature |= 0x80;

            writer.Write(signature);

            if (section.IsFat)
            {
                writer.Write((byte)((section.ExceptionHandlers.Length * 12) + 4));
                writer.Write((ushort)0);
            }
            else
            {
                writer.Write(BitConverter.GetBytes((section.ExceptionHandlers.Length * 24) + 4), 0, 3);
            }
        }

        private void AppendExceptionHandler(ExceptionHandler handler, BinaryWriter writer, bool fatFormat)
        {
            if (fatFormat)
            {
                writer.Write((uint)handler.Type);
                writer.Write(handler.TryStart);
                writer.Write(handler.TryEnd - handler.TryStart);
                writer.Write(handler.HandlerStart);
                writer.Write(handler.HandlerEnd - handler.HandlerStart);

                if (handler.CatchType == null)
                    writer.Write(0);
                else
                    writer.Write(handler.CatchType.MetaDataToken);

                writer.Write(handler.FilterStart);
            }
            else
            {
                writer.Write((ushort)handler.Type);
                writer.Write((ushort)handler.TryStart);
                writer.Write((byte)(handler.TryEnd - handler.TryStart));
                writer.Write((ushort)handler.HandlerStart);
                writer.Write((byte)(handler.HandlerEnd - handler.HandlerStart));

                if (handler.CatchType == null)
                    writer.Write(0);
                else
                    writer.Write(handler.CatchType.MetaDataToken);

                writer.Write(handler.FilterStart);
            }
        }
    }
}
