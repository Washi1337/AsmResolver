using System;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.DotNet.Signatures
{
    public sealed class BoxedArgument
    {
        public BoxedArgument(TypeSignature type, object value)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
        
        public TypeSignature Type
        {
            get;
        }

        public object Value
        {
            get;
        }
    }
}