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

        #region Constructors


        internal x86OpCode()
        {
        }
        internal x86OpCode(string name, byte[] opcodebytes, int operandlength, x86OperandType type)
        {
            this.name = name;
            this.originalbytes = opcodebytes;
            this.opcodebytes = opcodebytes;
            this.operandtype = type;
            this.operandlength = operandlength;
            variableByteIndex = -1;
        }
        internal x86OpCode(string name, byte[] opcodebytes, int operandlength, x86OperandType type, int variableByteIndex)
        {
            this.name = name;
            this.originalbytes = opcodebytes;
            this.opcodebytes = opcodebytes;
            this.operandtype = type;
            this.operandlength = operandlength;
            this.variableByteIndex = variableByteIndex;
            
        }


        #endregion

        #region vars
     

        internal string name;
        internal byte[] opcodebytes;
        internal byte[] originalbytes;
        internal x86OperandType operandtype;
        internal int operandlength;
        internal int variableByteIndex;
        
        #endregion

        #region Properties

        /// <summary>
        /// Gets the assembly opcode name.
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            internal set
            {
                name = value;
            }
        }

        /// <summary>
        /// Gets the opcode bytes of the current opcode.
        /// </summary>
        public byte[] OpCodeBytes
        {
            get { return opcodebytes; }
        }
        
        /// <summary>
        /// Gets a value indicating the operand size.
        /// </summary>
        public int OperandLength
        {
            get { return operandlength; }
        }

        /// <summary>
        /// Gets the corresponding operand type.
        /// </summary>
        public x86OperandType OperandType
        {
            get
            {
                return operandtype;
            }
        }

        #endregion

        #region Methods
        

        /// <summary>
        /// Returns the opcode string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            //System.Diagnostics.Debugger.Launch();
            //if (Name.Contains("ADD %"))
            //    System.Diagnostics.Debugger.Break();
            string res = name;

            

            return res;
        }

        public override bool Equals(object obj)
        {
            if (obj is x86OpCode)
            {
                return IsBasedOn((x86OpCode)obj);
            }
            else
                return false;
        }


        /// <summary>
        /// Returns a boolean value if the current opcode is based on the given opcode.
        /// </summary>
        /// <param name="code">The Opcode to compare with.</param>
        /// <returns></returns>
        public bool IsBasedOn(x86OpCode code)
        {
            return code.originalbytes == this.originalbytes;
        }

        internal static x86OpCode Create(x86OpCode code)
        {
            // copies an opcode. we don't want to change the opcode bytes of the list.
            x86OpCode newCode = new x86OpCode();
            newCode.name = code.name;
            newCode.originalbytes = code.originalbytes;
            newCode.opcodebytes = code.opcodebytes;
            newCode.operandtype = code.operandtype;
            newCode.operandlength = code.operandlength;
            newCode.variableByteIndex = code.variableByteIndex;
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
        #endregion

    }


   

}
