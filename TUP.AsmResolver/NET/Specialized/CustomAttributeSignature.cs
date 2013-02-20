using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class CustomAttributeSignature
    {

        public CustomAttributeSignature(CustomAttributeArgument[] fixedArgs, CustomAttributeArgument[] namedArgs)
        {
            FixedArguments = fixedArgs;
            NamedArguments = namedArgs;
        }

        public CustomAttributeArgument[] FixedArguments { get; internal set; }
        public CustomAttributeArgument[] NamedArguments { get; internal set; }

    }
}
