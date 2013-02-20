using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized.MSIL
{
    public class MSILOpCode
    {
        internal MSILOpCode(string name, byte[] bytes, OperandType operandType)
        {
            Name = name;
            Bytes = bytes;
            OperandType = operandType;
        }

        public string Name { get; internal set; }
        public byte[] Bytes { get; internal set; }
        public OperandType OperandType { get; internal set; }
        public MSILCode Code
        {
            get
            {
                foreach (var field in typeof(MSILCode).GetFields())
                    if (field.Name.ToLower() == Name.ToLower().Replace(".", "_"))
                        return (MSILCode)field.GetValue(null);
                return MSILCode.Nop;
            }
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
