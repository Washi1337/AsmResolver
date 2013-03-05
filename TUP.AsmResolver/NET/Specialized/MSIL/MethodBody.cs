using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.PE;
using TUP.AsmResolver.PE.Readers;
namespace TUP.AsmResolver.NET.Specialized.MSIL
{
    public class MethodBody
    {
        NETMethodReader reader;

        internal MethodBody()
        {
        }

        public static MethodBody FromMethod(MethodDefinition methodDef)
        {
            MethodBody body = new MethodBody();
            body.Method = methodDef;
            body.reader = new NETMethodReader(methodDef.netheader.assembly.peImage, body);
            if (body.IsFat)
                body.reader.ReadFatMethod();
            return body;
        }

        public MethodDefinition Method { get; internal set; }

        public bool IsFat
        {
            get { return ((reader.signature & 3) == 3); }
        }

        public int HeaderSize
        {
            get
            {
                if (IsFat)
                    return 0xC;
                else
                    return 1;
            }
        }

        public uint CodeSize
        {
            get
            {
                if (IsFat)
                    return reader.codesize;
                else
                    return (uint)(reader.signature >> 2);
            }
            
        }

        public int MaxStack
        {
            get
            {
                if (IsFat)
                    return reader.maxstack;
                else
                    return 8;
            }
        }

        public uint LocalVarSig
        {
            get
            {
                if (IsFat)
                    return reader.localvarsig;
                else
                    return 0;
            }
        }

        public bool InitLocals
        {
            get
            {
                if (IsFat)
                    return (reader.fatsig & 0x10) == 0x10;
                else
                    return false;
            }
        }

        public bool HasExtraSections
        {
            get { return ((reader.fatsig & 8) == 8); }
        }

        public VariableDefinition[] Variables
        {
            get
            {
                if (IsFat)
                {
                    if (reader.vars == null)
                        reader.ReadVariables();
                    return reader.vars;
                }
                else
                    return null;
            }
                
        }

        public MSILInstruction[] Disassemble()
        {
            MSILDisassembler disassembler = new MSILDisassembler(this);
            return disassembler.Disassemble();
        }

        public MethodBodySection[] ExtraSections
        {
            get
            {
                if (HasExtraSections)
                {
                    if (reader.sections == null)
                        reader.ReadSections();

                    return reader.sections;
                }
                else
                    return null;
            }
        }
    }
}
