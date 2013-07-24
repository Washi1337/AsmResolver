using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.NET.Specialized;
using TUP.AsmResolver.NET.Specialized.MSIL;

namespace TUP.AsmResolver.PE.Readers
{
    internal class NETMethodReader
    {
        internal byte signature;
        internal short fatsig;
        internal short maxstack;
        internal uint codesize;
        internal uint localvarsig;
        internal VariableDefinition[] vars;
        internal MethodBodySection[] sections;


        MetaDataTokenResolver tokenResolver;
        PeImage peImage;
        uint rva;
        uint rawoffset;
        MethodBody methodbody;
        internal NETMethodReader(PeImage peImage, MethodBody methodbody)
        {
            tokenResolver = new MetaDataTokenResolver(peImage.ParentAssembly._netHeader);
            this.peImage = peImage;
            this.rva = methodbody.Method.RVA;
            this.methodbody = methodbody;
            peImage.SetOffset(Offset.FromRva(rva, peImage.ParentAssembly).FileOffset);
            rawoffset = (uint)peImage.Stream.Position;
            signature = peImage.Reader.ReadByte();
            
        }

        internal void ReadFatMethod()
        {

            peImage.SetOffset(Offset.FromRva(rva, peImage.ParentAssembly).FileOffset);

            fatsig = BitConverter.ToInt16(peImage.ReadBytes(2),0);
            maxstack = BitConverter.ToInt16(peImage.ReadBytes(2),0);
            codesize = BitConverter.ToUInt32(peImage.ReadBytes(4),0);
            localvarsig = BitConverter.ToUInt32(peImage.ReadBytes(4), 0); 

        }

        internal void ReadVariables()
        {
            if (methodbody.LocalVarSig == 0)
                return;

            var sig = (StandAloneSignature)tokenResolver.ResolveMember(methodbody.LocalVarSig);

            vars = methodbody.Method._netheader.BlobHeap.ReadVariableSignature(sig.Signature, methodbody.Method);

        }

        internal void ReadSections()
        {

            peImage.SetOffset(rawoffset + methodbody.HeaderSize + methodbody.CodeSize);


            List<MethodBodySection> sections = new List<MethodBodySection>();
            MethodBodySection section = ReadSection();
            sections.Add(section);

            while (section.HasMoreSections)
            {
                section = ReadSection();
                sections.Add(section);
            }

            this.sections = sections.ToArray();
        }

        private void Align(int align)
        {
            align--;
            peImage.ReadBytes((((int)peImage.Stream.Position + align) & ~align) - (int)peImage.Stream.Position);
        }

        internal MethodBodySection ReadSection()
        {
            Align(4);
            byte flag = peImage.Reader.ReadByte();
            if ((flag & 0x40) == 0)
                return ReadSmallSection(flag);
            else
                return ReadFatSection(flag); 
            
        }

        internal MethodBodySection ReadSmallSection(byte flag)
        {
            MethodBodySection section = new MethodBodySection(flag);

            int count = peImage.Reader.ReadByte() / 12;
            peImage.ReadBytes(2);
            
            for (int i = 0; i < count; i++)
            {
                ExceptionHandler handler = new ExceptionHandler((ExceptionHandlerType)(BitConverter.ToInt16(peImage.ReadBytes(2), 0) & 7),
                        BitConverter.ToInt16(peImage.ReadBytes(2), 0),
                        peImage.Reader.ReadByte(),
                        BitConverter.ToInt16(peImage.ReadBytes(2), 0),
                        peImage.Reader.ReadByte());

                if (handler.Type == ExceptionHandlerType.Catch)
                    handler.CatchType = (TypeReference)methodbody.Method._netheader.TokenResolver.ResolveMember(BitConverter.ToUInt32(peImage.ReadBytes(4), 0));
                else if (handler.Type == ExceptionHandlerType.Filter)
                    handler.FilterStart = BitConverter.ToInt32(peImage.ReadBytes(4), 0);
                else
                    peImage.ReadBytes(4);
                section._handlers.Add(handler);

            }
            return section;
        }

        internal MethodBodySection ReadFatSection(byte flag)
        {
            MethodBodySection section = new MethodBodySection(flag);
            peImage.SetOffset(peImage.Stream.Position - 1);
            int count = (BitConverter.ToInt32(peImage.ReadBytes(4),0) >> 8) / 0x18;

            for (int i = 0; i < count; i++)
            {
               ExceptionHandler handler = new ExceptionHandler((ExceptionHandlerType)(BitConverter.ToInt32(peImage.ReadBytes(4),0) & 7),
                        BitConverter.ToInt32(peImage.ReadBytes(4), 0),
                        BitConverter.ToInt32(peImage.ReadBytes(4), 0),
                        BitConverter.ToInt32(peImage.ReadBytes(4), 0),
                        BitConverter.ToInt32(peImage.ReadBytes(4), 0));

                if (handler.Type == ExceptionHandlerType.Catch)
                    try
                    {
                        handler.CatchType = (TypeReference)methodbody.Method._netheader.TokenResolver.ResolveMember(BitConverter.ToUInt32(peImage.ReadBytes(4), 0));
                    }
                    catch { handler.CatchType = methodbody.Method._netheader.TypeSystem.Object; }
                    
                else if (handler.Type == ExceptionHandlerType.Filter)
                    handler.FilterStart = BitConverter.ToInt32(peImage.ReadBytes(4), 0);
                else
                    peImage.ReadBytes(4);
                section._handlers.Add(handler);
            }
            return section;
        }

        
    }
}
