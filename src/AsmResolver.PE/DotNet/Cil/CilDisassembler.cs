// AsmResolver - Executable file format inspection library 
// Copyright (C) 2016-2019 Washi
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA

using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Provides a mechanism for decoding CIL instructions from an input stream.
    /// </summary>
    public class CilDisassembler
    {
        private int _currentOffset;
        
        /// <summary>
        /// Creates a new CIL disassembler using the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream to read the code from.</param>
        public CilDisassembler(IBinaryStreamReader reader)
        {
            Reader = reader;
        }
        
        /// <summary>
        /// Gets the input stream that is used to read the raw bytes.
        /// </summary>
        public IBinaryStreamReader Reader
        {
            get;
        }

        /// <summary>
        /// Reads all instructions from the input stream.
        /// </summary>
        /// <returns>The instructions.</returns>
        public IList<CilInstruction> ReadAllInstructions()
        {
            var instructions = new List<CilInstruction>();
            while (Reader.FileOffset < Reader.StartPosition + Reader.Length)
                instructions.Add(ReadInstruction());
            return instructions;
        }
        
        /// <summary>
        /// Reads the next instruction from the input stream.
        /// </summary>
        /// <returns>The instruction.</returns>
        public CilInstruction ReadInstruction()
        {
            uint start = Reader.FileOffset;
            
            var code = ReadOpCode();
            var operand = ReadOperand(code.OperandType);
            var result =  new CilInstruction(_currentOffset, code, operand);

            _currentOffset += (int) (Reader.FileOffset - start);
            
            return result;
        }

        private CilOpCode ReadOpCode()
        {
            byte op = Reader.ReadByte();
            return op == 0xFE 
                ? CilOpCodes.MultiByteOpCodes[Reader.ReadByte()] 
                : CilOpCodes.SingleByteOpCodes[op];
        }

        private object ReadOperand(CilOperandType operandType)
        {
            return operandType switch
            {
                CilOperandType.InlineNone => (object) null,
                CilOperandType.ShortInlineI => Reader.ReadSByte(),
                CilOperandType.ShortInlineBrTarget => Reader.ReadSByte(),
                CilOperandType.ShortInlineVar => Reader.ReadByte(),
                CilOperandType.ShortInlineArgument => Reader.ReadByte(),
                CilOperandType.InlineVar => Reader.ReadUInt16(),
                CilOperandType.InlineArgument => Reader.ReadUInt16(),
                CilOperandType.InlineI => Reader.ReadInt32(),
                CilOperandType.InlineBrTarget => Reader.ReadInt32(),
                CilOperandType.ShortInlineR => Reader.ReadSingle(),
                CilOperandType.InlineI8 => Reader.ReadInt64(),
                CilOperandType.InlineR => Reader.ReadDouble(),
                CilOperandType.InlineField => new MetadataToken(Reader.ReadUInt32()),
                CilOperandType.InlineMethod => new MetadataToken(Reader.ReadUInt32()),
                CilOperandType.InlineSig => new MetadataToken(Reader.ReadUInt32()),
                CilOperandType.InlineString => new MetadataToken(Reader.ReadUInt32()),
                CilOperandType.InlineTok => new MetadataToken(Reader.ReadUInt32()),
                CilOperandType.InlineType => new MetadataToken(Reader.ReadUInt32()),
                CilOperandType.InlinePhi => throw new NotSupportedException(),
                CilOperandType.InlineSwitch => ReadSwitchTable(),
                _ => throw new ArgumentOutOfRangeException(nameof(operandType), operandType, null)
            };
        }

        private IList<int> ReadSwitchTable()
        {
            int count = Reader.ReadInt32();
            
            var offsets = new List<int>(count);
            for (int i = 0; i < count; i++)
                offsets.Add(Reader.ReadInt32());
            
            return offsets;
        }
        
    }
}