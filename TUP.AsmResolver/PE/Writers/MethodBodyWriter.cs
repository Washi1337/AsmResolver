using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.NET.Specialized.MSIL;

namespace TUP.AsmResolver.PE.Writers
{
    internal class MethodBodyWriter : IWriterTask
    {
        internal MethodBodyWriter(PEWriter writer)
        {
            Writer = writer;
        }

        OffsetConverter offsetConverter;

        public PEWriter Writer
        {
            get;
            private set;
        }

        public void RunProcedure()
        {
            offsetConverter = new OffsetConverter(Section.GetSectionByRva(Writer.OriginalAssembly, Writer.OriginalAssembly.NTHeader.OptionalHeader.Entrypoint.Rva));
        }

        private void WriteMethodBody(MethodBody methodBody)
        {
            Writer.MoveToOffset(offsetConverter.RvaToFileOffset(methodBody.Method.RVA));

            if (methodBody.IsFat)
            {
                WriteFatHeader(methodBody);
            }
            else
            {
                WriteSmallHeader(methodBody);
            }

            WriteCode(methodBody);


        }

        private void WriteSmallHeader(MethodBody methodBody)
        {
            byte signature = (byte)(methodBody.CodeSize << 2 | 2);
            Writer.BinWriter.Write(signature);
        }

        private void WriteFatHeader(MethodBody methodBody)
        {

            ushort signature = (ushort)((methodBody.InitLocals ? 1 << 4 : 0) | (methodBody.HasExtraSections ? 1 << 2 : 0) | 2);
            Writer.BinWriter.Write(signature);
            Writer.BinWriter.Write((ushort)methodBody.MaxStack);
            Writer.BinWriter.Write(methodBody.CodeSize);
            Writer.BinWriter.Write(methodBody.LocalVarSig);

        }

        private void WriteCode(MethodBody methodBody)
        {

        }

    }
}
