using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class MethodSignature : IMemberSignature
    {
        public MethodCallingConvention CallingConvention { get; internal set; }

        public bool ExplicitThis { get; internal set; }

        public bool HasThis { get; internal set; }

        public ParameterReference[] Parameters { get; internal set; }

        public TypeReference ReturnType { get; internal set; }

        public bool HasParameters { get { return Parameters != null && Parameters.Length > 0; } }

        public string GetParameterString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("(");
            if (HasParameters)
            {
                for (int i = 0; i < Parameters.Length; i++)
                    builder.Append(Parameters[i].ParameterType.FullName + (i == Parameters.Length - 1 ? "" : ", "));
            }
            builder.Append(")");
            return builder.ToString();
        }

    }
}
