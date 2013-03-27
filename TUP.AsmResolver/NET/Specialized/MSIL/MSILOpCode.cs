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
        private MSILCode? code;

        public string Name { get; internal set; }
        public byte[] Bytes { get; internal set; }
        public OperandType OperandType { get; internal set; }
        public MSILCode Code
        {
            get
            {
                if (!code.HasValue)
                {
                    foreach (var field in typeof(MSILCode).GetFields())
                        if (field.Name.ToLower() == Name.ToLower().Replace(".", "_"))
                        {
                            code = (MSILCode)field.GetValue(null);
                            break;
                        }
                }

                if (!code.HasValue)
                    return MSILCode.Nop;
                
                return code.Value;
            }
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
