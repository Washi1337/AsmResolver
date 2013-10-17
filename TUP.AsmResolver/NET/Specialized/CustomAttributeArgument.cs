using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class CustomAttributeArgument
    {
        public CustomAttributeArgument(object value)
        {
            ArgumentType = CustomAttributeArgumentType.Fixed;
            Value = value;
            Name = string.Empty;
        }
        public CustomAttributeArgument(object value, string name, CustomAttributeArgumentType type)
        {
            ArgumentType = type;
            Value = value;
            Name = name;
        }

        public string Name { get; internal set; }
        public CustomAttributeArgumentType ArgumentType { get; internal set; }
        public object Value { get; internal set; }

        public override string ToString()
        {
            return Name + " = " + (Value != null ? Value.ToString() : "null");
        }
    }

    public enum CustomAttributeArgumentType
    {
        Fixed,
        NamedField,
        NamedProperty,
    }
}
