using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;


namespace TUP.AsmResolver.ASM
{
    /// <summary>
    /// Represents a 32-bit assembly opcode of an instruction.
    /// </summary>
    public class x86OpCode
    {
        internal x86OpCode()
        {
        }

        internal x86OpCode(string name, byte[] opcodebytes, int operandlength, x86OperandType type)
        {
            this._name = name;
            this._originalBytes = opcodebytes;
            this._opcodeBytes = opcodebytes;
            this._operandType = type;
            this._operandLength = operandlength;
            _variableByteIndex = -1;
        }

        internal x86OpCode(string name, byte[] opcodebytes, int operandlength, x86OperandType type, int variableByteIndex)
        {
            this._name = name;
            this._originalBytes = opcodebytes;
            this._opcodeBytes = opcodebytes;
            this._operandType = type;
            this._operandLength = operandlength;
            this._variableByteIndex = variableByteIndex;
            
        }

        internal string _name;
        internal byte[] _opcodeBytes;
        internal byte[] _originalBytes;
        internal x86OperandType _operandType;
        internal int _operandLength;
        internal int _variableByteIndex;
        internal bool _isValid;

        /// <summary>
        /// Gets the assembly opcode name.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            internal set
            {
                _name = value;
            }
        }

        /// <summary>
        /// Gets the opcode bytes of the current opcode.
        /// </summary>
        public byte[] OpCodeBytes
        {
            get { return _opcodeBytes; }
        }
        
        /// <summary>
        /// Gets a value indicating the operand size.
        /// </summary>
        public int OperandLength
        {
            get { return _operandLength; }
        }

        /// <summary>
        /// Gets the corresponding operand type.
        /// </summary>
        public x86OperandType OperandType
        {
            get
            {
                return _operandType;
            }
        }

        public bool IsValid
        {
            get
            {
                return _isValid;
            }
        }

        /// <summary>
        /// Returns the opcode string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            //System.Diagnostics.Debugger.Launch();
            //if (Name.Contains("ADD %"))
            //    System.Diagnostics.Debugger.Break();
            string res = _name;

            

            return res;
        }

        /// <summary>
        /// Returns a boolean value if the current opcode is based on the given opcode.
        /// </summary>
        /// <param name="code">The Opcode to compare with.</param>
        /// <returns></returns>
        public bool IsBasedOn(x86OpCode code)
        {
            return code._originalBytes == this._originalBytes;
        }

        internal static x86OpCode Create(x86OpCode code)
        {
            // copies an opcode. we don't want to change the opcode bytes of the list.
            x86OpCode newCode = new x86OpCode();
            newCode._name = code._name;
            newCode._originalBytes = code._originalBytes;
            newCode._opcodeBytes = code._opcodeBytes;
            newCode._operandType = code._operandType;
            newCode._operandLength = code._operandLength;
            newCode._variableByteIndex = code._variableByteIndex;
            return newCode;
        }

        public x86OperandType GetNormalOperandType()
        {
            return OperandType & x86OperandType.NormalOperandMask;
        }

        public x86OperandType GetRegisterOperandType()
        {
            return OperandType & x86OperandType.RegisterOperandMask;
        }

        public x86OperandType GetOverrideOperandType()
        {
            return OperandType & x86OperandType.OverridingOperandMask;
        }

    }


   

}
