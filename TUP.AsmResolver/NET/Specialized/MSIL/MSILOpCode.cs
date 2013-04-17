using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized.MSIL
{
    public class MSILOpCode
    {
        internal MSILOpCode(string name, byte[] bytes, OperandType operandType, StackBehaviour stackBehaviour)
        {
            Name = name;
            Bytes = bytes;
            OperandType = operandType;
            StackBehaviour = stackBehaviour;
        }

        private MSILCode? code;

        public string Name { get; internal set; }

        public byte[] Bytes { get; internal set; }

        public OperandType OperandType { get; internal set; }

        public StackBehaviour StackBehaviour { get; internal set; }

        public StackBehaviour StackBehaviourPush
        {
            get
            {
                return (StackBehaviour)((ushort)StackBehaviour & 0xFF00);
            }
        }

        public StackBehaviour StackBehaviourPop
        {
            get
            {
                return (StackBehaviour)((ushort)StackBehaviour & 0x00FF);
            }
        }
        
        public MSILCode Code
        {
            get
            {
                if (!code.HasValue)
                {
                    string name = Name.ToLower();
                    if (name[name.Length - 1] == '.')
                        name = name.Substring(0, name.Length - 1);
                                        
                    name = name.Replace(".", "_");

                    foreach (var field in typeof(MSILCode).GetFields())
                    {
                        
                        if (field.Name.ToLower() == name)
                        {
                            code = (MSILCode)field.GetValue(null);
                            break;
                        }
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
