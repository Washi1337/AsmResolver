using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.PE;
using TUP.AsmResolver.PE.Readers;
namespace TUP.AsmResolver.NET.Specialized.MSIL
{
    public class MethodBody : IImageProvider, ICacheProvider
    {
        internal MethodBody()
        {
        }

        private NETMethodReader _reader;
        private MSILInstruction[] _instructions;

        public static MethodBody FromMethod(MethodDefinition methodDef)
        {
            MethodBody body = new MethodBody();
            body.Method = methodDef;
            body._reader = new NETMethodReader(methodDef._netheader._assembly._peImage, body);
            if (body.IsFat)
                body._reader.ReadFatMethod();
            return body;
        }

        public MethodDefinition Method { get; internal set; }

        public bool IsFat
        {
            get { return ((_reader.signature & 3) == 3); }
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
                    return _reader.codesize;
                else
                    return (uint)(_reader.signature >> 2);
            }
            
        }

        public int MaxStack
        {
            get
            {
                if (IsFat)
                    return _reader.maxstack;
                else
                    return 8;
            }
        }

        public uint LocalVarSig
        {
            get
            {
                if (IsFat)
                    return _reader.localvarsig;
                else
                    return 0;
            }
        }

        public bool InitLocals
        {
            get
            {
                if (IsFat)
                    return (_reader.fatsig & 0x10) == 0x10;
                else
                    return false;
            }
        }

        public bool HasExtraSections
        {
            get { return ((_reader.fatsig & 8) == 8); }
        }

        public bool HasVariables
        {
            get
            {
                return IsFat && (Variables != null && Variables.Length > 0);
            }
        }

        public VariableDefinition[] Variables
        {
            get
            {
                if (IsFat)
                {
                    if (_reader.vars == null)
                        _reader.ReadVariables();
                    return _reader.vars;
                }
                else
                    return null;
            }
                
        }

        public MethodBodySection[] ExtraSections
        {
            get
            {
                if (HasExtraSections)
                {
                    if (_reader.sections == null)
                        _reader.ReadSections();

                    return _reader.sections;
                }
                else
                    return null;
            }
        }

        public bool HasImage
        {
            get 
            {
                return Method.HasImage;
            }
        }

        public MSILInstruction[] Instructions
        {
            get
            {
                if (_instructions == null)
                {
                    MSILDisassembler disassembler = new MSILDisassembler(this);
                    _instructions = disassembler.Disassemble();
                }
                return _instructions;
            }
        }

        public void ClearCache()
        {
            _instructions = null;
        }

        public void LoadCache()
        {
            _instructions = Instructions;
            _reader.vars = Variables;
        }
    }
}
