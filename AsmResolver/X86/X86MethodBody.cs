using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.Net;
using AsmResolver.Net.Builder;

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
                var instructions = new List<X86Instruction>();
                if (_readingContext != null)
                {
                    var disassembler = new X86Disassembler(_readingContext.Reader);
                    while (_readingContext.Reader.Position
                           < _readingContext.Reader.StartPosition + _readingContext.Reader.Length)
                    {
                        instructions.Add(disassembler.ReadNextInstruction());
                    }
                }

                return _instructions = instructions;
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

        public override RvaDataSegment CreateDataSegment(MetadataBuffer buffer)
        {
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryStreamWriter(stream);
                var assembler = new X86Assembler(writer);
                foreach (var instruction in Instructions)
                    assembler.Write(instruction);
                return new RvaDataSegment(stream.ToArray());
            }
        }
    }
}
