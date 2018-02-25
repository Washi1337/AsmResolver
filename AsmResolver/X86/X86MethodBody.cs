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
            body._instructions = new LazyValue<List<X86Instruction>>(() =>
            {
                var instructions = new List<X86Instruction>();
                var reader = context.Reader.CreateSubReader(context.Reader.StartPosition);
                var disassembler = new X86Disassembler(reader);
                while (reader.Position < reader.StartPosition + reader.Length)
                {
                    instructions.Add(disassembler.ReadNextInstruction());
                }

                return instructions;
            });
            return body;
        }

        private LazyValue<List<X86Instruction>> _instructions;

        public X86MethodBody()
        {
            _instructions = new LazyValue<List<X86Instruction>>(new List<X86Instruction>());
        }
        
        /// <summary>
        /// Gets the instructions in the method body.
        /// </summary>
        public IList<X86Instruction> Instructions
        {
            get { return _instructions.Value; }
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
