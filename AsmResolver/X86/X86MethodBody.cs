using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.X86
{
    /// <summary>
    /// Represents a chunk of x86 instructions.
    /// </summary>
    public class X86MethodBody : MethodBody
    {
        public static X86MethodBody FromReadingContext(ReadingContext context)
        {
            var body = new X86MethodBody();
            body._readingContext = context.CreateSubContext(context.Reader.StartPosition);
            return body;
        }

        private List<X86Instruction> _instructions;
        private ReadingContext _readingContext;
        
        /// <summary>
        /// Gets the instructions in the method body.
        /// </summary>
        public IList<X86Instruction> Instructions
        {
            get
            {
                if (_instructions != null)
                    return _instructions;
                _instructions = new List<X86Instruction>();
                if (_readingContext != null)
                {
                    var disassembler = new X86Disassembler(_readingContext.Reader);
                    while (_readingContext.Reader.Position
                           < _readingContext.Reader.StartPosition + _readingContext.Reader.Length)
                    {
                        _instructions.Add(disassembler.ReadNextInstruction());
                    }
                }
                return _instructions;
            }
        }

        public override uint GetPhysicalLength()
        {
            return (uint) Instructions.Sum(x => x.ComputeSize());
        }

        public override void Write(WritingContext context)
        {
            var assembler = new X86Assembler(context.Writer);
            foreach (var instruction in Instructions)
                assembler.Write(instruction);
        }
    }
}
