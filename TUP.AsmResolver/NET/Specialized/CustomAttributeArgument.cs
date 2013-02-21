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
            Fixed = true;
            Value = value;
            Name = string.Empty;
        }
        public CustomAttributeArgument(object value, string name)
        {
            Fixed = false;
            Value = value;
            Name = name;
        }
        public bool Fixed { get; internal set; }
        public string Name { get; internal set; }
        public object Value { get; internal set; }

        public override string ToString()
        {
            return Name + " = " + (Value != null ? Value.ToString() : "null");
        }

    }
}
