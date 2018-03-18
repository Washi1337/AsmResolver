using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.Net;
using AsmResolver.Net.Emit;

namespace AsmResolver.X86
{
    /// <summary>
    /// Represents a chunk of x86 instructions.
    /// </summary>
    public class X86MethodBody : MethodBody
    {
        public static X86MethodBody FromReader(IBinaryStreamReader reader)
        {
            var body = new X86MethodBody();
            
            var disassembler = new X86Disassembler(reader);
            while (reader.Position < reader.StartPosition + reader.Length)
                body.Instructions.Add(disassembler.ReadNextInstruction());

            return body;
        }

        public X86MethodBody()
        {
            Instructions = new List<X86Instruction>();
        }
        
        /// <summary>
        /// Gets the instructions in the method body.
        /// </summary>
        public IList<X86Instruction> Instructions
        {
            get;
            private set;
        }

        public override uint GetCodeSize()
        {
            return (uint) Instructions.Sum(x => x.ComputeSize());
        }

        public override FileSegment CreateRawMethodBody(MetadataBuffer buffer)
        {
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryStreamWriter(stream);
                var assembler = new X86Assembler(writer);
                foreach (var instruction in Instructions)
                    assembler.Write(instruction);
                return new DataSegment(stream.ToArray());
            }
        }
    }
}
